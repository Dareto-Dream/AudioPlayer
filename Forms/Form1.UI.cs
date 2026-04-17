namespace Spectrallis;

public partial class Form1
{
    private void UpdateSeekBar()
    {
        var length = engine.GetLength();
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
        visualizerControl.UpdateFrame(engine.GetVisualizerFrame(), engine.IsPlaying, engine.GetPosition());

        // Sync embedded video position with audio playback
        if (embeddedContentControl?.HasContent == true)
        {
            var position = engine.GetPosition();
            _ = embeddedContentControl.SyncVideoPosition(position);
        }
    }

    private void UpdateUiState()
    {
        var track = engine.CurrentTrack;
        EnsureEffectiveTheme();
        UpdateAlbumArt(track);

        btnPlayPause.Text = track is null
            ? "Open Audio"
            : engine.IsPlaying ? "Pause" : "Play";
        btnPlayPause.AccentColor = track is null ? AccentSecondaryColor : AccentPrimaryColor;
        btnPlayPause.ForeColor = AccentContrastColor;

        btnStop.Enabled = track is not null;
        trackBarSeek.Enabled = track is not null;

        lblNowPlaying.Text = track?.DisplayName ?? "Drop a file here or press Play";
        lblTrackInfo.Text = BuildTrackInfoText(track);
        ApplyInformationVisibility();
        SetLyricsVisible(track?.Lyrics is not null);
        lyricsView.UpdateState(track, engine.GetPosition());

        if (isMuted || trackBarVolume.Value == 0)
        {
            lblVolumeValue.Text = "Muted";
            lblVolumeValue.ForeColor = TextMutedColor;
            btnMute.Text = "Unmute";
            btnMute.AccentColor = DangerColor;
            btnMute.ForeColor = DangerTextColor;
        }
        else
        {
            lblVolumeValue.Text = $"{trackBarVolume.Value}%";
            lblVolumeValue.ForeColor = TextSecondaryColor;
            btnMute.Text = "Mute";
            btnMute.AccentColor = AccentSoftColor;
            btnMute.ForeColor = TextSecondaryColor;
        }

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
        toolTip1.SetToolTip(cmbVisualizerMode, GetVisualizerModeToolTip());

        UpdateMenuState();
        Text = track is null ? "Audio Player" : BuildWindowTitle(track);
        SyncNowPlayingState();
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
        const string separator = "  \u00B7  ";

        if (track is null)
        {
            return "Supports MP3, WAV, FLAC, AAC, M4A, WMA, OGG Vorbis, AIFF, Opus, WebM, 3GP and more through installed Windows codecs.";
        }

        var descriptiveParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(track.Artist))
            descriptiveParts.Add(track.Artist);

        if (!string.IsNullOrWhiteSpace(track.Album))
            descriptiveParts.Add(track.Album);

        var technicalLine =
            $"{track.FormatName}{separator}{track.Channels} ch{separator}{track.SourceSampleRate / 1000d:0.#} kHz{separator}{FormatBitDepth(track.BitsPerSample)}{separator}{FormatTime((float)track.Duration.TotalSeconds)}";

        if (track.Lyrics is not null)
        {
            technicalLine += track.Lyrics.HasWordTimings
                ? $"{separator}Enhanced lyrics"
                : $"{separator}Synced lyrics";
        }

        if (track.EmbeddedVisualizer is not null)
        {
            technicalLine += $"{separator}Embedded visualizer";
        }

        if (track.EmbeddedTheme is not null)
        {
            technicalLine += $"{separator}Embedded theme";
        }

        return descriptiveParts.Count == 0
            ? technicalLine
            : $"{string.Join(separator, descriptiveParts)}{Environment.NewLine}{technicalLine}";
    }

    private static string BuildWindowTitle(AudioTrackInfo track) =>
        string.IsNullOrWhiteSpace(track.Artist)
            ? $"{track.DisplayName}  \u2014  Audio Player"
            : $"{track.Artist} - {track.DisplayName}  \u2014  Audio Player";

    private void SetLyricsVisible(bool visible)
    {
        lyricsView.Visible = visible;
        contentLayout.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, visible ? 58F : 100F);
        contentLayout.ColumnStyles[1] = visible
            ? new ColumnStyle(SizeType.Percent, 42F)
            : new ColumnStyle(SizeType.Absolute, 0F);
        contentLayout.PerformLayout();
    }

    private void ShowError(string message, string title)
    {
        MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
