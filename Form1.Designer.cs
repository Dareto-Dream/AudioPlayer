namespace AudioPlayer
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        // Layout
        private System.Windows.Forms.TableLayoutPanel rootLayout;
        private System.Windows.Forms.TableLayoutPanel headerLayout;
        private System.Windows.Forms.TableLayoutPanel transportLayout;
        private System.Windows.Forms.FlowLayoutPanel leftButtonsPanel;
        private System.Windows.Forms.FlowLayoutPanel rightControlsPanel;
        private System.Windows.Forms.TableLayoutPanel seekLayout;

        // Header labels
        private System.Windows.Forms.Label lblSectionCaption;
        private System.Windows.Forms.Label lblNowPlaying;
        private System.Windows.Forms.Label lblTrackInfo;

        // Transport buttons
        private ModernButton btnOpen;
        private ModernButton btnPlayPause;
        private ModernButton btnStop;
        private ModernButton btnMute;
        private ModernButton btnDefaultApp;

        // Volume
        private System.Windows.Forms.Label lblVolumeCaption;
        private ModernSlider trackBarVolume;
        private System.Windows.Forms.Label lblVolumeValue;

        // Settings strip
        private System.Windows.Forms.FlowLayoutPanel settingsPanel;
        private System.Windows.Forms.Label lblVisualizerModeCaption;
        private System.Windows.Forms.ComboBox cmbVisualizerMode;
        private System.Windows.Forms.CheckBox chkPeakHold;
        private System.Windows.Forms.Label lblSampleRateCaption;
        private System.Windows.Forms.ComboBox cmbSampleRate;
        private System.Windows.Forms.Label lblSensitivityCaption;
        private ModernSlider trackBarSensitivity;

        // Visualizer
        private SpectrumVisualizerControl visualizerControl;

        // Seek row
        private System.Windows.Forms.Label lblCurrentTime;
        private ModernSlider trackBarSeek;
        private System.Windows.Forms.Label lblDuration;

        // Timer & Status
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

            // Instantiate all controls
            rootLayout = new System.Windows.Forms.TableLayoutPanel();
            headerLayout = new System.Windows.Forms.TableLayoutPanel();
            transportLayout = new System.Windows.Forms.TableLayoutPanel();
            leftButtonsPanel = new System.Windows.Forms.FlowLayoutPanel();
            rightControlsPanel = new System.Windows.Forms.FlowLayoutPanel();
            seekLayout = new System.Windows.Forms.TableLayoutPanel();

            lblSectionCaption = new System.Windows.Forms.Label();
            lblNowPlaying = new System.Windows.Forms.Label();
            lblTrackInfo = new System.Windows.Forms.Label();

            btnOpen = new ModernButton();
            btnPlayPause = new ModernButton();
            btnStop = new ModernButton();
            btnMute = new ModernButton();
            btnDefaultApp = new ModernButton();

            lblVolumeCaption = new System.Windows.Forms.Label();
            trackBarVolume = new ModernSlider();
            lblVolumeValue = new System.Windows.Forms.Label();

            settingsPanel = new System.Windows.Forms.FlowLayoutPanel();
            lblVisualizerModeCaption = new System.Windows.Forms.Label();
            cmbVisualizerMode = new System.Windows.Forms.ComboBox();
            chkPeakHold = new System.Windows.Forms.CheckBox();
            lblSampleRateCaption = new System.Windows.Forms.Label();
            cmbSampleRate = new System.Windows.Forms.ComboBox();
            lblSensitivityCaption = new System.Windows.Forms.Label();
            trackBarSensitivity = new ModernSlider();

            visualizerControl = new SpectrumVisualizerControl();

            lblCurrentTime = new System.Windows.Forms.Label();
            trackBarSeek = new ModernSlider();
            lblDuration = new System.Windows.Forms.Label();

            timer1 = new System.Windows.Forms.Timer(components);
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            toolStripOutputLabel = new System.Windows.Forms.ToolStripStatusLabel();
            toolStripHintLabel = new System.Windows.Forms.ToolStripStatusLabel();
            toolTip1 = new System.Windows.Forms.ToolTip(components);

            rootLayout.SuspendLayout();
            headerLayout.SuspendLayout();
            transportLayout.SuspendLayout();
            leftButtonsPanel.SuspendLayout();
            rightControlsPanel.SuspendLayout();
            seekLayout.SuspendLayout();
            settingsPanel.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();

            // ── rootLayout ────────────────────────────────────────────────
            rootLayout.ColumnCount = 1;
            rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            rootLayout.Controls.Add(headerLayout, 0, 0);
            rootLayout.Controls.Add(visualizerControl, 0, 1);
            rootLayout.Controls.Add(seekLayout, 0, 2);
            rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            rootLayout.Location = new System.Drawing.Point(18, 18);
            rootLayout.Margin = new System.Windows.Forms.Padding(0);
            rootLayout.Name = "rootLayout";
            rootLayout.RowCount = 3;
            rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            rootLayout.TabIndex = 0;

            // ── headerLayout ─────────────────────────────────────────────
            headerLayout.AutoSize = true;
            headerLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            headerLayout.ColumnCount = 1;
            headerLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            headerLayout.Controls.Add(lblSectionCaption, 0, 0);
            headerLayout.Controls.Add(lblNowPlaying, 0, 1);
            headerLayout.Controls.Add(lblTrackInfo, 0, 2);
            headerLayout.Controls.Add(transportLayout, 0, 3);
            headerLayout.Controls.Add(settingsPanel, 0, 4);
            headerLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            headerLayout.Location = new System.Drawing.Point(0, 0);
            headerLayout.Margin = new System.Windows.Forms.Padding(0);
            headerLayout.Name = "headerLayout";
            headerLayout.RowCount = 5;
            headerLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            headerLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            headerLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            headerLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            headerLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            headerLayout.TabIndex = 0;

            // ── lblSectionCaption ─────────────────────────────────────────
            lblSectionCaption.AutoSize = true;
            lblSectionCaption.Font = new System.Drawing.Font("Segoe UI Semibold", 8F, System.Drawing.FontStyle.Bold);
            lblSectionCaption.ForeColor = System.Drawing.Color.FromArgb(130, 150, 195);
            lblSectionCaption.Location = new System.Drawing.Point(0, 0);
            lblSectionCaption.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
            lblSectionCaption.Name = "lblSectionCaption";
            lblSectionCaption.Text = "NOW PLAYING";

            // ── lblNowPlaying ─────────────────────────────────────────────
            lblNowPlaying.AutoEllipsis = true;
            lblNowPlaying.AutoSize = false;
            lblNowPlaying.Dock = System.Windows.Forms.DockStyle.Fill;
            lblNowPlaying.Font = new System.Drawing.Font("Segoe UI Semibold", 22F, System.Drawing.FontStyle.Bold);
            lblNowPlaying.ForeColor = System.Drawing.Color.FromArgb(235, 241, 255);
            lblNowPlaying.Location = new System.Drawing.Point(0, 18);
            lblNowPlaying.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
            lblNowPlaying.Name = "lblNowPlaying";
            lblNowPlaying.Size = new System.Drawing.Size(1018, 44);
            lblNowPlaying.Text = "Drop audio here or use Open";

            // ── lblTrackInfo ──────────────────────────────────────────────
            lblTrackInfo.AutoSize = true;
            lblTrackInfo.ForeColor = System.Drawing.Color.FromArgb(155, 170, 205);
            lblTrackInfo.Location = new System.Drawing.Point(0, 67);
            lblTrackInfo.Margin = new System.Windows.Forms.Padding(0, 0, 0, 14);
            lblTrackInfo.MaximumSize = new System.Drawing.Size(980, 0);
            lblTrackInfo.Name = "lblTrackInfo";
            lblTrackInfo.Text = "Supports MP3, WAV, FLAC, AAC, M4A, WMA, OGG Vorbis, AIFF, Opus, WebM, 3GP and more through installed Windows codecs.";

            // ── transportLayout ───────────────────────────────────────────
            transportLayout.AutoSize = true;
            transportLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            transportLayout.ColumnCount = 2;
            transportLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            transportLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            transportLayout.Controls.Add(leftButtonsPanel, 0, 0);
            transportLayout.Controls.Add(rightControlsPanel, 1, 0);
            transportLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            transportLayout.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            transportLayout.Name = "transportLayout";
            transportLayout.RowCount = 1;
            transportLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());

            // ── leftButtonsPanel ──────────────────────────────────────────
            leftButtonsPanel.AutoSize = true;
            leftButtonsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            leftButtonsPanel.Controls.Add(btnOpen);
            leftButtonsPanel.Controls.Add(btnPlayPause);
            leftButtonsPanel.Controls.Add(btnStop);
            leftButtonsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            leftButtonsPanel.Margin = new System.Windows.Forms.Padding(0);
            leftButtonsPanel.Name = "leftButtonsPanel";

            // ── rightControlsPanel ────────────────────────────────────────
            rightControlsPanel.AutoSize = true;
            rightControlsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            rightControlsPanel.Controls.Add(lblVolumeCaption);
            rightControlsPanel.Controls.Add(btnMute);
            rightControlsPanel.Controls.Add(trackBarVolume);
            rightControlsPanel.Controls.Add(lblVolumeValue);
            rightControlsPanel.Controls.Add(btnDefaultApp);
            rightControlsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            rightControlsPanel.Margin = new System.Windows.Forms.Padding(0);
            rightControlsPanel.Name = "rightControlsPanel";
            rightControlsPanel.WrapContents = false;

            // ── btnOpen ───────────────────────────────────────────────────
            btnOpen.AccentColor = System.Drawing.Color.FromArgb(45, 70, 130);
            btnOpen.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            btnOpen.ForeColor = System.Drawing.Color.White;
            btnOpen.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
            btnOpen.Name = "btnOpen";
            btnOpen.Size = new System.Drawing.Size(110, 46);
            btnOpen.TabIndex = 0;
            btnOpen.Text = "Open";
            btnOpen.Click += btnOpen_Click;
            toolTip1.SetToolTip(btnOpen, "Open audio file (Ctrl+O)");

            // ── btnPlayPause ──────────────────────────────────────────────
            btnPlayPause.AccentColor = System.Drawing.Color.FromArgb(28, 148, 110);
            btnPlayPause.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            btnPlayPause.ForeColor = System.Drawing.Color.White;
            btnPlayPause.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
            btnPlayPause.Name = "btnPlayPause";
            btnPlayPause.Size = new System.Drawing.Size(120, 46);
            btnPlayPause.TabIndex = 1;
            btnPlayPause.Text = "Open Audio";
            btnPlayPause.Click += btnPlayPause_Click;
            toolTip1.SetToolTip(btnPlayPause, "Play / Pause (Space)");

            // ── btnStop ───────────────────────────────────────────────────
            btnStop.AccentColor = System.Drawing.Color.FromArgb(140, 60, 68);
            btnStop.Enabled = false;
            btnStop.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            btnStop.ForeColor = System.Drawing.Color.White;
            btnStop.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            btnStop.Name = "btnStop";
            btnStop.Size = new System.Drawing.Size(100, 46);
            btnStop.TabIndex = 2;
            btnStop.Text = "Stop";
            btnStop.Click += btnStop_Click;
            toolTip1.SetToolTip(btnStop, "Stop playback (Escape)");

            // ── lblVolumeCaption ──────────────────────────────────────────
            lblVolumeCaption.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblVolumeCaption.AutoSize = true;
            lblVolumeCaption.ForeColor = System.Drawing.Color.FromArgb(155, 170, 205);
            lblVolumeCaption.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            lblVolumeCaption.Margin = new System.Windows.Forms.Padding(24, 0, 8, 0);
            lblVolumeCaption.Name = "lblVolumeCaption";
            lblVolumeCaption.Text = "Volume";

            // ── btnMute ───────────────────────────────────────────────────
            btnMute.AccentColor = System.Drawing.Color.FromArgb(38, 52, 90);
            btnMute.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            btnMute.ForeColor = System.Drawing.Color.FromArgb(200, 220, 255);
            btnMute.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            btnMute.Name = "btnMute";
            btnMute.Size = new System.Drawing.Size(46, 46);
            btnMute.TabIndex = 3;
            btnMute.Text = "Mute";
            btnMute.Click += btnMute_Click;
            toolTip1.SetToolTip(btnMute, "Toggle mute (M)");

            // ── trackBarVolume ────────────────────────────────────────────
            trackBarVolume.IsLarge = false;
            trackBarVolume.Maximum = 100;
            trackBarVolume.Minimum = 0;
            trackBarVolume.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            trackBarVolume.Name = "trackBarVolume";
            trackBarVolume.Size = new System.Drawing.Size(148, 46);
            trackBarVolume.TabIndex = 4;
            trackBarVolume.Value = 85;
            trackBarVolume.Scroll += trackBarVolume_Scroll;
            toolTip1.SetToolTip(trackBarVolume, "Volume (Up/Down arrows)");

            // ── lblVolumeValue ────────────────────────────────────────────
            lblVolumeValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblVolumeValue.AutoSize = true;
            lblVolumeValue.ForeColor = System.Drawing.Color.FromArgb(200, 220, 255);
            lblVolumeValue.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            lblVolumeValue.Margin = new System.Windows.Forms.Padding(0, 0, 16, 0);
            lblVolumeValue.MinimumSize = new System.Drawing.Size(38, 0);
            lblVolumeValue.Name = "lblVolumeValue";
            lblVolumeValue.Text = "85%";

            // ── btnDefaultApp ─────────────────────────────────────────────
            btnDefaultApp.AccentColor = System.Drawing.Color.FromArgb(62, 52, 118);
            btnDefaultApp.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            btnDefaultApp.ForeColor = System.Drawing.Color.White;
            btnDefaultApp.Margin = new System.Windows.Forms.Padding(0);
            btnDefaultApp.Name = "btnDefaultApp";
            btnDefaultApp.Size = new System.Drawing.Size(130, 46);
            btnDefaultApp.TabIndex = 5;
            btnDefaultApp.Text = "Set as Default...";
            btnDefaultApp.Click += btnDefaultApp_Click;
            toolTip1.SetToolTip(btnDefaultApp, "Register as default audio player for Windows");

            // ── settingsPanel ─────────────────────────────────────────────
            settingsPanel.AutoSize = true;
            settingsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            settingsPanel.Controls.Add(lblVisualizerModeCaption);
            settingsPanel.Controls.Add(cmbVisualizerMode);
            settingsPanel.Controls.Add(chkPeakHold);
            settingsPanel.Controls.Add(lblSampleRateCaption);
            settingsPanel.Controls.Add(cmbSampleRate);
            settingsPanel.Controls.Add(lblSensitivityCaption);
            settingsPanel.Controls.Add(trackBarSensitivity);
            settingsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            settingsPanel.Location = new System.Drawing.Point(0, 0);
            settingsPanel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            settingsPanel.Name = "settingsPanel";
            settingsPanel.WrapContents = false;

            // ── lblVisualizerModeCaption ──────────────────────────────────
            lblVisualizerModeCaption.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblVisualizerModeCaption.AutoSize = true;
            lblVisualizerModeCaption.ForeColor = System.Drawing.Color.FromArgb(155, 170, 205);
            lblVisualizerModeCaption.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            lblVisualizerModeCaption.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            lblVisualizerModeCaption.Name = "lblVisualizerModeCaption";
            lblVisualizerModeCaption.Text = "Visualizer";

            // ── cmbVisualizerMode ─────────────────────────────────────────
            cmbVisualizerMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbVisualizerMode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            cmbVisualizerMode.BackColor = System.Drawing.Color.FromArgb(24, 30, 46);
            cmbVisualizerMode.ForeColor = System.Drawing.Color.FromArgb(220, 230, 255);
            cmbVisualizerMode.FormattingEnabled = true;
            cmbVisualizerMode.Margin = new System.Windows.Forms.Padding(0, 0, 18, 0);
            cmbVisualizerMode.Name = "cmbVisualizerMode";
            cmbVisualizerMode.Size = new System.Drawing.Size(155, 23);
            cmbVisualizerMode.TabIndex = 1;
            cmbVisualizerMode.SelectedIndexChanged += cmbVisualizerMode_SelectedIndexChanged;

            // ── chkPeakHold ───────────────────────────────────────────────
            chkPeakHold.Anchor = System.Windows.Forms.AnchorStyles.Left;
            chkPeakHold.AutoSize = true;
            chkPeakHold.ForeColor = System.Drawing.Color.FromArgb(185, 200, 230);
            chkPeakHold.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            chkPeakHold.Margin = new System.Windows.Forms.Padding(0, 0, 20, 0);
            chkPeakHold.Name = "chkPeakHold";
            chkPeakHold.Text = "Peak hold";
            chkPeakHold.UseVisualStyleBackColor = false;
            chkPeakHold.BackColor = System.Drawing.Color.Transparent;
            chkPeakHold.TabIndex = 2;
            chkPeakHold.CheckedChanged += chkPeakHold_CheckedChanged;

            // ── lblSampleRateCaption ──────────────────────────────────────
            lblSampleRateCaption.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblSampleRateCaption.AutoSize = true;
            lblSampleRateCaption.ForeColor = System.Drawing.Color.FromArgb(155, 170, 205);
            lblSampleRateCaption.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            lblSampleRateCaption.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            lblSampleRateCaption.Name = "lblSampleRateCaption";
            lblSampleRateCaption.Text = "Output rate";

            // ── cmbSampleRate ─────────────────────────────────────────────
            cmbSampleRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbSampleRate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            cmbSampleRate.BackColor = System.Drawing.Color.FromArgb(24, 30, 46);
            cmbSampleRate.ForeColor = System.Drawing.Color.FromArgb(220, 230, 255);
            cmbSampleRate.FormattingEnabled = true;
            cmbSampleRate.Margin = new System.Windows.Forms.Padding(0, 0, 20, 0);
            cmbSampleRate.Name = "cmbSampleRate";
            cmbSampleRate.Size = new System.Drawing.Size(138, 23);
            cmbSampleRate.TabIndex = 4;
            cmbSampleRate.SelectedIndexChanged += cmbSampleRate_SelectedIndexChanged;

            // ── lblSensitivityCaption ─────────────────────────────────────
            lblSensitivityCaption.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblSensitivityCaption.AutoSize = true;
            lblSensitivityCaption.ForeColor = System.Drawing.Color.FromArgb(155, 170, 205);
            lblSensitivityCaption.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            lblSensitivityCaption.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            lblSensitivityCaption.Name = "lblSensitivityCaption";
            lblSensitivityCaption.Text = "Sensitivity";

            // ── trackBarSensitivity ───────────────────────────────────────
            trackBarSensitivity.IsLarge = false;
            trackBarSensitivity.Maximum = 200;
            trackBarSensitivity.Minimum = 50;
            trackBarSensitivity.Margin = new System.Windows.Forms.Padding(0);
            trackBarSensitivity.Name = "trackBarSensitivity";
            trackBarSensitivity.Size = new System.Drawing.Size(120, 28);
            trackBarSensitivity.TabIndex = 6;
            trackBarSensitivity.Value = 100;
            trackBarSensitivity.Scroll += trackBarSensitivity_Scroll;
            toolTip1.SetToolTip(trackBarSensitivity, "Visualizer sensitivity");

            // ── visualizerControl ─────────────────────────────────────────
            visualizerControl.Dock = System.Windows.Forms.DockStyle.Fill;
            visualizerControl.Margin = new System.Windows.Forms.Padding(0, 14, 0, 10);
            visualizerControl.Mode = AudioPlayer.VisualizerMode.MirrorSpectrum;
            visualizerControl.Name = "visualizerControl";
            visualizerControl.ShowPeaks = true;
            visualizerControl.TabIndex = 1;

            // ── seekLayout ────────────────────────────────────────────────
            seekLayout.AutoSize = true;
            seekLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            seekLayout.ColumnCount = 3;
            seekLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 64F));
            seekLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            seekLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 64F));
            seekLayout.Controls.Add(lblCurrentTime, 0, 0);
            seekLayout.Controls.Add(trackBarSeek, 1, 0);
            seekLayout.Controls.Add(lblDuration, 2, 0);
            seekLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            seekLayout.Margin = new System.Windows.Forms.Padding(0);
            seekLayout.Name = "seekLayout";
            seekLayout.RowCount = 1;
            seekLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            seekLayout.TabIndex = 2;

            // ── lblCurrentTime ────────────────────────────────────────────
            lblCurrentTime.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblCurrentTime.AutoSize = true;
            lblCurrentTime.Cursor = System.Windows.Forms.Cursors.Hand;
            lblCurrentTime.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            lblCurrentTime.ForeColor = System.Drawing.Color.FromArgb(200, 220, 255);
            lblCurrentTime.Margin = new System.Windows.Forms.Padding(0, 10, 4, 0);
            lblCurrentTime.Name = "lblCurrentTime";
            lblCurrentTime.Text = "0:00";
            lblCurrentTime.Click += lblCurrentTime_Click;
            toolTip1.SetToolTip(lblCurrentTime, "Click to toggle remaining time");

            // ── trackBarSeek ──────────────────────────────────────────────
            trackBarSeek.IsLarge = true;
            trackBarSeek.Dock = System.Windows.Forms.DockStyle.Fill;
            trackBarSeek.Enabled = false;
            trackBarSeek.Maximum = 100;
            trackBarSeek.Minimum = 0;
            trackBarSeek.Margin = new System.Windows.Forms.Padding(0);
            trackBarSeek.Name = "trackBarSeek";
            trackBarSeek.TabIndex = 1;
            trackBarSeek.Scroll += trackBarSeek_Scroll;
            toolTip1.SetToolTip(trackBarSeek, "Seek (Left/Right arrows for 5s, Shift for 30s)");

            // ── lblDuration ───────────────────────────────────────────────
            lblDuration.Anchor = System.Windows.Forms.AnchorStyles.Right;
            lblDuration.AutoSize = true;
            lblDuration.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            lblDuration.ForeColor = System.Drawing.Color.FromArgb(140, 158, 195);
            lblDuration.Margin = new System.Windows.Forms.Padding(4, 10, 0, 0);
            lblDuration.Name = "lblDuration";
            lblDuration.Text = "0:00";

            // ── timer1 ────────────────────────────────────────────────────
            timer1.Interval = 33;
            timer1.Tick += timer1_Tick;
            timer1.Start();

            // ── statusStrip1 ──────────────────────────────────────────────
            statusStrip1.BackColor = System.Drawing.Color.FromArgb(10, 14, 26);
            statusStrip1.ForeColor = System.Drawing.Color.FromArgb(155, 170, 205);
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[]
            {
                toolStripStatusLabel, toolStripOutputLabel, toolStripHintLabel
            });
            statusStrip1.Padding = new System.Windows.Forms.Padding(2, 0, 16, 0);
            statusStrip1.SizingGrip = false;
            statusStrip1.TabIndex = 1;

            // ── toolStripStatusLabel ──────────────────────────────────────
            toolStripStatusLabel.ForeColor = System.Drawing.Color.FromArgb(130, 220, 160);
            toolStripStatusLabel.Name = "toolStripStatusLabel";
            toolStripStatusLabel.Text = "Ready";

            // ── toolStripOutputLabel ──────────────────────────────────────
            toolStripOutputLabel.ForeColor = System.Drawing.Color.FromArgb(155, 170, 205);
            toolStripOutputLabel.Margin = new System.Windows.Forms.Padding(16, 3, 0, 2);
            toolStripOutputLabel.Name = "toolStripOutputLabel";
            toolStripOutputLabel.Text = "Output: Match source";

            // ── toolStripHintLabel ────────────────────────────────────────
            toolStripHintLabel.ForeColor = System.Drawing.Color.FromArgb(100, 115, 155);
            toolStripHintLabel.Margin = new System.Windows.Forms.Padding(16, 3, 0, 2);
            toolStripHintLabel.Name = "toolStripHintLabel";
            toolStripHintLabel.Text = "Space: Play/Pause  |  \u2190\u2192: Seek 5s  |  Shift+\u2190\u2192: Seek 30s  |  Up/Down: Volume  |  M: Mute  |  Ctrl+O: Open";

            // ── Form1 ─────────────────────────────────────────────────────
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(15, 19, 32);
            ClientSize = new System.Drawing.Size(1054, 680);
            Controls.Add(rootLayout);
            Controls.Add(statusStrip1);
            MinimumSize = new System.Drawing.Size(860, 600);
            Name = "Form1";
            Padding = new System.Windows.Forms.Padding(18);
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Audio Player";
            Load += Form1_Load;

            rootLayout.ResumeLayout(false);
            headerLayout.ResumeLayout(false);
            headerLayout.PerformLayout();
            transportLayout.ResumeLayout(false);
            transportLayout.PerformLayout();
            leftButtonsPanel.ResumeLayout(false);
            rightControlsPanel.ResumeLayout(false);
            rightControlsPanel.PerformLayout();
            settingsPanel.ResumeLayout(false);
            settingsPanel.PerformLayout();
            seekLayout.ResumeLayout(false);
            seekLayout.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
