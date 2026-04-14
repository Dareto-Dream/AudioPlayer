namespace AudioPlayer
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        // ── Layout ─────────────────────────────────────────────────────────
        private System.Windows.Forms.TableLayoutPanel rootLayout;
        private System.Windows.Forms.TableLayoutPanel contentLayout;
        private System.Windows.Forms.TableLayoutPanel trackInfoPanel;
        private System.Windows.Forms.TableLayoutPanel seekLayout;
        private System.Windows.Forms.TableLayoutPanel transportLayout;
        private System.Windows.Forms.FlowLayoutPanel leftButtonsPanel;
        private System.Windows.Forms.FlowLayoutPanel rightControlsPanel;
        private System.Windows.Forms.FlowLayoutPanel settingsPanel;

        // ── Track info ─────────────────────────────────────────────────────
        private System.Windows.Forms.PictureBox picAlbumArt;
        private System.Windows.Forms.Label lblSectionCaption;
        private System.Windows.Forms.Label lblNowPlaying;
        private System.Windows.Forms.Label lblTrackInfo;

        // ── Visualizer ─────────────────────────────────────────────────────
        private SpectrumVisualizerControl visualizerControl;
        private LyricsViewControl lyricsView;

        // ── Seek bar ───────────────────────────────────────────────────────
        private System.Windows.Forms.Label lblCurrentTime;
        private ModernSlider trackBarSeek;
        private System.Windows.Forms.Label lblDuration;

        // ── Transport ──────────────────────────────────────────────────────
        private ModernButton btnOpen;
        private ModernButton btnPlayPause;
        private ModernButton btnStop;
        private ModernButton btnMute;
        private System.Windows.Forms.Label lblVolumeCaption;
        private ModernSlider trackBarVolume;
        private System.Windows.Forms.Label lblVolumeValue;

        // ── Settings ───────────────────────────────────────────────────────
        private System.Windows.Forms.Label lblVisualizerModeCaption;
        private System.Windows.Forms.ComboBox cmbVisualizerMode;
        private System.Windows.Forms.CheckBox chkPeakHold;
        private System.Windows.Forms.Label lblSampleRateCaption;
        private System.Windows.Forms.ComboBox cmbSampleRate;
        private System.Windows.Forms.Label lblSensitivityCaption;
        private ModernSlider trackBarSensitivity;
        private ModernButton btnDefaultApp;

        // ── Status & timer ─────────────────────────────────────────────────
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel toolStripOutputLabel;
        private System.Windows.Forms.ToolStripStatusLabel toolStripHintLabel;
        private System.Windows.Forms.ToolTip toolTip1;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            rootLayout       = new System.Windows.Forms.TableLayoutPanel();
            contentLayout    = new System.Windows.Forms.TableLayoutPanel();
            trackInfoPanel   = new System.Windows.Forms.TableLayoutPanel();
            seekLayout       = new System.Windows.Forms.TableLayoutPanel();
            transportLayout  = new System.Windows.Forms.TableLayoutPanel();
            leftButtonsPanel = new System.Windows.Forms.FlowLayoutPanel();
            rightControlsPanel = new System.Windows.Forms.FlowLayoutPanel();
            settingsPanel    = new System.Windows.Forms.FlowLayoutPanel();

            picAlbumArt      = new System.Windows.Forms.PictureBox();
            lblSectionCaption = new System.Windows.Forms.Label();
            lblNowPlaying     = new System.Windows.Forms.Label();
            lblTrackInfo      = new System.Windows.Forms.Label();

            visualizerControl = new SpectrumVisualizerControl();
            lyricsView        = new LyricsViewControl();

            lblCurrentTime = new System.Windows.Forms.Label();
            trackBarSeek   = new ModernSlider();
            lblDuration    = new System.Windows.Forms.Label();

            btnOpen     = new ModernButton();
            btnPlayPause = new ModernButton();
            btnStop      = new ModernButton();
            btnMute      = new ModernButton();

            lblVolumeCaption = new System.Windows.Forms.Label();
            trackBarVolume   = new ModernSlider();
            lblVolumeValue   = new System.Windows.Forms.Label();

            lblVisualizerModeCaption = new System.Windows.Forms.Label();
            cmbVisualizerMode        = new System.Windows.Forms.ComboBox();
            chkPeakHold              = new System.Windows.Forms.CheckBox();
            lblSampleRateCaption     = new System.Windows.Forms.Label();
            cmbSampleRate            = new System.Windows.Forms.ComboBox();
            lblSensitivityCaption    = new System.Windows.Forms.Label();
            trackBarSensitivity      = new ModernSlider();
            btnDefaultApp            = new ModernButton();

            timer1               = new System.Windows.Forms.Timer(components);
            statusStrip1         = new System.Windows.Forms.StatusStrip();
            toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            toolStripOutputLabel = new System.Windows.Forms.ToolStripStatusLabel();
            toolStripHintLabel   = new System.Windows.Forms.ToolStripStatusLabel();
            toolTip1             = new System.Windows.Forms.ToolTip(components);

            rootLayout.SuspendLayout();
            contentLayout.SuspendLayout();
            trackInfoPanel.SuspendLayout();
            seekLayout.SuspendLayout();
            transportLayout.SuspendLayout();
            leftButtonsPanel.SuspendLayout();
            rightControlsPanel.SuspendLayout();
            settingsPanel.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();

            // ════════════════════════════════════════════════════════════════
            // rootLayout  — 5 rows: track-info / visualizer / seek / transport / settings
            // ════════════════════════════════════════════════════════════════
            rootLayout.ColumnCount = 1;
            rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            rootLayout.Controls.Add(trackInfoPanel,  0, 0);
            rootLayout.Controls.Add(contentLayout, 0, 1);
            rootLayout.Controls.Add(seekLayout,      0, 2);
            rootLayout.Controls.Add(transportLayout, 0, 3);
            rootLayout.Controls.Add(settingsPanel,   0, 4);
            rootLayout.Dock     = System.Windows.Forms.DockStyle.Fill;
            rootLayout.Location = new System.Drawing.Point(20, 20);
            rootLayout.Margin   = new System.Windows.Forms.Padding(0);
            rootLayout.Name     = "rootLayout";
            rootLayout.RowCount = 5;
            rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());                                                    // 0 track info
            rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));         // 1 visualizer
            rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());                                                    // 2 seek
            rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());                                                    // 3 transport
            rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());                                                    // 4 settings
            rootLayout.TabIndex = 0;

            contentLayout.ColumnCount = 2;
            contentLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 58F));
            contentLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 42F));
            contentLayout.Controls.Add(visualizerControl, 0, 0);
            contentLayout.Controls.Add(lyricsView, 1, 0);
            contentLayout.Dock      = System.Windows.Forms.DockStyle.Fill;
            contentLayout.Margin    = new System.Windows.Forms.Padding(0);
            contentLayout.Name      = "contentLayout";
            contentLayout.RowCount  = 1;
            contentLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            contentLayout.TabIndex  = 1;

            // ════════════════════════════════════════════════════════════════
            // trackInfoPanel  — stacks: caption / title / metadata
            // ════════════════════════════════════════════════════════════════
            trackInfoPanel.AutoSize     = true;
            trackInfoPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            trackInfoPanel.ColumnCount  = 2;
            trackInfoPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 138F));
            trackInfoPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            trackInfoPanel.Controls.Add(picAlbumArt,       0, 0);
            trackInfoPanel.Controls.Add(lblSectionCaption, 1, 0);
            trackInfoPanel.Controls.Add(lblNowPlaying,     1, 1);
            trackInfoPanel.Controls.Add(lblTrackInfo,      1, 2);
            trackInfoPanel.Dock   = System.Windows.Forms.DockStyle.Fill;
            trackInfoPanel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 16);
            trackInfoPanel.Name   = "trackInfoPanel";
            trackInfoPanel.RowCount = 3;
            trackInfoPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            trackInfoPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            trackInfoPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            trackInfoPanel.SetRowSpan(picAlbumArt, 3);
            trackInfoPanel.TabIndex = 0;

            // ── picAlbumArt ───────────────────────────────────────────────
            picAlbumArt.BackColor   = System.Drawing.Color.FromArgb(20, 25, 40);
            picAlbumArt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            picAlbumArt.Margin      = new System.Windows.Forms.Padding(0, 0, 18, 0);
            picAlbumArt.Name        = "picAlbumArt";
            picAlbumArt.Size        = new System.Drawing.Size(120, 120);
            picAlbumArt.SizeMode    = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picAlbumArt.TabStop     = false;
            toolTip1.SetToolTip(picAlbumArt, "Embedded album artwork");

            // ── lblSectionCaption ─────────────────────────────────────────
            // Small uppercase "NOW PLAYING" label
            lblSectionCaption.AutoSize  = true;
            lblSectionCaption.Font      = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            lblSectionCaption.ForeColor = System.Drawing.Color.FromArgb(80, 100, 148);
            lblSectionCaption.Margin    = new System.Windows.Forms.Padding(0, 0, 0, 6);
            lblSectionCaption.Name      = "lblSectionCaption";
            lblSectionCaption.Text      = "NOW PLAYING";

            // ── lblNowPlaying ─────────────────────────────────────────────
            // Main track title — large and prominent
            lblNowPlaying.AutoEllipsis = true;
            lblNowPlaying.AutoSize     = false;
            lblNowPlaying.Dock         = System.Windows.Forms.DockStyle.Fill;
            lblNowPlaying.Font         = new System.Drawing.Font("Segoe UI Semibold", 21F, System.Drawing.FontStyle.Bold);
            lblNowPlaying.ForeColor    = System.Drawing.Color.FromArgb(230, 238, 255);
            lblNowPlaying.Margin       = new System.Windows.Forms.Padding(0, 0, 0, 5);
            lblNowPlaying.Name         = "lblNowPlaying";
            lblNowPlaying.Size         = new System.Drawing.Size(1014, 42);
            lblNowPlaying.Text         = "Drop audio here or use Open";

            // ── lblTrackInfo ──────────────────────────────────────────────
            // Format / channels / sample-rate metadata line
            lblTrackInfo.AutoSize    = true;
            lblTrackInfo.Font        = new System.Drawing.Font("Segoe UI", 9F);
            lblTrackInfo.ForeColor   = System.Drawing.Color.FromArgb(105, 122, 168);
            lblTrackInfo.Margin      = new System.Windows.Forms.Padding(0, 0, 0, 0);
            lblTrackInfo.MaximumSize = new System.Drawing.Size(980, 0);
            lblTrackInfo.Name        = "lblTrackInfo";
            lblTrackInfo.Text        = "Supports MP3, WAV, FLAC, AAC, M4A, WMA, OGG Vorbis, AIFF, Opus, WebM, 3GP and more through installed Windows codecs.";

            // ════════════════════════════════════════════════════════════════
            // visualizerControl
            // ════════════════════════════════════════════════════════════════
            visualizerControl.Dock   = System.Windows.Forms.DockStyle.Fill;
            visualizerControl.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            visualizerControl.Mode   = AudioPlayer.VisualizerMode.MirrorSpectrum;
            visualizerControl.Name   = "visualizerControl";
            visualizerControl.ShowPeaks = true;
            visualizerControl.TabIndex  = 1;

            lyricsView.BackColor   = System.Drawing.Color.FromArgb(14, 19, 32);
            lyricsView.Dock        = System.Windows.Forms.DockStyle.Fill;
            lyricsView.Margin      = new System.Windows.Forms.Padding(16, 0, 0, 0);
            lyricsView.MinimumSize = new System.Drawing.Size(280, 0);
            lyricsView.Name        = "lyricsView";
            lyricsView.TabIndex    = 2;

            // ════════════════════════════════════════════════════════════════
            // seekLayout  — time / slider / time
            // ════════════════════════════════════════════════════════════════
            seekLayout.AutoSize     = true;
            seekLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            seekLayout.ColumnCount  = 3;
            seekLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            seekLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            seekLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            seekLayout.Controls.Add(lblCurrentTime, 0, 0);
            seekLayout.Controls.Add(trackBarSeek,   1, 0);
            seekLayout.Controls.Add(lblDuration,    2, 0);
            seekLayout.Dock     = System.Windows.Forms.DockStyle.Fill;
            seekLayout.Margin   = new System.Windows.Forms.Padding(0, 10, 0, 0);
            seekLayout.Name     = "seekLayout";
            seekLayout.RowCount = 1;
            seekLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            seekLayout.TabIndex = 2;

            // ── lblCurrentTime ────────────────────────────────────────────
            lblCurrentTime.Anchor    = System.Windows.Forms.AnchorStyles.Left;
            lblCurrentTime.AutoSize  = true;
            lblCurrentTime.Cursor    = System.Windows.Forms.Cursors.Hand;
            lblCurrentTime.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            lblCurrentTime.ForeColor = System.Drawing.Color.FromArgb(160, 180, 220);
            lblCurrentTime.Margin    = new System.Windows.Forms.Padding(0, 0, 6, 0);
            lblCurrentTime.Name      = "lblCurrentTime";
            lblCurrentTime.Text      = "0:00";
            lblCurrentTime.Click    += lblCurrentTime_Click;
            toolTip1.SetToolTip(lblCurrentTime, "Click to toggle remaining time");

            // ── trackBarSeek ──────────────────────────────────────────────
            trackBarSeek.IsLarge  = true;
            trackBarSeek.Dock     = System.Windows.Forms.DockStyle.Fill;
            trackBarSeek.Enabled  = false;
            trackBarSeek.Maximum  = 100;
            trackBarSeek.Minimum  = 0;
            trackBarSeek.Margin   = new System.Windows.Forms.Padding(0);
            trackBarSeek.Name     = "trackBarSeek";
            trackBarSeek.TabIndex = 1;
            trackBarSeek.Scroll  += trackBarSeek_Scroll;
            toolTip1.SetToolTip(trackBarSeek, "Seek  \u2190 / \u2192 = \u00b15s   Shift+\u2190\u2192 = \u00b130s");

            // ── lblDuration ───────────────────────────────────────────────
            lblDuration.Anchor    = System.Windows.Forms.AnchorStyles.Right;
            lblDuration.AutoSize  = true;
            lblDuration.Font      = new System.Drawing.Font("Segoe UI", 9F);
            lblDuration.ForeColor = System.Drawing.Color.FromArgb(80, 96, 138);
            lblDuration.Margin    = new System.Windows.Forms.Padding(6, 0, 0, 0);
            lblDuration.Name      = "lblDuration";
            lblDuration.Text      = "0:00";

            // ════════════════════════════════════════════════════════════════
            // transportLayout  — left buttons | play (centered) | volume group
            // ════════════════════════════════════════════════════════════════
            transportLayout.ColumnCount = 3;
            transportLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            transportLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            transportLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            transportLayout.Controls.Add(leftButtonsPanel,  0, 0);
            transportLayout.Controls.Add(btnPlayPause,      1, 0);
            transportLayout.Controls.Add(rightControlsPanel, 2, 0);
            transportLayout.Dock     = System.Windows.Forms.DockStyle.Fill;
            transportLayout.Margin   = new System.Windows.Forms.Padding(0, 14, 0, 0);
            transportLayout.Name     = "transportLayout";
            transportLayout.RowCount = 1;
            transportLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            transportLayout.TabIndex = 3;

            // ── leftButtonsPanel ──────────────────────────────────────────
            leftButtonsPanel.AutoSize     = true;
            leftButtonsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            leftButtonsPanel.Anchor       = System.Windows.Forms.AnchorStyles.Left;
            leftButtonsPanel.Controls.Add(btnOpen);
            leftButtonsPanel.Controls.Add(btnStop);
            leftButtonsPanel.Margin  = new System.Windows.Forms.Padding(0);
            leftButtonsPanel.Name    = "leftButtonsPanel";
            leftButtonsPanel.WrapContents = false;

            // ── btnOpen ───────────────────────────────────────────────────
            // Ghost-style secondary button
            btnOpen.IsGhost     = true;
            btnOpen.AccentColor = System.Drawing.Color.FromArgb(70, 92, 160);
            btnOpen.Font        = new System.Drawing.Font("Segoe UI", 9.5F);
            btnOpen.ForeColor   = System.Drawing.Color.FromArgb(165, 183, 225);
            btnOpen.Margin      = new System.Windows.Forms.Padding(0, 0, 10, 0);
            btnOpen.Name        = "btnOpen";
            btnOpen.Size        = new System.Drawing.Size(100, 52);
            btnOpen.TabIndex    = 0;
            btnOpen.Text        = "Open";
            btnOpen.Click      += btnOpen_Click;
            toolTip1.SetToolTip(btnOpen, "Open audio file (Ctrl+O)");

            // ── btnStop ───────────────────────────────────────────────────
            btnStop.IsGhost     = true;
            btnStop.AccentColor = System.Drawing.Color.FromArgb(150, 55, 62);
            btnStop.Enabled     = false;
            btnStop.Font        = new System.Drawing.Font("Segoe UI", 9.5F);
            btnStop.ForeColor   = System.Drawing.Color.FromArgb(165, 183, 225);
            btnStop.Margin      = new System.Windows.Forms.Padding(0);
            btnStop.Name        = "btnStop";
            btnStop.Size        = new System.Drawing.Size(90, 52);
            btnStop.TabIndex    = 1;
            btnStop.Text        = "Stop";
            btnStop.Click      += btnStop_Click;
            toolTip1.SetToolTip(btnStop, "Stop playback (Escape)");

            // ── btnPlayPause ──────────────────────────────────────────────
            // Primary action: pill-shaped, accent-filled, centered in its column
            btnPlayPause.Pill        = true;
            btnPlayPause.AccentColor = System.Drawing.Color.FromArgb(52, 211, 153);
            btnPlayPause.Font        = new System.Drawing.Font("Segoe UI Semibold", 10.5F, System.Drawing.FontStyle.Bold);
            btnPlayPause.ForeColor   = System.Drawing.Color.FromArgb(6, 20, 16);
            btnPlayPause.Anchor      = System.Windows.Forms.AnchorStyles.None;
            btnPlayPause.Name        = "btnPlayPause";
            btnPlayPause.Size        = new System.Drawing.Size(196, 52);
            btnPlayPause.TabIndex    = 2;
            btnPlayPause.Text        = "Open Audio";
            btnPlayPause.Click      += btnPlayPause_Click;
            toolTip1.SetToolTip(btnPlayPause, "Play / Pause (Space)");

            // ── rightControlsPanel ────────────────────────────────────────
            rightControlsPanel.AutoSize     = true;
            rightControlsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            rightControlsPanel.Anchor       = System.Windows.Forms.AnchorStyles.Right;
            rightControlsPanel.Controls.Add(lblVolumeCaption);
            rightControlsPanel.Controls.Add(btnMute);
            rightControlsPanel.Controls.Add(trackBarVolume);
            rightControlsPanel.Controls.Add(lblVolumeValue);
            rightControlsPanel.Margin  = new System.Windows.Forms.Padding(0);
            rightControlsPanel.Name    = "rightControlsPanel";
            rightControlsPanel.WrapContents = false;

            // ── lblVolumeCaption ──────────────────────────────────────────
            lblVolumeCaption.Anchor    = System.Windows.Forms.AnchorStyles.Left;
            lblVolumeCaption.AutoSize  = true;
            lblVolumeCaption.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            lblVolumeCaption.ForeColor = System.Drawing.Color.FromArgb(80, 100, 148);
            lblVolumeCaption.Margin    = new System.Windows.Forms.Padding(0, 0, 8, 0);
            lblVolumeCaption.Name      = "lblVolumeCaption";
            lblVolumeCaption.Text      = "Volume";

            // ── btnMute ───────────────────────────────────────────────────
            btnMute.IsGhost     = true;
            btnMute.AccentColor = System.Drawing.Color.FromArgb(70, 92, 160);
            btnMute.Font        = new System.Drawing.Font("Segoe UI", 8.5F);
            btnMute.ForeColor   = System.Drawing.Color.FromArgb(140, 158, 205);
            btnMute.Margin      = new System.Windows.Forms.Padding(0, 0, 8, 0);
            btnMute.Name        = "btnMute";
            btnMute.Size        = new System.Drawing.Size(52, 52);
            btnMute.TabIndex    = 3;
            btnMute.Text        = "Mute";
            btnMute.Click      += btnMute_Click;
            toolTip1.SetToolTip(btnMute, "Toggle mute (M)");

            // ── trackBarVolume ────────────────────────────────────────────
            trackBarVolume.IsLarge  = false;
            trackBarVolume.Maximum  = 100;
            trackBarVolume.Minimum  = 0;
            trackBarVolume.Margin   = new System.Windows.Forms.Padding(0, 0, 8, 0);
            trackBarVolume.Name     = "trackBarVolume";
            trackBarVolume.Size     = new System.Drawing.Size(156, 52);
            trackBarVolume.TabIndex = 4;
            trackBarVolume.Value    = 85;
            trackBarVolume.Scroll  += trackBarVolume_Scroll;
            toolTip1.SetToolTip(trackBarVolume, "Volume  Up/Down arrows");

            // ── lblVolumeValue ────────────────────────────────────────────
            lblVolumeValue.Anchor      = System.Windows.Forms.AnchorStyles.Left;
            lblVolumeValue.AutoSize    = true;
            lblVolumeValue.Font        = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            lblVolumeValue.ForeColor   = System.Drawing.Color.FromArgb(155, 175, 220);
            lblVolumeValue.MinimumSize = new System.Drawing.Size(40, 0);
            lblVolumeValue.Margin      = new System.Windows.Forms.Padding(0);
            lblVolumeValue.Name        = "lblVolumeValue";
            lblVolumeValue.Text        = "85%";

            // ════════════════════════════════════════════════════════════════
            // settingsPanel  — compact row: visualizer options + set default
            // ════════════════════════════════════════════════════════════════
            settingsPanel.AutoSize     = true;
            settingsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            settingsPanel.Controls.Add(lblVisualizerModeCaption);
            settingsPanel.Controls.Add(cmbVisualizerMode);
            settingsPanel.Controls.Add(chkPeakHold);
            settingsPanel.Controls.Add(lblSampleRateCaption);
            settingsPanel.Controls.Add(cmbSampleRate);
            settingsPanel.Controls.Add(lblSensitivityCaption);
            settingsPanel.Controls.Add(trackBarSensitivity);
            settingsPanel.Controls.Add(btnDefaultApp);
            settingsPanel.Dock   = System.Windows.Forms.DockStyle.Fill;
            settingsPanel.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
            settingsPanel.Name   = "settingsPanel";
            settingsPanel.WrapContents = false;

            // ── lblVisualizerModeCaption ──────────────────────────────────
            lblVisualizerModeCaption.Anchor    = System.Windows.Forms.AnchorStyles.Left;
            lblVisualizerModeCaption.AutoSize  = true;
            lblVisualizerModeCaption.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            lblVisualizerModeCaption.ForeColor = System.Drawing.Color.FromArgb(80, 100, 148);
            lblVisualizerModeCaption.Margin    = new System.Windows.Forms.Padding(0, 0, 8, 0);
            lblVisualizerModeCaption.Name      = "lblVisualizerModeCaption";
            lblVisualizerModeCaption.Text      = "Visualizer";

            // ── cmbVisualizerMode ─────────────────────────────────────────
            cmbVisualizerMode.DropDownStyle     = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbVisualizerMode.FlatStyle         = System.Windows.Forms.FlatStyle.Flat;
            cmbVisualizerMode.BackColor         = System.Drawing.Color.FromArgb(22, 28, 46);
            cmbVisualizerMode.ForeColor         = System.Drawing.Color.FromArgb(185, 200, 235);
            cmbVisualizerMode.FormattingEnabled = true;
            cmbVisualizerMode.Margin            = new System.Windows.Forms.Padding(0, 0, 20, 0);
            cmbVisualizerMode.Name              = "cmbVisualizerMode";
            cmbVisualizerMode.Size              = new System.Drawing.Size(152, 24);
            cmbVisualizerMode.TabIndex          = 5;
            cmbVisualizerMode.SelectedIndexChanged += cmbVisualizerMode_SelectedIndexChanged;

            // ── chkPeakHold ───────────────────────────────────────────────
            chkPeakHold.Anchor              = System.Windows.Forms.AnchorStyles.Left;
            chkPeakHold.AutoSize            = true;
            chkPeakHold.BackColor           = System.Drawing.Color.Transparent;
            chkPeakHold.Font                = new System.Drawing.Font("Segoe UI", 8.5F);
            chkPeakHold.ForeColor           = System.Drawing.Color.FromArgb(105, 122, 168);
            chkPeakHold.Margin              = new System.Windows.Forms.Padding(0, 0, 22, 0);
            chkPeakHold.Name                = "chkPeakHold";
            chkPeakHold.Text                = "Peak hold";
            chkPeakHold.UseVisualStyleBackColor = false;
            chkPeakHold.TabIndex            = 6;
            chkPeakHold.CheckedChanged     += chkPeakHold_CheckedChanged;

            // ── lblSampleRateCaption ──────────────────────────────────────
            lblSampleRateCaption.Anchor    = System.Windows.Forms.AnchorStyles.Left;
            lblSampleRateCaption.AutoSize  = true;
            lblSampleRateCaption.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            lblSampleRateCaption.ForeColor = System.Drawing.Color.FromArgb(80, 100, 148);
            lblSampleRateCaption.Margin    = new System.Windows.Forms.Padding(0, 0, 8, 0);
            lblSampleRateCaption.Name      = "lblSampleRateCaption";
            lblSampleRateCaption.Text      = "Output rate";

            // ── cmbSampleRate ─────────────────────────────────────────────
            cmbSampleRate.DropDownStyle     = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbSampleRate.FlatStyle         = System.Windows.Forms.FlatStyle.Flat;
            cmbSampleRate.BackColor         = System.Drawing.Color.FromArgb(22, 28, 46);
            cmbSampleRate.ForeColor         = System.Drawing.Color.FromArgb(185, 200, 235);
            cmbSampleRate.FormattingEnabled = true;
            cmbSampleRate.Margin            = new System.Windows.Forms.Padding(0, 0, 22, 0);
            cmbSampleRate.Name              = "cmbSampleRate";
            cmbSampleRate.Size              = new System.Drawing.Size(135, 24);
            cmbSampleRate.TabIndex          = 7;
            cmbSampleRate.SelectedIndexChanged += cmbSampleRate_SelectedIndexChanged;

            // ── lblSensitivityCaption ─────────────────────────────────────
            lblSensitivityCaption.Anchor    = System.Windows.Forms.AnchorStyles.Left;
            lblSensitivityCaption.AutoSize  = true;
            lblSensitivityCaption.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            lblSensitivityCaption.ForeColor = System.Drawing.Color.FromArgb(80, 100, 148);
            lblSensitivityCaption.Margin    = new System.Windows.Forms.Padding(0, 0, 8, 0);
            lblSensitivityCaption.Name      = "lblSensitivityCaption";
            lblSensitivityCaption.Text      = "Sensitivity";

            // ── trackBarSensitivity ───────────────────────────────────────
            trackBarSensitivity.IsLarge  = false;
            trackBarSensitivity.Maximum  = 200;
            trackBarSensitivity.Minimum  = 50;
            trackBarSensitivity.Margin   = new System.Windows.Forms.Padding(0, 0, 0, 0);
            trackBarSensitivity.Name     = "trackBarSensitivity";
            trackBarSensitivity.Size     = new System.Drawing.Size(112, 28);
            trackBarSensitivity.TabIndex = 8;
            trackBarSensitivity.Value    = 100;
            trackBarSensitivity.Scroll  += trackBarSensitivity_Scroll;

            // ── btnDefaultApp ─────────────────────────────────────────────
            // Secondary action — pushed to the far right via Margin
            btnDefaultApp.IsGhost     = true;
            btnDefaultApp.AccentColor = System.Drawing.Color.FromArgb(62, 52, 118);
            btnDefaultApp.Font        = new System.Drawing.Font("Segoe UI", 8F);
            btnDefaultApp.ForeColor   = System.Drawing.Color.FromArgb(90, 108, 155);
            btnDefaultApp.Margin      = new System.Windows.Forms.Padding(28, 0, 0, 0);
            btnDefaultApp.Name        = "btnDefaultApp";
            btnDefaultApp.Size        = new System.Drawing.Size(118, 28);
            btnDefaultApp.TabIndex    = 9;
            btnDefaultApp.Text        = "Set as Default\u2026";
            btnDefaultApp.Click      += btnDefaultApp_Click;
            toolTip1.SetToolTip(btnDefaultApp, "Register as default audio player for Windows");

            // ════════════════════════════════════════════════════════════════
            // Timer
            // ════════════════════════════════════════════════════════════════
            timer1.Interval = 33;
            timer1.Tick    += timer1_Tick;
            timer1.Start();

            // ════════════════════════════════════════════════════════════════
            // statusStrip1
            // ════════════════════════════════════════════════════════════════
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[]
                { toolStripStatusLabel, toolStripOutputLabel, toolStripHintLabel });
            statusStrip1.Padding      = new System.Windows.Forms.Padding(2, 0, 16, 0);
            statusStrip1.SizingGrip   = false;
            statusStrip1.TabIndex     = 1;

            toolStripStatusLabel.Name = "toolStripStatusLabel";
            toolStripStatusLabel.Text = "Ready";

            toolStripOutputLabel.Margin = new System.Windows.Forms.Padding(16, 3, 0, 2);
            toolStripOutputLabel.Name   = "toolStripOutputLabel";
            toolStripOutputLabel.Text   = "Output: Match source";

            toolStripHintLabel.Margin = new System.Windows.Forms.Padding(16, 3, 0, 2);
            toolStripHintLabel.Name   = "toolStripHintLabel";
            toolStripHintLabel.Text   =
                "Space: Play/Pause  \u00b7  \u2190\u2192: Seek \u00b15s  \u00b7  Shift+\u2190\u2192: \u00b130s  \u00b7  \u2191\u2193: Volume  \u00b7  M: Mute  \u00b7  Ctrl+O: Open";

            // ════════════════════════════════════════════════════════════════
            // Form1
            // ════════════════════════════════════════════════════════════════
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            BackColor           = System.Drawing.Color.FromArgb(10, 13, 22);
            ClientSize          = new System.Drawing.Size(1054, 700);
            Controls.Add(rootLayout);
            Controls.Add(statusStrip1);
            MinimumSize         = new System.Drawing.Size(860, 600);
            Name                = "Form1";
            Padding             = new System.Windows.Forms.Padding(20);
            StartPosition       = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text                = "Audio Player";
            Load               += Form1_Load;

            rootLayout.ResumeLayout(false);
            contentLayout.ResumeLayout(false);
            trackInfoPanel.ResumeLayout(false);
            trackInfoPanel.PerformLayout();
            seekLayout.ResumeLayout(false);
            seekLayout.PerformLayout();
            transportLayout.ResumeLayout(false);
            transportLayout.PerformLayout();
            leftButtonsPanel.ResumeLayout(false);
            leftButtonsPanel.PerformLayout();
            rightControlsPanel.ResumeLayout(false);
            rightControlsPanel.PerformLayout();
            settingsPanel.ResumeLayout(false);
            settingsPanel.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
