using System.Drawing;
using System.IO;

namespace AudioPlayer;

public partial class Form1 : Form
{
    private readonly AudioEngine engine = new();
    private readonly string? startupPath;

    private bool isUpdatingSeekBar;
    private bool isApplyingSettings;
    private bool showRemainingTime;
    private bool isMuted;
    private float preMuteVolume = 0.85f;

    public Form1(string? startupPath = null)
    {
        this.startupPath = startupPath;

        InitializeComponent();
        ApplyTheme();
        PopulateSettings();
        WireFileDrop(this);
        UpdateUiState();
    }

    // ── Load ─────────────────────────────────────────────────────────────

    private void Form1_Load(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(startupPath))
            return;

        if (File.Exists(startupPath))
        {
            LoadAudioFile(startupPath);
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
                OpenAudioFile();
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
            OpenAudioFile();
            return;
        }

        engine.Toggle();
        UpdateUiState();
    }

    private void btnOpen_Click(object sender, EventArgs e)
    {
        OpenAudioFile();
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
        if (isApplyingSettings || cmbSampleRate.SelectedItem is not SampleRateOption option)
            return;

        engine.SetPreferredSampleRate(option.SampleRate);
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

        LoadAudioFile(files[0]);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        timer1.Stop();
        engine.Dispose();
        base.OnFormClosed(e);
    }

    // ── Theme / Init ──────────────────────────────────────────────────────

    private void ApplyTheme()
    {
        ForeColor = Color.FromArgb(220, 232, 255);

        // Status strip dark renderer
        statusStrip1.Renderer = new DarkStatusStripRenderer();

        // Checkbox styling
        chkPeakHold.ForeColor = Color.FromArgb(185, 200, 230);
    }

    private void PopulateSettings()
    {
        isApplyingSettings = true;

        cmbVisualizerMode.Items.Clear();
        cmbVisualizerMode.Items.AddRange(new object[]
        {
            new VisualizerModeOption("Spectrum",        VisualizerMode.Spectrum),
            new VisualizerModeOption("Mirror Spectrum",  VisualizerMode.MirrorSpectrum),
            new VisualizerModeOption("Waveform",        VisualizerMode.Waveform)
        });
        cmbVisualizerMode.SelectedIndex = 1;

        cmbSampleRate.Items.Clear();
        cmbSampleRate.Items.AddRange(new object[]
        {
            new SampleRateOption("Match source", 0),
            new SampleRateOption("44.1 kHz", 44100),
            new SampleRateOption("48 kHz",   48000),
            new SampleRateOption("88.2 kHz", 88200),
            new SampleRateOption("96 kHz",   96000)
        });
        cmbSampleRate.SelectedIndex = 0;

        trackBarSensitivity.Value = 100;
        trackBarVolume.Value = 85;
        chkPeakHold.Checked = true;
        engine.Volume = trackBarVolume.Value / 100f;

        isApplyingSettings = false;
        ApplyVisualizerSettings();
    }

    private void ApplyVisualizerSettings()
    {
        if (cmbVisualizerMode.SelectedItem is VisualizerModeOption option)
            visualizerControl.Mode = option.Mode;

        visualizerControl.ShowPeaks = chkPeakHold.Checked;
        visualizerControl.Sensitivity = trackBarSensitivity.Value / 100f;
    }

    private void WireFileDrop(Control control)
    {
        control.AllowDrop = true;
        control.DragEnter -= Form1_DragEnter;
        control.DragDrop  -= Form1_DragDrop;
        control.DragEnter += Form1_DragEnter;
        control.DragDrop  += Form1_DragDrop;

        foreach (Control child in control.Controls)
            WireFileDrop(child);
    }

    // ── File operations ───────────────────────────────────────────────────

    private void OpenAudioFile()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = SupportedAudioFormats.OpenFileDialogFilter,
            Title = "Select an audio file"
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
            LoadAudioFile(dialog.FileName);
    }

    private void LoadAudioFile(string path)
    {
        if (!File.Exists(path))
        {
            ShowError(
                $"The selected file could not be found:{Environment.NewLine}{Environment.NewLine}{path}",
                "Open Error");
            return;
        }

        try
        {
            engine.Load(path);
            visualizerControl.ClearFrame();
            trackBarSeek.Value = 0;
        }
        catch (Exception ex)
        {
            ShowError(
                $"Unable to load the selected audio file.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                "Load Error");
        }

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

        // Play/Pause button text
        btnPlayPause.Text = track is null
            ? "Open Audio"
            : engine.IsPlaying ? "Pause" : "Play";

        // Stop button
        btnStop.Enabled     = track is not null;
        trackBarSeek.Enabled = track is not null;

        // Now-playing header
        lblNowPlaying.Text = track?.DisplayName ?? "Drop audio here or use Open";
        lblTrackInfo.Text = track is null
            ? "Supports MP3, WAV, FLAC, AAC, M4A, WMA, OGG Vorbis, AIFF, Opus, WebM, 3GP and more through installed Windows codecs."
            : $"{track.FormatName}  \u00b7  {track.Channels} ch  \u00b7  {track.SourceSampleRate / 1000d:0.#} kHz  \u00b7  {FormatBitDepth(track.BitsPerSample)}  \u00b7  {FormatTime((float)track.Duration.TotalSeconds)}";

        // Volume label & mute button
        if (isMuted || trackBarVolume.Value == 0)
        {
            lblVolumeValue.Text = "Muted";
            lblVolumeValue.ForeColor = Color.FromArgb(150, 155, 175);
            btnMute.Text = "Unmute";
            btnMute.AccentColor = Color.FromArgb(90, 60, 60);
        }
        else
        {
            lblVolumeValue.Text = $"{trackBarVolume.Value}%";
            lblVolumeValue.ForeColor = Color.FromArgb(200, 220, 255);
            btnMute.Text = "Mute";
            btnMute.AccentColor = Color.FromArgb(38, 52, 90);
        }

        // Status bar
        toolStripStatusLabel.Text = track is null
            ? "Ready"
            : engine.IsPlaying ? "Playing"
            : Math.Abs(engine.GetPosition()) < 0.01f ? "Loaded" : "Paused";

        toolStripStatusLabel.ForeColor = engine.IsPlaying
            ? Color.FromArgb(100, 230, 160)
            : Color.FromArgb(155, 170, 205);

        toolStripOutputLabel.Text = engine.IsLoaded
            ? $"Output  {engine.EffectiveSampleRate / 1000d:0.#} kHz"
            : $"Output  {GetSelectedSampleRateLabel()}";

        Text = track is null ? "Audio Player" : $"{track.DisplayName}  \u2014  Audio Player";
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private string GetSelectedSampleRateLabel() =>
        cmbSampleRate.SelectedItem is SampleRateOption option ? option.Label : "Match source";

    private static string FormatTime(float seconds)
    {
        var duration = TimeSpan.FromSeconds(Math.Max(0, seconds));
        return duration.TotalHours >= 1
            ? duration.ToString(@"h\:mm\:ss")
            : duration.ToString(@"m\:ss");
    }

    private static string FormatBitDepth(int bitsPerSample) =>
        bitsPerSample > 0 ? $"{bitsPerSample}-bit" : "float";

    private void ShowError(string message, string title)
    {
        MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    // ── Inner types ───────────────────────────────────────────────────────

    private sealed record SampleRateOption(string Label, int SampleRate)
    {
        public override string ToString() => Label;
    }

    private sealed record VisualizerModeOption(string Label, VisualizerMode Mode)
    {
        public override string ToString() => Label;
    }

    // Dark renderer for the status strip
    private sealed class DarkStatusStripRenderer : ToolStripProfessionalRenderer
    {
        public DarkStatusStripRenderer() : base(new DarkColorTable()) { }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using var brush = new SolidBrush(Color.FromArgb(10, 14, 26));
            e.Graphics.FillRectangle(brush, e.AffectedBounds);
        }

        protected override void OnRenderLabelBackground(ToolStripItemRenderEventArgs e) { }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e) { }
    }

    private sealed class DarkColorTable : ProfessionalColorTable
    {
        public override Color StatusStripGradientBegin => Color.FromArgb(10, 14, 26);
        public override Color StatusStripGradientEnd   => Color.FromArgb(10, 14, 26);
    }
}
