using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace AudioPlayer;

public partial class Form1 : Form
{
    private static readonly SelectionOption<VisualizerMode>[] StandardVisualizerModes =
    [
        new("Spectrum", VisualizerMode.Spectrum),
        new("Mirror Spectrum", VisualizerMode.MirrorSpectrum),
        new("Waveform", VisualizerMode.Waveform)
    ];
    private static readonly SelectionOption<VisualizerMode> SpinningDiskVisualizerMode =
        new("Spinning Disk", VisualizerMode.SpinningDisk);

    private readonly AudioEngine engine = new();
    private readonly string? startupPath;

    private AppSettings appSettings;
    private ThemePalette themePalette;
    private bool isUpdatingSeekBar;
    private bool isApplyingSettings;
    private bool showRemainingTime;
    private bool isMuted;
    private float preMuteVolume = 0.85f;
    private AudioTrackInfo? displayedArtworkTrack;
    private Image? visualizerAlbumArt;
    private long nextVisualizerCycleTick;

    public Form1(string? startupPath = null)
    {
        this.startupPath = startupPath;
        appSettings = AppSettingsStore.Load();
        themePalette = ThemePalette.Create(appSettings.ThemeMode, appSettings.ThemeAccent);

        InitializeComponent();
        nextVisualizerCycleTick = Environment.TickCount64 + (long)VisualizerAutoCycleInterval.TotalMilliseconds;
        ApplyTheme();
        PopulateSettings();
        WireFileDrop(this);
        UpdateUiState();
    }

    private Color WindowBackColor => themePalette.WindowBackColor;
    private Color SurfaceBackColor => themePalette.SurfaceBackColor;
    private Color SurfaceAltBackColor => themePalette.SurfaceAltBackColor;
    private Color SurfaceRaisedColor => themePalette.SurfaceRaisedColor;
    private Color TextPrimaryColor => themePalette.TextPrimaryColor;
    private Color TextSecondaryColor => themePalette.TextSecondaryColor;
    private Color TextSoftColor => themePalette.TextSoftColor;
    private Color TextMutedColor => themePalette.TextMutedColor;
    private Color AccentPrimaryColor => themePalette.AccentPrimaryColor;
    private Color AccentSecondaryColor => themePalette.AccentSecondaryColor;
    private Color AccentSoftColor => themePalette.AccentSoftColor;
    private Color AccentContrastColor => themePalette.AccentContrastColor;
    private Color DangerColor => themePalette.DangerColor;
    private Color DangerTextColor => themePalette.DangerTextColor;
    private Color StatusBorderColor => themePalette.BorderStrongColor;
    private TimeSpan VisualizerAutoCycleInterval => TimeSpan.FromSeconds(appSettings.VisualizerCycleSeconds);

    // ── Load ─────────────────────────────────────────────────────────────

    private void Form1_Load(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(startupPath))
            return;

        if (File.Exists(startupPath))
        {
            LoadAudioFile(startupPath, appSettings.AutoPlayOnOpen);
            return;
        }

        ShowError(
            $"The startup file could not be found:{Environment.NewLine}{Environment.NewLine}{startupPath}",
            "Open Error");
    }

    // ── Keyboard shortcuts ────────────────────────────────────────────────

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        // Don't steal navigation keys from open combo boxes
        if (ActiveControl is ComboBox)
            return base.ProcessCmdKey(ref msg, keyData);

        switch (keyData)
        {
            case Keys.Space:
                btnPlayPause_Click(this, EventArgs.Empty);
                return true;

            case Keys.Escape:
                if (engine.IsLoaded) { engine.Stop(); UpdateUiState(); }
                return true;

            case Keys.Left:
                SeekRelative(-5);
                return true;

            case Keys.Right:
                SeekRelative(5);
                return true;

            case Keys.Shift | Keys.Left:
                SeekRelative(-30);
                return true;

            case Keys.Shift | Keys.Right:
                SeekRelative(30);
                return true;

            case Keys.Up:
                AdjustVolume(5);
                return true;

            case Keys.Down:
                AdjustVolume(-5);
                return true;

            case Keys.M:
                btnMute_Click(this, EventArgs.Empty);
                return true;

            case Keys.Control | Keys.O:
                OpenAudioFile(startPlayback: true);
                return true;

            case Keys.Control | Keys.Oemcomma:
                ShowSettingsDialog();
                return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void SeekRelative(float deltaSec)
    {
        if (!engine.IsLoaded) return;
        engine.Seek(Math.Clamp(engine.GetPosition() + deltaSec, 0, engine.GetLength()));
        UpdateUiState();
    }

    private void AdjustVolume(int delta)
    {
        trackBarVolume.Value = Math.Clamp(trackBarVolume.Value + delta, 0, 100);
        engine.Volume = trackBarVolume.Value / 100f;
        UpdateUiState();
    }

    // ── Button handlers ───────────────────────────────────────────────────

    private void btnPlayPause_Click(object sender, EventArgs e)
    {
        if (!engine.IsLoaded)
        {
            OpenAudioFile(startPlayback: true);
            return;
        }

        engine.Toggle();
        UpdateUiState();
    }

    private void btnStop_Click(object sender, EventArgs e)
    {
        engine.Stop();
        UpdateUiState();
    }

    private void btnMute_Click(object sender, EventArgs e)
    {
        if (isMuted)
        {
            isMuted = false;
            trackBarVolume.Value = Math.Max(1, (int)(preMuteVolume * 100));
        }
        else
        {
            preMuteVolume = Math.Max(0.01f, trackBarVolume.Value / 100f);
            isMuted = true;
            trackBarVolume.Value = 0;
        }

        engine.Volume = trackBarVolume.Value / 100f;
        UpdateUiState();
    }

    private void btnDefaultApp_Click(object sender, EventArgs e)
    {
        try
        {
            DefaultAppRegistrar.RegisterCurrentUser();
            DefaultAppRegistrar.OpenDefaultAppsSettings();

            MessageBox.Show(
                this,
                "Audio Player has been registered for the current Windows user. Choose Audio Player in the Default Apps window that just opened to make it the handler for supported audio files.",
                "Default App Registration",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            ShowError(
                $"Audio Player could not register itself for Windows file associations.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                "Registration Error");
        }
    }

    // ── Settings handlers ─────────────────────────────────────────────────

    private void cmbVisualizerMode_SelectedIndexChanged(object sender, EventArgs e)
    {
        ApplyVisualizerSettings();
    }

    private void cmbSampleRate_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (isApplyingSettings || cmbSampleRate.SelectedItem is not SelectionOption<int> option)
            return;

        appSettings.PreferredSampleRate = option.Value;
        engine.SetPreferredSampleRate(option.Value);
        SaveAppSettings();
        UpdateUiState();
    }

    private void chkPeakHold_CheckedChanged(object sender, EventArgs e)
    {
        ApplyVisualizerSettings();
    }

    private void trackBarSensitivity_Scroll(object sender, EventArgs e)
    {
        ApplyVisualizerSettings();
    }

    private void trackBarVolume_Scroll(object sender, EventArgs e)
    {
        // If user manually moves volume slider, exit mute state
        if (isMuted && trackBarVolume.Value > 0)
            isMuted = false;

        engine.Volume = trackBarVolume.Value / 100f;
        UpdateUiState();
    }

    private void trackBarSeek_Scroll(object sender, EventArgs e)
    {
        if (isUpdatingSeekBar)
            return;

        engine.Seek(trackBarSeek.Value);
        UpdateUiState();
    }

    // ── Time label click ──────────────────────────────────────────────────

    private void lblCurrentTime_Click(object sender, EventArgs e)
    {
        showRemainingTime = !showRemainingTime;
        UpdateSeekBar();
    }

    // ── Timer ─────────────────────────────────────────────────────────────

    private void timer1_Tick(object sender, EventArgs e)
    {
        UpdateSeekBar();
        UpdateVisualizer();
        CycleVisualizerIfDue();
        UpdateUiState();
    }

    // ── Drag & drop ───────────────────────────────────────────────────────

    private void Form1_DragEnter(object? sender, DragEventArgs e)
    {
        var hasFiles = e.Data?.GetDataPresent(DataFormats.FileDrop) == true;
        e.Effect = hasFiles ? DragDropEffects.Copy : DragDropEffects.None;
    }

    private void Form1_DragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetData(DataFormats.FileDrop) is not string[] files || files.Length == 0)
            return;

        LoadAudioFile(files[0], appSettings.AutoPlayOnOpen);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        timer1.Stop();
        DisposeDisplayedArtwork();
        visualizerAlbumArt?.Dispose();
        engine.Dispose();
        base.OnFormClosed(e);
    }

    // ── Theme / Init ──────────────────────────────────────────────────────

    private void ApplyTheme()
    {
        themePalette = ThemePalette.Create(appSettings.ThemeMode, appSettings.ThemeAccent);

        BackColor = WindowBackColor;
        ForeColor = TextPrimaryColor;
        ApplyAppIcon();

        statusStrip1.BackColor = WindowBackColor;
        statusStrip1.ForeColor = TextSecondaryColor;
        statusStrip1.Renderer = new ThemeStatusStripRenderer(WindowBackColor, StatusBorderColor);
        menuStrip1.BackColor = WindowBackColor;
        menuStrip1.ForeColor = TextPrimaryColor;
        menuStrip1.Renderer = new ThemeMenuStripRenderer(WindowBackColor, SurfaceAltBackColor, SurfaceBackColor, StatusBorderColor);

        toolStripOutputLabel.ForeColor = TextSecondaryColor;
        toolStripHintLabel.ForeColor = TextMutedColor;

        picAlbumArt.BackColor = SurfaceBackColor;
        lblNowPlaying.ForeColor = TextPrimaryColor;
        lblTrackInfo.ForeColor = TextSecondaryColor;
        lblCurrentTime.ForeColor = TextSoftColor;
        lblDuration.ForeColor = TextMutedColor;
        lblVolumeValue.ForeColor = TextSecondaryColor;

        lblVisualizerModeCaption.ForeColor = TextMutedColor;
        lblSampleRateCaption.ForeColor = TextMutedColor;
        lblSensitivityCaption.ForeColor = TextMutedColor;
        ThemeControlStyler.ApplyCheckBoxTheme(chkPeakHold, themePalette);

        ThemeControlStyler.ApplyComboBoxTheme(cmbVisualizerMode, themePalette);
        ThemeControlStyler.ApplyComboBoxTheme(cmbSampleRate, themePalette);
        ThemeControlStyler.ApplySliderTheme(trackBarSeek, themePalette);
        ThemeControlStyler.ApplySliderTheme(trackBarVolume, themePalette);
        ThemeControlStyler.ApplySliderTheme(trackBarSensitivity, themePalette);
        toolTip1.SetToolTip(
            cmbVisualizerMode,
            GetVisualizerCycleToolTip());
        ApplyMenuTheme(menuStrip1.Items);
        visualizerControl.ApplyTheme(themePalette);
        lyricsView.ApplyTheme(themePalette);

        transportLayout.Margin = new Padding(0, 14, 0, 0);
        leftButtonsPanel.Padding = Padding.Empty;
        rightControlsPanel.Padding = Padding.Empty;
        settingsPanel.Padding = Padding.Empty;
        settingsPanel.Margin = Padding.Empty;
        settingsPanel.Visible = false;

        ThemeControlStyler.ApplyPrimaryButtonTheme(btnPlayPause, themePalette, AccentPrimaryColor);
        btnPlayPause.Pill = false;
        btnPlayPause.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold, GraphicsUnit.Point);
        btnPlayPause.Size = new Size(158, 40);

        ThemeControlStyler.ApplyGhostButtonTheme(btnStop, themePalette, DangerColor);
        btnStop.Font = new Font("Segoe UI Semibold", 8.75F, FontStyle.Bold, GraphicsUnit.Point);
        btnStop.Margin = new Padding(0, 0, 10, 0);
        btnStop.Size = new Size(74, 38);

        ThemeControlStyler.ApplyGhostButtonTheme(btnMute, themePalette, AccentSoftColor);
        btnMute.Font = new Font("Segoe UI Semibold", 8.75F, FontStyle.Bold, GraphicsUnit.Point);
        btnMute.Margin = new Padding(0, 0, 12, 0);
        btnMute.Size = new Size(74, 38);

        ThemeControlStyler.ApplyGhostButtonTheme(btnDefaultApp, themePalette, AccentSoftColor);
        btnDefaultApp.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
        btnDefaultApp.Margin = new Padding(18, 0, 0, 0);
        btnDefaultApp.Size = new Size(136, 32);

        trackBarVolume.Margin = new Padding(0, 0, 12, 0);
        trackBarVolume.Size = new Size(148, 38);
        trackBarSensitivity.Size = new Size(120, 28);
        ApplyInformationVisibility();
    }

    private void ApplyAppIcon()
    {
        var extractedIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        if (extractedIcon is null)
            return;

        using (extractedIcon)
        {
            Icon = (Icon)extractedIcon.Clone();
        }
    }

    private void PopulateSettings()
    {
        ApplyStoredSettings(appSettings.DefaultVisualizer);
    }

    private void ApplyStoredSettings(VisualizerMode currentVisualizer)
    {
        var normalizedSettings = AppSettingsStore.Normalize(appSettings.Clone());
        appSettings = normalizedSettings;

        isApplyingSettings = true;
        try
        {
            RefreshVisualizerModeOptions(currentVisualizer);

            var sampleRateOptions = GetSampleRateOptions();
            cmbSampleRate.BeginUpdate();
            try
            {
                cmbSampleRate.Items.Clear();
                cmbSampleRate.Items.AddRange(sampleRateOptions.Select(static option => (object)option).ToArray());
                SelectComboValue(cmbSampleRate, appSettings.PreferredSampleRate);
            }
            finally
            {
                cmbSampleRate.EndUpdate();
            }

            trackBarSensitivity.Value = appSettings.VisualizerSensitivity;
            trackBarVolume.Value = appSettings.DefaultVolume;
            chkPeakHold.Checked = appSettings.PeakHold;
            preMuteVolume = Math.Max(0.01f, trackBarVolume.Value / 100f);
            isMuted = trackBarVolume.Value == 0;
            engine.SetPreferredSampleRate(appSettings.PreferredSampleRate);
            engine.Volume = trackBarVolume.Value / 100f;
        }
        finally
        {
            isApplyingSettings = false;
        }

        ApplyVisualizerSettings();
        ApplyInformationVisibility();
        ResetVisualizerCycleDeadline();
    }

    private void ApplyVisualizerSettings()
    {
        if (cmbVisualizerMode.SelectedItem is SelectionOption<VisualizerMode> option)
            visualizerControl.Mode = option.Value;

        visualizerControl.ShowPeaks = chkPeakHold.Checked;
        visualizerControl.Sensitivity = trackBarSensitivity.Value / 100f;

        if (!isApplyingSettings)
            ResetVisualizerCycleDeadline();
    }

    private void RefreshVisualizerModeOptions(VisualizerMode? preferredMode = null)
    {
        var selectedMode = preferredMode
            ?? (cmbVisualizerMode.SelectedItem as SelectionOption<VisualizerMode>)?.Value
            ?? VisualizerMode.MirrorSpectrum;
        var availableModes = GetAvailableVisualizerModeOptions();

        if (!availableModes.Any(option => option.Value == selectedMode))
        {
            selectedMode = availableModes.Any(option => option.Value == VisualizerMode.MirrorSpectrum)
                ? VisualizerMode.MirrorSpectrum
                : availableModes[0].Value;
        }

        var wasApplyingSettings = isApplyingSettings;
        isApplyingSettings = true;
        cmbVisualizerMode.BeginUpdate();
        try
        {
            cmbVisualizerMode.Items.Clear();
            cmbVisualizerMode.Items.AddRange(availableModes.Select(static option => (object)option).ToArray());
            cmbVisualizerMode.SelectedIndex = Array.FindIndex(availableModes, option => option.Value == selectedMode);
        }
        finally
        {
            cmbVisualizerMode.EndUpdate();
            isApplyingSettings = wasApplyingSettings;
        }

        ApplyVisualizerSettings();
        ResetVisualizerCycleDeadline();
    }

    private SelectionOption<VisualizerMode>[] GetAvailableVisualizerModeOptions() =>
        visualizerAlbumArt is null
            ? StandardVisualizerModes.ToArray()
            : [.. StandardVisualizerModes, SpinningDiskVisualizerMode];

    private static SelectionOption<VisualizerMode>[] GetAllVisualizerModeOptions() =>
        [.. StandardVisualizerModes, SpinningDiskVisualizerMode];

    private static SelectionOption<int>[] GetSampleRateOptions() =>
    [
        new("Match source", 0),
        new("44.1 kHz", 44100),
        new("48 kHz", 48000),
        new("88.2 kHz", 88200),
        new("96 kHz", 96000)
    ];

    private static SelectionOption<int>[] GetCycleDurationOptions() =>
    [
        new("5 seconds", 5),
        new("8 seconds", 8),
        new("12 seconds", 12),
        new("20 seconds", 20),
        new("30 seconds", 30),
        new("45 seconds", 45),
        new("60 seconds", 60)
    ];

    private void CycleVisualizerIfDue()
    {
        if (!appSettings.EnableVisualizerAutoCycle)
            return;

        if (!engine.IsLoaded || cmbVisualizerMode.Items.Count <= 1)
            return;

        if (cmbVisualizerMode.DroppedDown)
        {
            ResetVisualizerCycleDeadline();
            return;
        }

        if (Environment.TickCount64 < nextVisualizerCycleTick)
            return;

        AdvanceVisualizerMode();
    }

    private void AdvanceVisualizerMode()
    {
        if (cmbVisualizerMode.Items.Count <= 1)
            return;

        cmbVisualizerMode.SelectedIndex = cmbVisualizerMode.SelectedIndex switch
        {
            < 0 => 0,
            var currentIndex => (currentIndex + 1) % cmbVisualizerMode.Items.Count
        };
    }

    private void ResetVisualizerCycleDeadline() =>
        nextVisualizerCycleTick = Environment.TickCount64 + (long)VisualizerAutoCycleInterval.TotalMilliseconds;

    private void ApplyMenuTheme(ToolStripItemCollection items)
    {
        foreach (ToolStripItem item in items)
        {
            item.ForeColor = TextPrimaryColor;

            if (item is ToolStripMenuItem menuItem)
                ApplyMenuTheme(menuItem.DropDownItems);
        }
    }

    private void WireFileDrop(Control control)
    {
        // Don't intercept mouse events on interactive controls — it breaks single-click.
        if (control is not (ButtonBase or ModernButton or ModernSlider or ComboBox or CheckBox or ListBox))
        {
            control.AllowDrop = true;
            control.DragEnter -= Form1_DragEnter;
            control.DragDrop  -= Form1_DragDrop;
            control.DragEnter += Form1_DragEnter;
            control.DragDrop  += Form1_DragDrop;
        }

        foreach (Control child in control.Controls)
            WireFileDrop(child);
    }

    // ── File operations ───────────────────────────────────────────────────

    private void OpenAudioFile(bool startPlayback)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = SupportedAudioFormats.OpenFileDialogFilter,
            Title = "Select an audio file"
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
            LoadAudioFile(dialog.FileName, startPlayback);
    }

    private void LoadAudioFile(string path, bool startPlayback)
    {
        if (!File.Exists(path))
        {
            ShowError(
                $"The selected file could not be found:{Environment.NewLine}{Environment.NewLine}{path}",
                "Open Error");
            return;
        }

        var loaded = false;

        try
        {
            engine.Load(path);
            visualizerControl.ClearFrame();
            trackBarSeek.Value = 0;
            loaded = true;
        }
        catch (Exception ex)
        {
            ShowError(
                $"Unable to load the selected audio file.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                "Load Error");
        }

        if (loaded && startPlayback)
            engine.Toggle();

        UpdateUiState();
    }

    // ── UI update ─────────────────────────────────────────────────────────

    private void UpdateSeekBar()
    {
        var length   = engine.GetLength();
        var position = engine.GetPosition();

        isUpdatingSeekBar = true;
        trackBarSeek.Maximum = Math.Max(1, (int)Math.Ceiling(Math.Max(length, 1)));
        trackBarSeek.Value = engine.IsLoaded
            ? Math.Clamp((int)Math.Floor(position), trackBarSeek.Minimum, trackBarSeek.Maximum)
            : 0;
        isUpdatingSeekBar = false;

        if (showRemainingTime && engine.IsLoaded)
            lblCurrentTime.Text = $"-{FormatTime(Math.Max(0, length - position))}";
        else
            lblCurrentTime.Text = FormatTime(position);

        lblDuration.Text = FormatTime(length);
    }

    private void UpdateVisualizer()
    {
        visualizerControl.UpdateFrame(engine.GetVisualizerFrame(), engine.IsPlaying);
    }

    private void UpdateUiState()
    {
        var track = engine.CurrentTrack;
        UpdateAlbumArt(track);

        // Play/Pause button text
        btnPlayPause.Text = track is null
            ? "Open Audio"
            : engine.IsPlaying ? "Pause" : "Play";
        btnPlayPause.AccentColor = track is null ? AccentSecondaryColor : AccentPrimaryColor;
        btnPlayPause.ForeColor = AccentContrastColor;

        // Stop button
        btnStop.Enabled     = track is not null;
        trackBarSeek.Enabled = track is not null;

        // Now-playing header
        lblNowPlaying.Text = track?.DisplayName ?? "Drop a file here or press Play";
        lblTrackInfo.Text = BuildTrackInfoText(track);
        ApplyInformationVisibility();
        SetLyricsVisible(track?.Lyrics is not null);
        lyricsView.UpdateState(track, engine.GetPosition());

        // Volume label & mute button
        if (isMuted || trackBarVolume.Value == 0)
        {
            lblVolumeValue.Text      = "Muted";
            lblVolumeValue.ForeColor = TextMutedColor;
            btnMute.Text             = "Unmute";
            btnMute.AccentColor      = DangerColor;
            btnMute.ForeColor        = DangerTextColor;
        }
        else
        {
            lblVolumeValue.Text      = $"{trackBarVolume.Value}%";
            lblVolumeValue.ForeColor = TextSecondaryColor;
            btnMute.Text             = "Mute";
            btnMute.AccentColor      = AccentSoftColor;
            btnMute.ForeColor        = TextSecondaryColor;
        }

        // Status bar
        toolStripStatusLabel.Text = track is null
            ? "Ready"
            : engine.IsPlaying ? "Playing"
            : Math.Abs(engine.GetPosition()) < 0.01f ? "Loaded" : "Paused";

        toolStripStatusLabel.ForeColor = engine.IsPlaying
            ? AccentPrimaryColor
            : TextMutedColor;

        toolStripOutputLabel.Text = engine.IsLoaded
            ? $"Output  {engine.EffectiveSampleRate / 1000d:0.#} kHz"
            : $"Output  {GetSelectedSampleRateLabel()}";
        toolStripOutputLabel.ForeColor = engine.IsLoaded ? TextSecondaryColor : TextMutedColor;
        toolStripHintLabel.ForeColor = TextMutedColor;

        UpdateMenuState();
        Text = track is null ? "Audio Player" : BuildWindowTitle(track);
    }

    private void UpdateMenuState()
    {
        playbackPlayPauseToolStripMenuItem.Text = engine.IsLoaded
            ? engine.IsPlaying ? "Pause" : "Play"
            : "Open Audio";
        playbackStopToolStripMenuItem.Enabled = engine.IsLoaded;
        playbackMuteToolStripMenuItem.Text = isMuted || trackBarVolume.Value == 0 ? "Unmute" : "Mute";
        playbackMuteToolStripMenuItem.Enabled = engine.IsLoaded || trackBarVolume.Value > 0 || isMuted;
    }

    private void ApplyInformationVisibility() => lblTrackInfo.Visible = appSettings.ShowMoreInfo;

    private string GetVisualizerCycleToolTip() =>
        appSettings.EnableVisualizerAutoCycle
            ? $"Visualizer mode (auto-cycles every {(int)VisualizerAutoCycleInterval.TotalSeconds} seconds)"
            : "Visualizer mode (auto-cycle disabled)";

    private void ShowSettingsDialog()
    {
        using var dialog = new SettingsDialog(
            appSettings,
            GetAvailableVisualizerModeOptions(),
            GetCurrentVisualizerMode(),
            GetAllVisualizerModeOptions(),
            GetSampleRateOptions(),
            GetCycleDurationOptions());

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        appSettings = AppSettingsStore.Normalize(dialog.Settings);
        SaveAppSettings();
        ApplyTheme();
        ApplyStoredSettings(dialog.SelectedCurrentVisualizer);
        UpdateUiState();
    }

    private VisualizerMode GetCurrentVisualizerMode() =>
        (cmbVisualizerMode.SelectedItem as SelectionOption<VisualizerMode>)?.Value
        ?? appSettings.DefaultVisualizer;

    private void SaveAppSettings() => AppSettingsStore.Save(appSettings);

    private static void SelectComboValue<T>(ComboBox comboBox, T value)
    {
        for (var index = 0; index < comboBox.Items.Count; index++)
        {
            if (comboBox.Items[index] is SelectionOption<T> option &&
                EqualityComparer<T>.Default.Equals(option.Value, value))
            {
                comboBox.SelectedIndex = index;
                return;
            }
        }

        if (comboBox.Items.Count > 0)
            comboBox.SelectedIndex = 0;
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private string GetSelectedSampleRateLabel() =>
        cmbSampleRate.SelectedItem is SelectionOption<int> option ? option.Label : "Match source";

    private static string FormatTime(float seconds)
    {
        var duration = TimeSpan.FromSeconds(Math.Max(0, seconds));
        return duration.TotalHours >= 1
            ? duration.ToString(@"h\:mm\:ss")
            : duration.ToString(@"m\:ss");
    }

    private static string FormatBitDepth(int bitsPerSample) =>
        bitsPerSample > 0 ? $"{bitsPerSample}-bit" : "float";

    private static string BuildTrackInfoText(AudioTrackInfo? track)
    {
        if (track is null)
        {
            return "Supports MP3, WAV, FLAC, AAC, M4A, WMA, OGG Vorbis, AIFF, Opus, WebM, 3GP and more through installed Windows codecs.";
        }

        var descriptiveParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(track.Artist))
        {
            descriptiveParts.Add(track.Artist);
        }

        if (!string.IsNullOrWhiteSpace(track.Album))
        {
            descriptiveParts.Add(track.Album);
        }

        var technicalLine =
            $"{track.FormatName}  \u00b7  {track.Channels} ch  \u00b7  {track.SourceSampleRate / 1000d:0.#} kHz  \u00b7  {FormatBitDepth(track.BitsPerSample)}  \u00b7  {FormatTime((float)track.Duration.TotalSeconds)}";

        if (track.Lyrics is not null)
        {
            technicalLine += track.Lyrics.HasWordTimings
                ? "  \u00b7  Enhanced lyrics"
                : "  \u00b7  Synced lyrics";
        }

        return descriptiveParts.Count == 0
            ? technicalLine
            : $"{string.Join("  \u00b7  ", descriptiveParts)}{Environment.NewLine}{technicalLine}";
    }

    private static string BuildWindowTitle(AudioTrackInfo track) =>
        string.IsNullOrWhiteSpace(track.Artist)
            ? $"{track.DisplayName}  \u2014  Audio Player"
            : $"{track.Artist} - {track.DisplayName}  \u2014  Audio Player";

    private void SetLyricsVisible(bool visible)
    {
        // Always sync column widths — the initial Designer state may not match runtime state.
        lyricsView.Visible = visible;
        contentLayout.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, visible ? 58F : 100F);
        contentLayout.ColumnStyles[1] = visible
            ? new ColumnStyle(SizeType.Percent, 42F)
            : new ColumnStyle(SizeType.Absolute, 0F);
        contentLayout.PerformLayout();
    }

    private void UpdateAlbumArt(AudioTrackInfo? track)
    {
        if (ReferenceEquals(displayedArtworkTrack, track) && picAlbumArt.Image is not null)
        {
            return;
        }

        displayedArtworkTrack = track;
        DisposeDisplayedArtwork();
        picAlbumArt.Image = CreateArtworkImage(track);

        // Update visualizer album art (real art only, no fallback)
        visualizerAlbumArt?.Dispose();
        visualizerAlbumArt = track?.AlbumArtBytes is { Length: > 0 } bytes ? TryLoadArtwork(bytes) : null;
        visualizerControl.AlbumArt = visualizerAlbumArt;
        RefreshVisualizerModeOptions(appSettings.DefaultVisualizer);
    }

    private void DisposeDisplayedArtwork()
    {
        if (picAlbumArt.Image is null)
        {
            return;
        }

        var image = picAlbumArt.Image;
        picAlbumArt.Image = null;
        image.Dispose();
    }

    private static Image? TryLoadArtwork(byte[] bytes)
    {
        try { return LoadArtwork(bytes); }
        catch { return null; }
    }

    private static Image CreateArtworkImage(AudioTrackInfo? track)
    {
        if (track?.AlbumArtBytes is { Length: > 0 } bytes)
        {
            try
            {
                return LoadArtwork(bytes);
            }
            catch
            {
                // Fall back to generated artwork when embedded art cannot be decoded.
            }
        }

        return CreateFallbackArtwork(track);
    }

    private static Image LoadArtwork(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        using var image = Image.FromStream(stream, useEmbeddedColorManagement: true, validateImageData: true);
        return new Bitmap(image);
    }

    private static Image CreateFallbackArtwork(AudioTrackInfo? track)
    {
        const int size = 240;
        var bitmap = new Bitmap(size, size);
        var accent = GetArtworkAccent(track);
        var shadow = ControlPaint.Dark(accent, 0.35f);

        using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using (var gradient = new LinearGradientBrush(new Rectangle(0, 0, size, size), accent, shadow, 45f))
        {
            graphics.FillRectangle(gradient, 0, 0, size, size);
        }

        using (var overlayBrush = new SolidBrush(Color.FromArgb(36, 255, 255, 255)))
        {
            graphics.FillEllipse(overlayBrush, -30, -10, 150, 150);
            graphics.FillEllipse(overlayBrush, 90, 120, 170, 170);
        }

        using var framePen = new Pen(Color.FromArgb(28, 255, 255, 255), 2f);
        graphics.DrawRectangle(framePen, 1, 1, size - 3, size - 3);

        using var textBrush = new SolidBrush(Color.FromArgb(245, 248, 255));
        using var textFormat = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        using var titleFont = new Font("Segoe UI Semibold", 70f, FontStyle.Bold, GraphicsUnit.Point);
        graphics.DrawString(GetArtworkInitials(track), titleFont, textBrush, new RectangleF(0, 0, size, size), textFormat);

        return bitmap;
    }

    private static Color GetArtworkAccent(AudioTrackInfo? track)
    {
        var seed = $"{track?.DisplayName}|{track?.Artist}|{track?.Album}";

        unchecked
        {
            var hash = 17;
            foreach (var character in seed)
            {
                hash = (hash * 31) + character;
            }

            return Color.FromArgb(
                255,
                72 + Math.Abs(hash % 72),
                88 + Math.Abs((hash / 7) % 80),
                120 + Math.Abs((hash / 13) % 84));
        }
    }

    private static string GetArtworkInitials(AudioTrackInfo? track)
    {
        if (track is null)
        {
            return "AP";
        }

        var source = !string.IsNullOrWhiteSpace(track.Album)
            ? track.Album
            : track.DisplayName;

        var initials = string.Concat(
            source
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Take(2)
                .Select(static part => char.ToUpperInvariant(part[0])));

        return string.IsNullOrWhiteSpace(initials) ? "AP" : initials;
    }

    private void ShowError(string message, string title)
    {
        MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void fileOpenToolStripMenuItem_Click(object sender, EventArgs e) => OpenAudioFile(startPlayback: true);

    private void fileSettingsToolStripMenuItem_Click(object sender, EventArgs e) => ShowSettingsDialog();

    private void fileSetDefaultToolStripMenuItem_Click(object sender, EventArgs e) => btnDefaultApp_Click(sender, e);

    private void fileExitToolStripMenuItem_Click(object sender, EventArgs e) => Close();

    private void playbackPlayPauseToolStripMenuItem_Click(object sender, EventArgs e) => btnPlayPause_Click(sender, e);

    private void playbackStopToolStripMenuItem_Click(object sender, EventArgs e) => btnStop_Click(sender, e);

    private void playbackMuteToolStripMenuItem_Click(object sender, EventArgs e) => btnMute_Click(sender, e);

    // ── Inner types ───────────────────────────────────────────────────────

    // Dark renderer for the status strip — matches form background exactly
    private sealed class ThemeStatusStripRenderer : ToolStripProfessionalRenderer
    {
        private readonly Color backgroundColor;
        private readonly Color borderColor;

        public ThemeStatusStripRenderer(Color backgroundColor, Color borderColor)
            : base(new ThemeColorTable(backgroundColor, backgroundColor, backgroundColor, borderColor))
        {
            this.backgroundColor = backgroundColor;
            this.borderColor = borderColor;
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using var brush = new SolidBrush(backgroundColor);
            e.Graphics.FillRectangle(brush, e.AffectedBounds);

            using var borderPen = new Pen(Color.FromArgb(48, borderColor), 1f);
            e.Graphics.DrawLine(
                borderPen,
                e.AffectedBounds.Left,
                e.AffectedBounds.Top,
                e.AffectedBounds.Right,
                e.AffectedBounds.Top);
        }

        protected override void OnRenderLabelBackground(ToolStripItemRenderEventArgs e) { }
        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e) { }
    }

    private sealed class ThemeMenuStripRenderer : ToolStripProfessionalRenderer
    {
        private readonly Color backgroundColor;
        private readonly Color imageMarginColor;

        public ThemeMenuStripRenderer(Color backgroundColor, Color imageMarginColor, Color selectionColor, Color borderColor)
            : base(new ThemeColorTable(backgroundColor, imageMarginColor, selectionColor, borderColor))
        {
            this.backgroundColor = backgroundColor;
            this.imageMarginColor = imageMarginColor;
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using var brush = new SolidBrush(backgroundColor);
            e.Graphics.FillRectangle(brush, e.AffectedBounds);
        }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
        {
            using var brush = new SolidBrush(imageMarginColor);
            e.Graphics.FillRectangle(brush, e.AffectedBounds);
        }
    }

    private sealed class ThemeColorTable : ProfessionalColorTable
    {
        private readonly Color backgroundColor;
        private readonly Color dropDownColor;
        private readonly Color selectionColor;
        private readonly Color borderColor;

        public ThemeColorTable(Color backgroundColor, Color dropDownColor, Color selectionColor, Color borderColor)
        {
            this.backgroundColor = backgroundColor;
            this.dropDownColor = dropDownColor;
            this.selectionColor = selectionColor;
            this.borderColor = borderColor;
        }

        public override Color StatusStripGradientBegin => backgroundColor;
        public override Color StatusStripGradientEnd => backgroundColor;
        public override Color MenuStripGradientBegin => backgroundColor;
        public override Color MenuStripGradientEnd => backgroundColor;
        public override Color ToolStripDropDownBackground => dropDownColor;
        public override Color ImageMarginGradientBegin => dropDownColor;
        public override Color ImageMarginGradientMiddle => dropDownColor;
        public override Color ImageMarginGradientEnd => dropDownColor;
        public override Color MenuItemSelected => selectionColor;
        public override Color MenuItemBorder => borderColor;
        public override Color MenuBorder => borderColor;
        public override Color MenuItemSelectedGradientBegin => selectionColor;
        public override Color MenuItemSelectedGradientEnd => selectionColor;
        public override Color MenuItemPressedGradientBegin => dropDownColor;
        public override Color MenuItemPressedGradientMiddle => dropDownColor;
        public override Color MenuItemPressedGradientEnd => dropDownColor;
        public override Color SeparatorDark => borderColor;
        public override Color SeparatorLight => dropDownColor;
    }
}
