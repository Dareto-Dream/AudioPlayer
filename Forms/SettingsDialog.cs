using System.Drawing;

namespace Spectrallis;

internal sealed class SettingsDialog : Form
{
    private readonly AppSettings workingSettings;
    private readonly SelectionOption<VisualizerMode>[] currentVisualizerOptions;
    private readonly string? activeEmbeddedVisualizerLabel;
    private readonly string? activeEmbeddedThemeLabel;
    private readonly List<Panel> sectionPanels = [];
    private readonly List<Control> sectionSurfaceControls = [];
    private readonly List<Label> fieldTitleLabels = [];
    private readonly List<Label> fieldDescriptionLabels = [];
    private readonly List<Label> sectionTitleLabels = [];
    private readonly List<Label> sectionSubtitleLabels = [];

    private readonly TableLayoutPanel rootLayout;
    private readonly Panel headerPanel;
    private readonly FlowLayoutPanel bodyFlow;
    private readonly TableLayoutPanel footerLayout;
    private readonly Label lblTitle;
    private readonly Label lblSubtitle;
    private readonly Panel accentPreview;
    private readonly ModernComboBox cmbThemeMode;
    private readonly ModernComboBox cmbAccent;
    private readonly ModernComboBox cmbCurrentVisualizer;
    private readonly ModernComboBox cmbDefaultVisualizer;
    private readonly ModernComboBox cmbPlaybackRate;
    private readonly ModernComboBox cmbCycleDuration;
    private readonly ModernSwitch chkShowMoreInfo;
    private readonly ModernSwitch chkUseEmbeddedThemes;
    private readonly ModernSwitch chkPeakHold;
    private readonly ModernSwitch chkAutoCycle;
    private readonly ModernSwitch chkAutoPlayOnOpen;
    private readonly ModernSwitch chkUseEmbeddedVisualizers;
    private readonly ModernSlider sldSensitivity;
    private readonly ModernSlider sldDefaultVolume;
    private readonly Label lblSensitivityValue;
    private readonly Label lblVolumeValue;
    private readonly ModernButton btnCancel;
    private readonly ModernButton btnSave;
    private readonly ModernButton btnSetDefaultApp;

    public SettingsDialog(
        AppSettings currentSettings,
        IReadOnlyList<SelectionOption<VisualizerMode>> currentVisualizerOptions,
        VisualizerMode currentVisualizer,
        IReadOnlyList<SelectionOption<VisualizerMode>> defaultVisualizerOptions,
        IReadOnlyList<SelectionOption<int>> sampleRateOptions,
        IReadOnlyList<SelectionOption<int>> cycleDurationOptions,
        string? activeEmbeddedVisualizerLabel,
        string? activeEmbeddedThemeLabel)
    {
        workingSettings = currentSettings.Clone();
        this.currentVisualizerOptions = currentVisualizerOptions.ToArray();
        this.activeEmbeddedVisualizerLabel = string.IsNullOrWhiteSpace(activeEmbeddedVisualizerLabel)
            ? null
            : activeEmbeddedVisualizerLabel.Trim();
        this.activeEmbeddedThemeLabel = string.IsNullOrWhiteSpace(activeEmbeddedThemeLabel)
            ? null
            : activeEmbeddedThemeLabel.Trim();

        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(720, 760);
        DoubleBuffered = true;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Padding = new Padding(0);
        ShowIcon = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Settings";
        HandleCreated += (_, _) => ApplyThemePreview();

        rootLayout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            RowCount = 3
        };
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayout.RowStyles.Add(new RowStyle());
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rootLayout.RowStyles.Add(new RowStyle());

        headerPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Height = 90,
            Margin = Padding.Empty,
            Padding = new Padding(28, 22, 28, 18)
        };

        lblTitle = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold, GraphicsUnit.Point),
            Location = new Point(0, 0),
            Text = "Settings"
        };

        lblSubtitle = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
            Location = new Point(0, 38),
            MaximumSize = new Size(620, 0),
            Text = "Appearance, playback, and visualizer defaults for new sessions and files opened through Audio Player."
        };

        headerPanel.Controls.Add(lblTitle);
        headerPanel.Controls.Add(lblSubtitle);

        bodyFlow = new FlowLayoutPanel
        {
            AutoScroll = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            Margin = Padding.Empty,
            Padding = new Padding(24, 0, 18, 0),
            WrapContents = false
        };

        cmbThemeMode = CreateComboBox();
        cmbAccent = CreateComboBox();
        cmbCurrentVisualizer = CreateComboBox();
        cmbDefaultVisualizer = CreateComboBox();
        cmbPlaybackRate = CreateComboBox();
        cmbCycleDuration = CreateComboBox();

        chkShowMoreInfo = CreateSwitch();
        chkUseEmbeddedThemes = CreateSwitch();
        chkPeakHold = CreateSwitch();
        chkAutoCycle = CreateSwitch();
        chkAutoPlayOnOpen = CreateSwitch();
        chkUseEmbeddedVisualizers = CreateSwitch();

        sldSensitivity = CreateSlider();
        sldDefaultVolume = CreateSlider();

        lblSensitivityValue = CreateValueLabel();
        lblVolumeValue = CreateValueLabel();

        accentPreview = new Panel
        {
            Margin = new Padding(10, 0, 0, 0),
            Size = new Size(28, 28)
        };

        btnCancel = new ModernButton
        {
            Size = new Size(120, 42),
            Text = "Cancel"
        };
        btnCancel.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        btnSave = new ModernButton
        {
            Size = new Size(140, 42),
            Text = "Save Settings"
        };
        btnSave.Click += btnSave_Click;

        btnSetDefaultApp = new ModernButton
        {
            Size = new Size(158, 36),
            Text = "Set as Default..."
        };
        btnSetDefaultApp.Click += btnSetDefaultApp_Click;

        var appearanceSection = CreateSection(
            "Appearance",
            "Change the shell theme and how much metadata stays visible by default.");
        appearanceSection.Controls.Add(CreateFieldRow("Theme mode", "Switch between the dark and light shell.", cmbThemeMode));
        appearanceSection.Controls.Add(CreateFieldRow("Theme accent", "Choose the accent color used across buttons, menus, and highlights.", CreateAccentControlHost()));
        appearanceSection.Controls.Add(CreateFieldRow("Use embedded track themes", GetEmbeddedThemeDescription(), chkUseEmbeddedThemes));
        appearanceSection.Controls.Add(CreateFieldRow("Show more info", "Displays the extra artist, format, and duration line under the track title.", chkShowMoreInfo));

        var visualizerSection = CreateSection(
            "Visualizer",
            "Set how the visualizer behaves right now and when new tracks are opened.");
        visualizerSection.Controls.Add(CreateFieldRow("Current visualizer", "Applies immediately to the track that's already loaded.", cmbCurrentVisualizer));
        visualizerSection.Controls.Add(CreateFieldRow("Default visualizer", "Used for newly loaded tracks. Spinning Disk falls back when no album art exists.", cmbDefaultVisualizer));
        visualizerSection.Controls.Add(CreateFieldRow("Use embedded track visualizers", GetEmbeddedVisualizerDescription(), chkUseEmbeddedVisualizers));
        visualizerSection.Controls.Add(CreateFieldRow("Auto-cycle visualizers", "Rotates through the available visualizers while a track is loaded.", chkAutoCycle));
        visualizerSection.Controls.Add(CreateFieldRow("Cycle duration", "How long each visualizer stays on screen before rotating.", cmbCycleDuration));
        visualizerSection.Controls.Add(CreateFieldRow("Peak hold", "Keeps peak markers visible for a brief moment in spectrum views.", chkPeakHold));
        visualizerSection.Controls.Add(CreateFieldRow("Sensitivity", "Controls how aggressively the visualizer reacts to quieter material.", CreateSliderHost(sldSensitivity, lblSensitivityValue)));

        var playbackSection = CreateSection(
            "Playback",
            "Defaults applied to new sessions and files opened through Windows file associations.");
        playbackSection.Controls.Add(CreateFieldRow("Default playback rate", "Preferred output sample rate. Match source leaves the file unchanged.", cmbPlaybackRate));
        playbackSection.Controls.Add(CreateFieldRow("Default volume", "Starting playback volume for a new session.", CreateSliderHost(sldDefaultVolume, lblVolumeValue)));
        playbackSection.Controls.Add(CreateFieldRow("Autoplay on open", "Starts playback automatically after opening a file. This is on by default.", chkAutoPlayOnOpen));

        var integrationSection = CreateSection(
            "Integration",
            "Windows-level behaviors that affect how Audio Player opens files.");
        integrationSection.Controls.Add(CreateFieldRow("Default app", "Registers Audio Player for supported audio extensions and opens Windows Default Apps.", btnSetDefaultApp));

        bodyFlow.Controls.Add(appearanceSection);
        bodyFlow.Controls.Add(visualizerSection);
        bodyFlow.Controls.Add(playbackSection);
        bodyFlow.Controls.Add(integrationSection);

        footerLayout = new TableLayoutPanel
        {
            ColumnCount = 3,
            Dock = DockStyle.Fill,
            Height = 82,
            Margin = Padding.Empty,
            Padding = new Padding(28, 18, 28, 22),
            RowCount = 1
        };
        footerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        footerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        footerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        footerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        btnCancel.Margin = new Padding(0, 0, 10, 0);
        btnSave.Margin = Padding.Empty;

        footerLayout.Controls.Add(btnCancel, 1, 0);
        footerLayout.Controls.Add(btnSave, 2, 0);

        rootLayout.Controls.Add(headerPanel, 0, 0);
        rootLayout.Controls.Add(bodyFlow, 0, 1);
        rootLayout.Controls.Add(footerLayout, 0, 2);

        Controls.Add(rootLayout);

        PopulateComboOptions(currentVisualizer, defaultVisualizerOptions, sampleRateOptions, cycleDurationOptions);
        WireEvents();
        ApplyThemePreview();
    }

    public AppSettings Settings => workingSettings.Clone();

    public VisualizerMode SelectedCurrentVisualizer { get; private set; }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        switch (keyData)
        {
            case Keys.Escape:
                DialogResult = DialogResult.Cancel;
                Close();
                return true;
            case Keys.Enter:
                btnSave_Click(this, EventArgs.Empty);
                return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private string GetEmbeddedThemeDescription() =>
        activeEmbeddedThemeLabel is null
            ? "If a track embeds a theme, the player can lock the shell to it until the track changes."
            : $"The current track embeds the theme '{activeEmbeddedThemeLabel}'. Turn this off to keep using the app theme instead.";

    private string GetEmbeddedVisualizerDescription() =>
        activeEmbeddedVisualizerLabel is null
            ? "If a track embeds a visualizer, the player can lock playback to it instead of built-in visualizers."
            : $"The current track embeds the visualizer '{activeEmbeddedVisualizerLabel}'. Turn this off to pick a built-in visualizer instead.";

    private void PopulateComboOptions(
        VisualizerMode currentVisualizer,
        IReadOnlyList<SelectionOption<VisualizerMode>> defaultVisualizerOptions,
        IReadOnlyList<SelectionOption<int>> sampleRateOptions,
        IReadOnlyList<SelectionOption<int>> cycleDurationOptions)
    {
        cmbThemeMode.Items.AddRange(
            [
                new SelectionOption<ThemeMode>("Dark",     ThemeMode.Dark),
                new SelectionOption<ThemeMode>("Light",    ThemeMode.Light),
                new SelectionOption<ThemeMode>("OLED",     ThemeMode.Oled),
                new SelectionOption<ThemeMode>("Midnight", ThemeMode.Midnight)
            ]);
        cmbAccent.Items.AddRange(
            [
                new SelectionOption<ThemeAccent>("Amber",   ThemeAccent.Amber),
                new SelectionOption<ThemeAccent>("Ocean",   ThemeAccent.Ocean),
                new SelectionOption<ThemeAccent>("Rose",    ThemeAccent.Rose),
                new SelectionOption<ThemeAccent>("Forest",  ThemeAccent.Forest),
                new SelectionOption<ThemeAccent>("Violet",  ThemeAccent.Violet),
                new SelectionOption<ThemeAccent>("Crimson", ThemeAccent.Crimson),
                new SelectionOption<ThemeAccent>("Cyan",    ThemeAccent.Cyan),
                new SelectionOption<ThemeAccent>("Mint",    ThemeAccent.Mint),
                new SelectionOption<ThemeAccent>("Sunset",  ThemeAccent.Sunset),
                new SelectionOption<ThemeAccent>("Gold",    ThemeAccent.Gold)
            ]);
        cmbDefaultVisualizer.Items.AddRange(defaultVisualizerOptions.Select(static option => (object)option).ToArray());
        cmbPlaybackRate.Items.AddRange(sampleRateOptions.Select(static option => (object)option).ToArray());
        cmbCycleDuration.Items.AddRange(cycleDurationOptions.Select(static option => (object)option).ToArray());

        SelectComboValue(cmbThemeMode, workingSettings.ThemeMode);
        SelectComboValue(cmbAccent, workingSettings.ThemeAccent);
        SelectComboValue(cmbDefaultVisualizer, workingSettings.DefaultVisualizer);
        SelectComboValue(cmbPlaybackRate, workingSettings.PreferredSampleRate);
        SelectComboValue(cmbCycleDuration, workingSettings.VisualizerCycleSeconds);

        chkShowMoreInfo.Checked = workingSettings.ShowMoreInfo;
        chkUseEmbeddedThemes.Checked = workingSettings.UseEmbeddedTrackThemes;
        chkPeakHold.Checked = workingSettings.PeakHold;
        chkAutoCycle.Checked = workingSettings.EnableVisualizerAutoCycle;
        chkAutoPlayOnOpen.Checked = workingSettings.AutoPlayOnOpen;
        chkUseEmbeddedVisualizers.Checked = workingSettings.UseEmbeddedTrackVisualizers;

        sldSensitivity.Minimum = 50;
        sldSensitivity.Maximum = 200;
        sldSensitivity.Value = workingSettings.VisualizerSensitivity;
        sldDefaultVolume.Minimum = 0;
        sldDefaultVolume.Maximum = 100;
        sldDefaultVolume.Value = workingSettings.DefaultVolume;
        UpdateSliderLabels();
        UpdateCycleDurationState();
        SelectedCurrentVisualizer = currentVisualizer;
        RefreshCurrentVisualizerOptions();
    }

    private void WireEvents()
    {
        cmbThemeMode.SelectedIndexChanged += (_, _) => ApplyThemePreview();
        cmbAccent.SelectedIndexChanged += (_, _) => ApplyThemePreview();
        cmbCurrentVisualizer.SelectedIndexChanged += (_, _) =>
            SelectedCurrentVisualizer = GetSelectedValue(cmbCurrentVisualizer, SelectedCurrentVisualizer);
        chkAutoCycle.CheckedChanged += (_, _) => UpdateCycleDurationState();
        chkUseEmbeddedVisualizers.CheckedChanged += (_, _) => RefreshCurrentVisualizerOptions();
        sldSensitivity.Scroll += (_, _) => UpdateSliderLabels();
        sldDefaultVolume.Scroll += (_, _) => UpdateSliderLabels();
    }

    private void UpdateSliderLabels()
    {
        lblSensitivityValue.Text = $"{sldSensitivity.Value}%";
        lblVolumeValue.Text = $"{sldDefaultVolume.Value}%";
    }

    private void UpdateCycleDurationState()
    {
        cmbCycleDuration.Enabled = chkAutoCycle.Checked;
    }

    private void RefreshCurrentVisualizerOptions()
    {
        var isLockedToEmbedded = chkUseEmbeddedVisualizers.Checked && activeEmbeddedVisualizerLabel is not null;

        cmbCurrentVisualizer.BeginUpdate();
        try
        {
            cmbCurrentVisualizer.Items.Clear();
            if (isLockedToEmbedded)
            {
                cmbCurrentVisualizer.Items.Add(new SelectionOption<VisualizerMode>(
                    $"Embedded: {activeEmbeddedVisualizerLabel}",
                    SelectedCurrentVisualizer));
                cmbCurrentVisualizer.SelectedIndex = 0;
                cmbCurrentVisualizer.Enabled = false;
                return;
            }

            cmbCurrentVisualizer.Items.AddRange(currentVisualizerOptions.Select(static option => (object)option).ToArray());
            SelectComboValue(cmbCurrentVisualizer, SelectedCurrentVisualizer);
            cmbCurrentVisualizer.Enabled = cmbCurrentVisualizer.Items.Count > 1;
        }
        finally
        {
            cmbCurrentVisualizer.EndUpdate();
        }
    }

    private void ApplyThemePreview()
    {
        var palette = ThemePalette.Create(
            GetSelectedValue(cmbThemeMode, workingSettings.ThemeMode),
            GetSelectedValue(cmbAccent, workingSettings.ThemeAccent));

        WindowChromeStyler.ApplyTheme(this, palette);
        BackColor = palette.WindowBackColor;
        ForeColor = palette.TextPrimaryColor;
        rootLayout.BackColor = palette.WindowBackColor;
        headerPanel.BackColor = palette.WindowBackColor;
        bodyFlow.BackColor = palette.WindowBackColor;
        footerLayout.BackColor = palette.WindowBackColor;
        lblTitle.ForeColor = palette.TextPrimaryColor;
        lblSubtitle.ForeColor = palette.TextSecondaryColor;

        foreach (var section in sectionPanels)
            section.BackColor = palette.SurfaceBackColor;

        foreach (var control in sectionSurfaceControls)
            control.BackColor = palette.SurfaceBackColor;

        foreach (var label in sectionTitleLabels)
            label.ForeColor = palette.TextPrimaryColor;

        foreach (var label in sectionSubtitleLabels)
            label.ForeColor = palette.TextMutedColor;

        foreach (var label in fieldTitleLabels)
            label.ForeColor = palette.TextPrimaryColor;

        foreach (var label in fieldDescriptionLabels)
            label.ForeColor = palette.TextMutedColor;

        lblSensitivityValue.ForeColor = palette.TextSecondaryColor;
        lblVolumeValue.ForeColor = palette.TextSecondaryColor;

        ThemeControlStyler.ApplyComboBoxTheme(cmbThemeMode, palette);
        ThemeControlStyler.ApplyComboBoxTheme(cmbAccent, palette);
        ThemeControlStyler.ApplyComboBoxTheme(cmbCurrentVisualizer, palette);
        ThemeControlStyler.ApplyComboBoxTheme(cmbDefaultVisualizer, palette);
        ThemeControlStyler.ApplyComboBoxTheme(cmbPlaybackRate, palette);
        ThemeControlStyler.ApplyComboBoxTheme(cmbCycleDuration, palette);

        ThemeControlStyler.ApplySliderTheme(sldSensitivity, palette);
        ThemeControlStyler.ApplySliderTheme(sldDefaultVolume, palette);
        ThemeControlStyler.ApplySwitchTheme(chkShowMoreInfo, palette);
        ThemeControlStyler.ApplySwitchTheme(chkUseEmbeddedThemes, palette);
        ThemeControlStyler.ApplySwitchTheme(chkPeakHold, palette);
        ThemeControlStyler.ApplySwitchTheme(chkAutoCycle, palette);
        ThemeControlStyler.ApplySwitchTheme(chkAutoPlayOnOpen, palette);
        ThemeControlStyler.ApplySwitchTheme(chkUseEmbeddedVisualizers, palette);

        ThemeControlStyler.ApplyGhostButtonTheme(btnCancel, palette, palette.BorderStrongColor);
        ThemeControlStyler.ApplyPrimaryButtonTheme(btnSave, palette, palette.AccentPrimaryColor);
        ThemeControlStyler.ApplyGhostButtonTheme(btnSetDefaultApp, palette, palette.AccentSoftColor);

        accentPreview.BackColor = palette.AccentPrimaryColor;
        accentPreview.BorderStyle = BorderStyle.FixedSingle;
    }

    private void btnSave_Click(object? sender, EventArgs e)
    {
        workingSettings.ThemeMode = GetSelectedValue(cmbThemeMode, ThemeMode.Dark);
        workingSettings.ThemeAccent = GetSelectedValue(cmbAccent, ThemeAccent.Amber);
        workingSettings.UseEmbeddedTrackThemes = chkUseEmbeddedThemes.Checked;
        workingSettings.DefaultVisualizer = GetSelectedValue(cmbDefaultVisualizer, VisualizerMode.MirrorSpectrum);
        workingSettings.UseEmbeddedTrackVisualizers = chkUseEmbeddedVisualizers.Checked;
        workingSettings.PreferredSampleRate = GetSelectedValue(cmbPlaybackRate, 0);
        workingSettings.DefaultVolume = sldDefaultVolume.Value;
        workingSettings.PeakHold = chkPeakHold.Checked;
        workingSettings.VisualizerSensitivity = sldSensitivity.Value;
        workingSettings.EnableVisualizerAutoCycle = chkAutoCycle.Checked;
        workingSettings.VisualizerCycleSeconds = GetSelectedValue(cmbCycleDuration, 12);
        workingSettings.AutoPlayOnOpen = chkAutoPlayOnOpen.Checked;
        workingSettings.ShowMoreInfo = chkShowMoreInfo.Checked;
        SelectedCurrentVisualizer = GetSelectedValue(cmbCurrentVisualizer, VisualizerMode.MirrorSpectrum);

        DialogResult = DialogResult.OK;
        Close();
    }

    private void btnSetDefaultApp_Click(object? sender, EventArgs e)
    {
        try
        {
            DefaultAppRegistrar.RegisterCurrentUser();
            DefaultAppRegistrar.OpenDefaultAppsSettings();
            MessageBox.Show(
                this,
                "Audio Player was registered for the current Windows user. Choose it in the Default Apps window that just opened to handle supported audio files.",
                "Default App Registration",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                $"Audio Player could not register itself for Windows file associations.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                "Registration Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private static ModernComboBox CreateComboBox() =>
        new()
        {
            Margin = Padding.Empty,
            Size = new Size(220, 36)
        };

    private static ModernSlider CreateSlider() =>
        new()
        {
            Dock = DockStyle.Fill,
            IsLarge = false,
            Margin = Padding.Empty,
            Size = new Size(164, 32)
        };

    private static ModernSwitch CreateSwitch() =>
        new()
        {
            Anchor = AnchorStyles.Left,
            Margin = Padding.Empty
        };

    private static Label CreateValueLabel() =>
        new()
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI Semibold", 8.75F, FontStyle.Bold, GraphicsUnit.Point),
            Margin = new Padding(10, 0, 0, 0),
            TextAlign = ContentAlignment.MiddleRight,
            Width = 46
        };

    private FlowLayoutPanel CreateSection(string title, string subtitle)
    {
        var section = new FlowLayoutPanel
        {
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 14),
            Padding = new Padding(18, 18, 18, 18),
            FlowDirection = FlowDirection.TopDown,
            Size = new Size(646, 10),
            WrapContents = false
        };

        var lblSectionTitle = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point),
            Margin = Padding.Empty,
            Text = title
        };
        sectionTitleLabels.Add(lblSectionTitle);

        var lblSectionSubtitle = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 8.75F, FontStyle.Regular, GraphicsUnit.Point),
            Margin = new Padding(0, 6, 0, 14),
            MaximumSize = new Size(580, 0),
            Text = subtitle
        };
        sectionSubtitleLabels.Add(lblSectionSubtitle);

        section.Controls.Add(lblSectionTitle);
        section.Controls.Add(lblSectionSubtitle);
        sectionPanels.Add(section);
        sectionSurfaceControls.Add(section);
        return section;
    }

    private Control CreateFieldRow(string title, string description, Control control)
    {
        var row = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 2,
            Margin = new Padding(0, 0, 0, 14),
            MaximumSize = new Size(590, 0),
            MinimumSize = new Size(590, 0),
            Padding = Padding.Empty,
            RowCount = 1
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240F));

        var labelStack = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            RowCount = 2
        };
        labelStack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        labelStack.RowStyles.Add(new RowStyle());
        labelStack.RowStyles.Add(new RowStyle());

        var lblTitle = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold, GraphicsUnit.Point),
            Margin = Padding.Empty,
            Text = title
        };
        fieldTitleLabels.Add(lblTitle);

        var lblDescription = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point),
            Margin = new Padding(0, 5, 0, 0),
            MaximumSize = new Size(340, 0),
            Text = description
        };
        fieldDescriptionLabels.Add(lblDescription);

        labelStack.Controls.Add(lblTitle, 0, 0);
        labelStack.Controls.Add(lblDescription, 0, 1);

        // Don't stretch switches - they have a fixed size
        if (control is not ModernSwitch)
        {
            control.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        }
        else
        {
            control.Anchor = AnchorStyles.Left;
        }
        control.Margin = Padding.Empty;

        row.Controls.Add(labelStack, 0, 0);
        row.Controls.Add(control, 1, 0);
        sectionSurfaceControls.Add(row);
        sectionSurfaceControls.Add(labelStack);
        return row;
    }

    private Control CreateAccentControlHost()
    {
        cmbAccent.Width = 182;

        var host = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            WrapContents = false
        };

        host.Controls.Add(cmbAccent);
        host.Controls.Add(accentPreview);
        sectionSurfaceControls.Add(host);
        return host;
    }

    private Control CreateSliderHost(ModernSlider slider, Label valueLabel)
    {
        var host = new TableLayoutPanel
        {
            ColumnCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            RowCount = 1,
            Size = new Size(220, 36)
        };
        host.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        host.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 52F));
        host.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        host.Controls.Add(slider, 0, 0);
        host.Controls.Add(valueLabel, 1, 0);
        sectionSurfaceControls.Add(host);
        return host;
    }

    private static void SelectComboValue<T>(ModernComboBox comboBox, T value)
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

    private static T GetSelectedValue<T>(ModernComboBox comboBox, T fallback) =>
        comboBox.SelectedItem is SelectionOption<T> option ? option.Value : fallback;
}
