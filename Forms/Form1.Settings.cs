namespace AudioPlayer;

public partial class Form1
{
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
        var hasAlbumArt = visualizerAlbumArt is not null;
        var availableModes = VisualizerCatalog.GetOptions(hasAlbumArt);
        selectedMode = VisualizerCatalog.GetPreferredMode(selectedMode, hasAlbumArt);

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
        VisualizerCatalog.GetOptions(visualizerAlbumArt is not null);

    private static SelectionOption<VisualizerMode>[] GetAllVisualizerModeOptions() =>
        VisualizerCatalog.GetOptions(includeAlbumArtDependent: true);

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
}
