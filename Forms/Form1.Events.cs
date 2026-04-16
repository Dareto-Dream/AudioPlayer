namespace AudioPlayer;

public partial class Form1
{
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (ActiveControl is ComboBox)
            return base.ProcessCmdKey(ref msg, keyData);

        switch (keyData)
        {
            case Keys.Space:
                btnPlayPause_Click(this, EventArgs.Empty);
                return true;

            case Keys.Escape:
                if (engine.IsLoaded)
                {
                    engine.Stop();
                    UpdateUiState();
                }
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

    private void lblCurrentTime_Click(object sender, EventArgs e)
    {
        showRemainingTime = !showRemainingTime;
        UpdateSeekBar();
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
        UpdateSeekBar();
        UpdateVisualizer();
        CycleVisualizerIfDue();
        UpdateUiState();
    }

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

    private void WireFileDrop(Control control)
    {
        if (control is not (ButtonBase or ModernButton or ModernSlider or ComboBox or CheckBox or ListBox))
        {
            control.AllowDrop = true;
            control.DragEnter -= Form1_DragEnter;
            control.DragDrop -= Form1_DragDrop;
            control.DragEnter += Form1_DragEnter;
            control.DragDrop += Form1_DragDrop;
        }

        foreach (Control child in control.Controls)
            WireFileDrop(child);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        timer1.Stop();
        DisposeDisplayedArtwork();
        visualizerAlbumArt?.Dispose();
        nowPlaying.CommandRequested -= NowPlaying_CommandRequested;
        nowPlaying.Dispose();
        engine.Dispose();
        base.OnFormClosed(e);
    }

    private void fileOpenToolStripMenuItem_Click(object sender, EventArgs e) => OpenAudioFile(startPlayback: true);

    private void fileSettingsToolStripMenuItem_Click(object sender, EventArgs e) => ShowSettingsDialog();

    private void fileSetDefaultToolStripMenuItem_Click(object sender, EventArgs e) => btnDefaultApp_Click(sender, e);

    private void fileExitToolStripMenuItem_Click(object sender, EventArgs e) => Close();

    private void playbackPlayPauseToolStripMenuItem_Click(object sender, EventArgs e) => btnPlayPause_Click(sender, e);

    private void playbackStopToolStripMenuItem_Click(object sender, EventArgs e) => btnStop_Click(sender, e);

    private void playbackMuteToolStripMenuItem_Click(object sender, EventArgs e) => btnMute_Click(sender, e);
}
