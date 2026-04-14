using System.Drawing;
using System.Drawing.Drawing2D;
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
    private AudioTrackInfo? displayedArtworkTrack;

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
        DisposeDisplayedArtwork();
        engine.Dispose();
        base.OnFormClosed(e);
    }

    // ── Theme / Init ──────────────────────────────────────────────────────

    private void ApplyTheme()
    {
        ForeColor = Color.FromArgb(220, 232, 255);
        statusStrip1.Renderer = new DarkStatusStripRenderer();
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
        UpdateAlbumArt(track);

        // Play/Pause button text
        btnPlayPause.Text = track is null
            ? "Open Audio"
            : engine.IsPlaying ? "Pause" : "Play";

        // Stop button
        btnStop.Enabled     = track is not null;
        trackBarSeek.Enabled = track is not null;

        // Now-playing header
        lblNowPlaying.Text = track?.DisplayName ?? "Drop audio here or use Open";
        lblTrackInfo.Text = BuildTrackInfoText(track);
        lyricsView.UpdateState(track, engine.GetPosition());

        // Volume label & mute button
        if (isMuted || trackBarVolume.Value == 0)
        {
            lblVolumeValue.Text      = "Muted";
            lblVolumeValue.ForeColor = Color.FromArgb(100, 108, 140);
            btnMute.Text             = "Unmute";
            btnMute.AccentColor      = Color.FromArgb(160, 65, 72);
            btnMute.ForeColor        = Color.FromArgb(210, 130, 135);
        }
        else
        {
            lblVolumeValue.Text      = $"{trackBarVolume.Value}%";
            lblVolumeValue.ForeColor = Color.FromArgb(155, 175, 220);
            btnMute.Text             = "Mute";
            btnMute.AccentColor      = Color.FromArgb(70, 92, 160);
            btnMute.ForeColor        = Color.FromArgb(140, 158, 205);
        }

        // Status bar
        toolStripStatusLabel.Text = track is null
            ? "Ready"
            : engine.IsPlaying ? "Playing"
            : Math.Abs(engine.GetPosition()) < 0.01f ? "Loaded" : "Paused";

        toolStripStatusLabel.ForeColor = engine.IsPlaying
            ? Color.FromArgb(52, 211, 153)   // matches play button accent
            : Color.FromArgb(80, 100, 148);

        toolStripOutputLabel.Text = engine.IsLoaded
            ? $"Output  {engine.EffectiveSampleRate / 1000d:0.#} kHz"
            : $"Output  {GetSelectedSampleRateLabel()}";

        Text = track is null ? "Audio Player" : BuildWindowTitle(track);
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

    private void UpdateAlbumArt(AudioTrackInfo? track)
    {
        if (ReferenceEquals(displayedArtworkTrack, track) && picAlbumArt.Image is not null)
        {
            return;
        }

        displayedArtworkTrack = track;
        DisposeDisplayedArtwork();
        picAlbumArt.Image = CreateArtworkImage(track);
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

    // ── Inner types ───────────────────────────────────────────────────────

    private sealed record SampleRateOption(string Label, int SampleRate)
    {
        public override string ToString() => Label;
    }

    private sealed record VisualizerModeOption(string Label, VisualizerMode Mode)
    {
        public override string ToString() => Label;
    }

    // Dark renderer for the status strip — matches form background exactly
    private sealed class DarkStatusStripRenderer : ToolStripProfessionalRenderer
    {
        // rgb(10,13,22) matches the form BackColor
        private static readonly Color BgColor = Color.FromArgb(10, 13, 22);

        public DarkStatusStripRenderer() : base(new DarkColorTable()) { }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using var brush = new SolidBrush(BgColor);
            e.Graphics.FillRectangle(brush, e.AffectedBounds);
        }

        protected override void OnRenderLabelBackground(ToolStripItemRenderEventArgs e) { }
        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e) { }
    }

    private sealed class DarkColorTable : ProfessionalColorTable
    {
        private static readonly Color Bg = Color.FromArgb(10, 13, 22);
        public override Color StatusStripGradientBegin => Bg;
        public override Color StatusStripGradientEnd   => Bg;
    }
}
