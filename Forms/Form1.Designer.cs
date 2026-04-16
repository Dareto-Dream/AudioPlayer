namespace Spectrallis
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
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileOpenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileSetDefaultToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileExitToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator fileToolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator fileToolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem playbackToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem playbackPlayPauseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem playbackStopToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem playbackMuteToolStripMenuItem;

        // ── Track info ─────────────────────────────────────────────────────
        private System.Windows.Forms.PictureBox picAlbumArt;
        private System.Windows.Forms.Label lblNowPlaying;
        private System.Windows.Forms.Label lblTrackInfo;

        // ── Visualizer ─────────────────────────────────────────────────────
        private SpectrumVisualizerControl visualizerControl;
        private LyricsViewControl lyricsView;
        private System.Windows.Forms.FlowLayoutPanel visualizerNavPanel;
        private ModernButton btnVisualizerPrev;
        private System.Windows.Forms.Label lblVisualizerModeName;
        private ModernButton btnVisualizerNext;
        private ModernSwitch chkVisualizerAutoCycle;
        private System.Windows.Forms.Label lblVisualizerAutoCycleCaption;

        // ── Seek bar ───────────────────────────────────────────────────────
        private System.Windows.Forms.Label lblCurrentTime;
        private ModernSlider trackBarSeek;
        private System.Windows.Forms.Label lblDuration;

        // ── Transport ──────────────────────────────────────────────────────
        private ModernButton btnPlayPause;
        private ModernButton btnStop;
        private ModernButton btnMute;
        private ModernSlider trackBarVolume;
        private System.Windows.Forms.Label lblVolumeValue;

        // ── Settings ───────────────────────────────────────────────────────
        private System.Windows.Forms.Label lblVisualizerModeCaption;
        private ModernComboBox cmbVisualizerMode;
        private ModernSwitch chkPeakHold;
        private System.Windows.Forms.Label lblPeakHoldCaption;
        private System.Windows.Forms.Label lblSampleRateCaption;
        private ModernComboBox cmbSampleRate;
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
            menuStrip1       = new System.Windows.Forms.MenuStrip();
            fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            fileOpenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            fileToolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            fileSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            fileSetDefaultToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            fileToolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            fileExitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            playbackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            playbackPlayPauseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            playbackStopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            playbackMuteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            picAlbumArt      = new System.Windows.Forms.PictureBox();
            lblNowPlaying     = new System.Windows.Forms.Label();
            lblTrackInfo      = new System.Windows.Forms.Label();

            visualizerControl = new SpectrumVisualizerControl();
            lyricsView        = new LyricsViewControl();

            visualizerNavPanel   = new System.Windows.Forms.FlowLayoutPanel();
            btnVisualizerPrev    = new ModernButton();
            lblVisualizerModeName = new System.Windows.Forms.Label();
            btnVisualizerNext    = new ModernButton();
            chkVisualizerAutoCycle = new ModernSwitch();
            lblVisualizerAutoCycleCaption = new System.Windows.Forms.Label();

            lblCurrentTime = new System.Windows.Forms.Label();
            trackBarSeek   = new ModernSlider();
            lblDuration    = new System.Windows.Forms.Label();

            btnPlayPause = new ModernButton();
            btnStop      = new ModernButton();
            btnMute      = new ModernButton();

            trackBarVolume   = new ModernSlider();
            lblVolumeValue   = new System.Windows.Forms.Label();

            lblVisualizerModeCaption = new System.Windows.Forms.Label();
            cmbVisualizerMode        = new ModernComboBox();
            chkPeakHold              = new ModernSwitch();
            lblPeakHoldCaption       = new System.Windows.Forms.Label();
            lblSampleRateCaption     = new System.Windows.Forms.Label();
            cmbSampleRate            = new ModernComboBox();
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
            visualizerNavPanel.SuspendLayout();
            settingsPanel.SuspendLayout();
            menuStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();

            // ════════════════════════════════════════════════════════════════
            // rootLayout  — 6 rows: track-info / visualizer / viz-nav / seek / transport / settings
            // ════════════════════════════════════════════════════════════════
            rootLayout.ColumnCount = 1;
            rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            rootLayout.Controls.Add(trackInfoPanel,  0, 0);
            rootLayout.Controls.Add(contentLayout, 0, 1);
            rootLayout.Controls.Add(visualizerNavPanel, 0, 2);
            rootLayout.Controls.Add(seekLayout,      0, 3);
            rootLayout.Controls.Add(transportLayout, 0, 4);
            rootLayout.Controls.Add(settingsPanel,   0, 5);
            rootLayout.Dock     = System.Windows.Forms.DockStyle.Fill;
            rootLayout.Location = new System.Drawing.Point(0, 34);
            rootLayout.Margin   = new System.Windows.Forms.Padding(0);
            rootLayout.Name     = "rootLayout";
            rootLayout.Padding  = new System.Windows.Forms.Padding(28, 18, 28, 0);
            rootLayout.RowCount = 6;
            rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());                                                    // 0 track info
            rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));         // 1 visualizer
            rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());                                                    // 2 viz nav
            rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());                                                    // 3 seek
            rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());                                                    // 4 transport
            rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());                                                    // 5 settings
            rootLayout.TabIndex = 0;

            contentLayout.ColumnCount = 2;
            // Col 1 starts at 0 — lyrics hidden by default; SetLyricsVisible adjusts at runtime.
            contentLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            contentLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 0F));
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
            trackInfoPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 116F));
            trackInfoPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            trackInfoPanel.Controls.Add(picAlbumArt,   0, 0);
            trackInfoPanel.Controls.Add(lblNowPlaying, 1, 0);
            trackInfoPanel.Controls.Add(lblTrackInfo,  1, 1);
            trackInfoPanel.Dock   = System.Windows.Forms.DockStyle.Fill;
            trackInfoPanel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 18);
            trackInfoPanel.Name   = "trackInfoPanel";
            trackInfoPanel.RowCount = 2;
            trackInfoPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            trackInfoPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            trackInfoPanel.SetRowSpan(picAlbumArt, 2);
            trackInfoPanel.TabIndex = 0;

            // ── picAlbumArt ───────────────────────────────────────────────
            picAlbumArt.BackColor   = System.Drawing.Color.FromArgb(16, 22, 36);
            picAlbumArt.BorderStyle = System.Windows.Forms.BorderStyle.None;
            picAlbumArt.Margin      = new System.Windows.Forms.Padding(0, 0, 16, 0);
            picAlbumArt.Name        = "picAlbumArt";
            picAlbumArt.Size        = new System.Drawing.Size(96, 96);
            picAlbumArt.SizeMode    = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picAlbumArt.TabStop     = false;
            toolTip1.SetToolTip(picAlbumArt, "Embedded album artwork");

            // ── lblNowPlaying ─────────────────────────────────────────────
            lblNowPlaying.AutoEllipsis = true;
            lblNowPlaying.AutoSize     = false;
            lblNowPlaying.Dock         = System.Windows.Forms.DockStyle.Fill;
            lblNowPlaying.Font         = new System.Drawing.Font("Segoe UI Semibold", 15F, System.Drawing.FontStyle.Bold);
            lblNowPlaying.ForeColor    = System.Drawing.Color.FromArgb(228, 236, 255);
            lblNowPlaying.Margin       = new System.Windows.Forms.Padding(0, 4, 0, 4);
            lblNowPlaying.Name         = "lblNowPlaying";
            lblNowPlaying.TextAlign    = System.Drawing.ContentAlignment.MiddleLeft;
            lblNowPlaying.Text         = "Drop a file here or press Play";

            // ── lblTrackInfo ──────────────────────────────────────────────
            // Format / channels / sample-rate metadata line
            lblTrackInfo.AutoSize    = true;
            lblTrackInfo.Font        = new System.Drawing.Font("Segoe UI", 8.5F);
            lblTrackInfo.ForeColor   = System.Drawing.Color.FromArgb(90, 108, 152);
            lblTrackInfo.Margin      = new System.Windows.Forms.Padding(0, 0, 0, 0);
            lblTrackInfo.MaximumSize = new System.Drawing.Size(980, 0);
            lblTrackInfo.Name        = "lblTrackInfo";
            lblTrackInfo.Text        = "MP3  \u00b7  WAV  \u00b7  FLAC  \u00b7  AAC  \u00b7  OGG  \u00b7  and more";

            // ════════════════════════════════════════════════════════════════
            // visualizerControl
            // ════════════════════════════════════════════════════════════════
            visualizerControl.Dock   = System.Windows.Forms.DockStyle.Fill;
            visualizerControl.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            visualizerControl.Mode   = Spectrallis.VisualizerMode.MirrorSpectrum;
            visualizerControl.Name   = "visualizerControl";
            visualizerControl.ShowPeaks = true;
            visualizerControl.TabIndex  = 1;

            lyricsView.BackColor   = System.Drawing.Color.FromArgb(14, 19, 32);
            lyricsView.Dock        = System.Windows.Forms.DockStyle.Fill;
            lyricsView.Margin      = new System.Windows.Forms.Padding(16, 0, 0, 0);
            lyricsView.MinimumSize = new System.Drawing.Size(280, 0);
            lyricsView.Name        = "lyricsView";
            lyricsView.TabIndex    = 2;
            lyricsView.Visible     = false;

            // ════════════════════════════════════════════════════════════════
            // visualizerNavPanel  — prev / name / next / auto-cycle
            // ════════════════════════════════════════════════════════════════
            visualizerNavPanel.AutoSize     = true;
            visualizerNavPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            visualizerNavPanel.Controls.Add(btnVisualizerPrev);
            visualizerNavPanel.Controls.Add(lblVisualizerModeName);
            visualizerNavPanel.Controls.Add(btnVisualizerNext);
            visualizerNavPanel.Controls.Add(lblVisualizerAutoCycleCaption);
            visualizerNavPanel.Controls.Add(chkVisualizerAutoCycle);
            visualizerNavPanel.Dock           = System.Windows.Forms.DockStyle.Fill;
            visualizerNavPanel.Margin         = new System.Windows.Forms.Padding(0, 4, 0, 6);
            visualizerNavPanel.Name           = "visualizerNavPanel";
            visualizerNavPanel.WrapContents   = false;

            // ── btnVisualizerPrev ─────────────────────────────────────────────
            btnVisualizerPrev.IsGhost       = true;
            btnVisualizerPrev.Font          = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            btnVisualizerPrev.Margin        = new System.Windows.Forms.Padding(0, 0, 6, 0);
            btnVisualizerPrev.Name          = "btnVisualizerPrev";
            btnVisualizerPrev.Size          = new System.Drawing.Size(30, 28);
            btnVisualizerPrev.TabIndex      = 0;
            btnVisualizerPrev.Text          = "‹";
            btnVisualizerPrev.Click        += btnVisualizerPrev_Click;
            toolTip1.SetToolTip(btnVisualizerPrev, "Previous visualizer mode");

            // ── lblVisualizerModeName ─────────────────────────────────────────
            lblVisualizerModeName.AutoSize  = true;
            lblVisualizerModeName.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            lblVisualizerModeName.Margin    = new System.Windows.Forms.Padding(0, 0, 0, 0);
            lblVisualizerModeName.Name      = "lblVisualizerModeName";
            lblVisualizerModeName.Text      = "Mirror Spectrum";
            lblVisualizerModeName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // ── btnVisualizerNext ─────────────────────────────────────────────
            btnVisualizerNext.IsGhost       = true;
            btnVisualizerNext.Font          = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            btnVisualizerNext.Margin        = new System.Windows.Forms.Padding(6, 0, 16, 0);
            btnVisualizerNext.Name          = "btnVisualizerNext";
            btnVisualizerNext.Size          = new System.Drawing.Size(30, 28);
            btnVisualizerNext.TabIndex      = 1;
            btnVisualizerNext.Text          = "›";
            btnVisualizerNext.Click        += btnVisualizerNext_Click;
            toolTip1.SetToolTip(btnVisualizerNext, "Next visualizer mode");

            // ── chkVisualizerAutoCycle ────────────────────────────────────────
            chkVisualizerAutoCycle.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            chkVisualizerAutoCycle.Name   = "chkVisualizerAutoCycle";
            chkVisualizerAutoCycle.CheckedChanged += chkVisualizerAutoCycle_CheckedChanged;
            toolTip1.SetToolTip(chkVisualizerAutoCycle, "Automatically cycle through visualizer modes");

            // ── lblVisualizerAutoCycleCaption ─────────────────────────────
            lblVisualizerAutoCycleCaption.Anchor    = System.Windows.Forms.AnchorStyles.Left;
            lblVisualizerAutoCycleCaption.AutoSize  = true;
            lblVisualizerAutoCycleCaption.Font      = new System.Drawing.Font("Segoe UI", 8F);
            lblVisualizerAutoCycleCaption.ForeColor = System.Drawing.Color.FromArgb(72, 90, 136);
            lblVisualizerAutoCycleCaption.Margin    = new System.Windows.Forms.Padding(0, 0, 6, 0);
            lblVisualizerAutoCycleCaption.Name      = "lblVisualizerAutoCycleCaption";
            lblVisualizerAutoCycleCaption.Text      = "Auto-cycle";

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
            seekLayout.Margin   = new System.Windows.Forms.Padding(0, 6, 0, 0);
            seekLayout.Name     = "seekLayout";
            seekLayout.RowCount = 1;
            seekLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            seekLayout.TabIndex = 2;

            // ── lblCurrentTime ────────────────────────────────────────────
            lblCurrentTime.Anchor    = System.Windows.Forms.AnchorStyles.Left;
            lblCurrentTime.AutoSize  = true;
            lblCurrentTime.Cursor    = System.Windows.Forms.Cursors.Hand;
            lblCurrentTime.Font      = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular);
            lblCurrentTime.ForeColor = System.Drawing.Color.FromArgb(140, 160, 205);
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
            lblDuration.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            lblDuration.ForeColor = System.Drawing.Color.FromArgb(72, 88, 128);
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
            leftButtonsPanel.Controls.Add(btnStop);
            leftButtonsPanel.Margin  = new System.Windows.Forms.Padding(0);
            leftButtonsPanel.Name    = "leftButtonsPanel";
            leftButtonsPanel.WrapContents = false;

            // ── btnStop ───────────────────────────────────────────────────
            btnStop.IsGhost     = true;
            btnStop.AccentColor = System.Drawing.Color.FromArgb(150, 55, 62);
            btnStop.Enabled     = false;
            btnStop.Font        = new System.Drawing.Font("Segoe UI Semibold", 8.75F, System.Drawing.FontStyle.Bold);
            btnStop.ForeColor   = System.Drawing.Color.FromArgb(140, 158, 205);
            btnStop.Margin      = new System.Windows.Forms.Padding(0, 0, 10, 0);
            btnStop.Name        = "btnStop";
            btnStop.Size        = new System.Drawing.Size(74, 38);
            btnStop.TabIndex    = 1;
            btnStop.Text        = "Stop";
            btnStop.Click      += btnStop_Click;
            toolTip1.SetToolTip(btnStop, "Stop playback (Escape)");

            // ── btnPlayPause ──────────────────────────────────────────────
            // Primary action
            btnPlayPause.Pill        = false;
            btnPlayPause.AccentColor = System.Drawing.Color.FromArgb(52, 211, 153);
            btnPlayPause.Font        = new System.Drawing.Font("Segoe UI Semibold", 9.25F, System.Drawing.FontStyle.Bold);
            btnPlayPause.ForeColor   = System.Drawing.Color.FromArgb(6, 20, 16);
            btnPlayPause.Anchor      = System.Windows.Forms.AnchorStyles.None;
            btnPlayPause.Name        = "btnPlayPause";
            btnPlayPause.Size        = new System.Drawing.Size(158, 40);
            btnPlayPause.TabIndex    = 2;
            btnPlayPause.Text        = "Open Audio";
            btnPlayPause.Click      += btnPlayPause_Click;
            toolTip1.SetToolTip(btnPlayPause, "Play / Pause (Space)");

            // ── rightControlsPanel ────────────────────────────────────────
            rightControlsPanel.AutoSize     = true;
            rightControlsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            rightControlsPanel.Anchor       = System.Windows.Forms.AnchorStyles.Right;
            rightControlsPanel.Controls.Add(btnMute);
            rightControlsPanel.Controls.Add(trackBarVolume);
            rightControlsPanel.Controls.Add(lblVolumeValue);
            rightControlsPanel.Margin  = new System.Windows.Forms.Padding(0);
            rightControlsPanel.Name    = "rightControlsPanel";
            rightControlsPanel.WrapContents = false;

            // ── btnMute ───────────────────────────────────────────────────
            btnMute.IsGhost     = true;
            btnMute.AccentColor = System.Drawing.Color.FromArgb(70, 92, 160);
            btnMute.Font        = new System.Drawing.Font("Segoe UI Semibold", 8.75F, System.Drawing.FontStyle.Bold);
            btnMute.ForeColor   = System.Drawing.Color.FromArgb(140, 158, 205);
            btnMute.Margin      = new System.Windows.Forms.Padding(0, 0, 12, 0);
            btnMute.Name        = "btnMute";
            btnMute.Size        = new System.Drawing.Size(74, 38);
            btnMute.TabIndex    = 3;
            btnMute.Text        = "Mute";
            btnMute.Click      += btnMute_Click;
            toolTip1.SetToolTip(btnMute, "Toggle mute (M)");

            // ── trackBarVolume ────────────────────────────────────────────
            trackBarVolume.IsLarge  = false;
            trackBarVolume.Maximum  = 100;
            trackBarVolume.Minimum  = 0;
            trackBarVolume.Margin   = new System.Windows.Forms.Padding(0, 0, 12, 0);
            trackBarVolume.Name     = "trackBarVolume";
            trackBarVolume.Size     = new System.Drawing.Size(148, 38);
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
            settingsPanel.Controls.Add(lblPeakHoldCaption);
            settingsPanel.Controls.Add(chkPeakHold);
            settingsPanel.Controls.Add(lblSampleRateCaption);
            settingsPanel.Controls.Add(cmbSampleRate);
            settingsPanel.Controls.Add(lblSensitivityCaption);
            settingsPanel.Controls.Add(trackBarSensitivity);
            settingsPanel.Controls.Add(btnDefaultApp);
            settingsPanel.Dock   = System.Windows.Forms.DockStyle.Fill;
            settingsPanel.Margin = new System.Windows.Forms.Padding(0, 14, 0, 0);
            settingsPanel.Name   = "settingsPanel";
            settingsPanel.WrapContents = false;

            menuStrip1.AutoSize  = false;
            menuStrip1.Dock      = System.Windows.Forms.DockStyle.Top;
            menuStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[]
            {
                fileToolStripMenuItem,
                playbackToolStripMenuItem
            });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name     = "menuStrip1";
            menuStrip1.Padding  = new System.Windows.Forms.Padding(18, 4, 2, 4);
            menuStrip1.Size     = new System.Drawing.Size(1020, 34);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text     = "menuStrip1";

            fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[]
            {
                fileOpenToolStripMenuItem,
                fileToolStripSeparator1,
                fileSettingsToolStripMenuItem,
                fileSetDefaultToolStripMenuItem,
                fileToolStripSeparator2,
                fileExitToolStripMenuItem
            });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Text = "&File";

            fileOpenToolStripMenuItem.Name         = "fileOpenToolStripMenuItem";
            fileOpenToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O;
            fileOpenToolStripMenuItem.Text         = "&Open...";
            fileOpenToolStripMenuItem.Click       += fileOpenToolStripMenuItem_Click;

            fileToolStripSeparator1.Name = "fileToolStripSeparator1";

            fileSettingsToolStripMenuItem.Name = "fileSettingsToolStripMenuItem";
            fileSettingsToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+,";
            fileSettingsToolStripMenuItem.Text = "&Settings...";
            fileSettingsToolStripMenuItem.Click += fileSettingsToolStripMenuItem_Click;

            fileSetDefaultToolStripMenuItem.Name = "fileSetDefaultToolStripMenuItem";
            fileSetDefaultToolStripMenuItem.Text = "Set as &Default...";
            fileSetDefaultToolStripMenuItem.Click += fileSetDefaultToolStripMenuItem_Click;

            fileToolStripSeparator2.Name = "fileToolStripSeparator2";

            fileExitToolStripMenuItem.Name = "fileExitToolStripMenuItem";
            fileExitToolStripMenuItem.Text = "E&xit";
            fileExitToolStripMenuItem.Click += fileExitToolStripMenuItem_Click;

            playbackToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[]
            {
                playbackPlayPauseToolStripMenuItem,
                playbackStopToolStripMenuItem,
                playbackMuteToolStripMenuItem
            });
            playbackToolStripMenuItem.Name = "playbackToolStripMenuItem";
            playbackToolStripMenuItem.Text = "&Playback";

            playbackPlayPauseToolStripMenuItem.Name = "playbackPlayPauseToolStripMenuItem";
            playbackPlayPauseToolStripMenuItem.Text = "Open Audio";
            playbackPlayPauseToolStripMenuItem.Click += playbackPlayPauseToolStripMenuItem_Click;

            playbackStopToolStripMenuItem.Name = "playbackStopToolStripMenuItem";
            playbackStopToolStripMenuItem.Text = "&Stop";
            playbackStopToolStripMenuItem.Click += playbackStopToolStripMenuItem_Click;

            playbackMuteToolStripMenuItem.Name = "playbackMuteToolStripMenuItem";
            playbackMuteToolStripMenuItem.Text = "&Mute";
            playbackMuteToolStripMenuItem.Click += playbackMuteToolStripMenuItem_Click;

            // ── lblVisualizerModeCaption ──────────────────────────────────
            lblVisualizerModeCaption.Anchor    = System.Windows.Forms.AnchorStyles.Left;
            lblVisualizerModeCaption.AutoSize  = true;
            lblVisualizerModeCaption.Font      = new System.Drawing.Font("Segoe UI", 8F);
            lblVisualizerModeCaption.ForeColor = System.Drawing.Color.FromArgb(72, 90, 136);
            lblVisualizerModeCaption.Margin    = new System.Windows.Forms.Padding(0, 0, 6, 0);
            lblVisualizerModeCaption.Name      = "lblVisualizerModeCaption";
            lblVisualizerModeCaption.Text      = "Visualizer";

            // ── cmbVisualizerMode ─────────────────────────────────────────
            cmbVisualizerMode.DropDownStyle     = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbVisualizerMode.FlatStyle         = System.Windows.Forms.FlatStyle.Flat;
            cmbVisualizerMode.BackColor         = System.Drawing.Color.FromArgb(22, 28, 46);
            cmbVisualizerMode.ForeColor         = System.Drawing.Color.FromArgb(185, 200, 235);
            cmbVisualizerMode.FormattingEnabled = true;
            cmbVisualizerMode.Margin            = new System.Windows.Forms.Padding(0, 0, 12, 0);
            cmbVisualizerMode.Name              = "cmbVisualizerMode";
            cmbVisualizerMode.Size              = new System.Drawing.Size(148, 36);
            cmbVisualizerMode.TabIndex          = 5;
            cmbVisualizerMode.SelectedIndexChanged += cmbVisualizerMode_SelectedIndexChanged;

            // ── chkPeakHold ───────────────────────────────────────────────
            chkPeakHold.Anchor   = System.Windows.Forms.AnchorStyles.Left;
            chkPeakHold.Margin   = new System.Windows.Forms.Padding(0, 0, 12, 0);
            chkPeakHold.Name     = "chkPeakHold";
            chkPeakHold.TabIndex = 6;
            chkPeakHold.CheckedChanged += chkPeakHold_CheckedChanged;

            // ── lblPeakHoldCaption ────────────────────────────────────────
            lblPeakHoldCaption.Anchor    = System.Windows.Forms.AnchorStyles.Left;
            lblPeakHoldCaption.AutoSize  = true;
            lblPeakHoldCaption.Font      = new System.Drawing.Font("Segoe UI", 8F);
            lblPeakHoldCaption.ForeColor = System.Drawing.Color.FromArgb(72, 90, 136);
            lblPeakHoldCaption.Margin    = new System.Windows.Forms.Padding(0, 0, 6, 0);
            lblPeakHoldCaption.Name      = "lblPeakHoldCaption";
            lblPeakHoldCaption.Text      = "Peak hold";

            // ── lblSampleRateCaption ──────────────────────────────────────
            lblSampleRateCaption.Anchor    = System.Windows.Forms.AnchorStyles.Left;
            lblSampleRateCaption.AutoSize  = true;
            lblSampleRateCaption.Font      = new System.Drawing.Font("Segoe UI", 8F);
            lblSampleRateCaption.ForeColor = System.Drawing.Color.FromArgb(72, 90, 136);
            lblSampleRateCaption.Margin    = new System.Windows.Forms.Padding(0, 0, 6, 0);
            lblSampleRateCaption.Name      = "lblSampleRateCaption";
            lblSampleRateCaption.Text      = "Output rate";

            // ── cmbSampleRate ─────────────────────────────────────────────
            cmbSampleRate.DropDownStyle     = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbSampleRate.FlatStyle         = System.Windows.Forms.FlatStyle.Flat;
            cmbSampleRate.BackColor         = System.Drawing.Color.FromArgb(22, 28, 46);
            cmbSampleRate.ForeColor         = System.Drawing.Color.FromArgb(185, 200, 235);
            cmbSampleRate.FormattingEnabled = true;
            cmbSampleRate.Margin            = new System.Windows.Forms.Padding(0, 0, 12, 0);
            cmbSampleRate.Name              = "cmbSampleRate";
            cmbSampleRate.Size              = new System.Drawing.Size(128, 36);
            cmbSampleRate.TabIndex          = 7;
            cmbSampleRate.SelectedIndexChanged += cmbSampleRate_SelectedIndexChanged;

            // ── lblSensitivityCaption ─────────────────────────────────────
            lblSensitivityCaption.Anchor    = System.Windows.Forms.AnchorStyles.Left;
            lblSensitivityCaption.AutoSize  = true;
            lblSensitivityCaption.Font      = new System.Drawing.Font("Segoe UI", 8F);
            lblSensitivityCaption.ForeColor = System.Drawing.Color.FromArgb(72, 90, 136);
            lblSensitivityCaption.Margin    = new System.Windows.Forms.Padding(0, 0, 6, 0);
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
            btnDefaultApp.Font        = new System.Drawing.Font("Segoe UI", 8.5F);
            btnDefaultApp.ForeColor   = System.Drawing.Color.FromArgb(90, 108, 155);
            btnDefaultApp.Margin      = new System.Windows.Forms.Padding(18, 0, 0, 0);
            btnDefaultApp.Name        = "btnDefaultApp";
            btnDefaultApp.Size        = new System.Drawing.Size(136, 32);
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
                "Space: Play/Pause  \u00b7  \u2190\u2192: Seek \u00b15s  \u00b7  Shift+\u2190\u2192: \u00b130s  \u00b7  \u2191\u2193: Volume  \u00b7  M: Mute  \u00b7  Ctrl+O: Open  \u00b7  Ctrl+,: Settings";

            // ════════════════════════════════════════════════════════════════
            // Form1
            // ════════════════════════════════════════════════════════════════
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            BackColor           = System.Drawing.Color.FromArgb(11, 14, 24);
            ClientSize          = new System.Drawing.Size(1020, 680);
            Controls.Add(rootLayout);
            Controls.Add(statusStrip1);
            Controls.Add(menuStrip1);
            MinimumSize         = new System.Drawing.Size(820, 580);
            MainMenuStrip       = menuStrip1;
            Name                = "Form1";
            Padding             = new System.Windows.Forms.Padding(0);
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
            visualizerNavPanel.ResumeLayout(false);
            visualizerNavPanel.PerformLayout();
            settingsPanel.ResumeLayout(false);
            settingsPanel.PerformLayout();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
