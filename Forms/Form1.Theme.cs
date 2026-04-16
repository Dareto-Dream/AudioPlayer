using System.Drawing;

namespace AudioPlayer;

public partial class Form1
{
    private void ApplyTheme()
    {
        themePalette = ThemePalette.Create(appSettings.ThemeMode, appSettings.ThemeAccent);
        WindowChromeStyler.ApplyTheme(this, themePalette);

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
        toolTip1.SetToolTip(cmbVisualizerMode, GetVisualizerCycleToolTip());
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

    private void ApplyMenuTheme(ToolStripItemCollection items)
    {
        foreach (ToolStripItem item in items)
        {
            item.ForeColor = TextPrimaryColor;

            if (item is ToolStripMenuItem menuItem)
                ApplyMenuTheme(menuItem.DropDownItems);
        }
    }
}
