using Windows.Media;
using Windows.Storage.Streams;

namespace AudioPlayer;

internal enum WindowsNowPlayingCommand
{
    Play,
    Pause,
    Stop
}

internal sealed class WindowsNowPlayingService : IDisposable
{
    private SystemMediaTransportControls? controls;
    private IntPtr windowHandle;
    private AudioTrackInfo? currentTrack;
    private InMemoryRandomAccessStream? artworkStream;
    private bool? isEnabled;
    private bool? isPlayEnabled;
    private bool? isPauseEnabled;
    private bool? isStopEnabled;
    private MediaPlaybackStatus? playbackStatus;
    private TimeSpan lastTimelinePosition = TimeSpan.MinValue;
    private TimeSpan lastTimelineDuration = TimeSpan.MinValue;

    public event EventHandler<WindowsNowPlayingCommand>? CommandRequested;

    public void Initialize(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
        {
            return;
        }

        if (controls is not null && windowHandle == handle)
        {
            return;
        }

        ResetTransportControls();

        try
        {
            controls = SystemMediaTransportControlsInterop.GetForWindow(handle);
            windowHandle = handle;
            controls.ButtonPressed += Controls_ButtonPressed;
            InvalidateCaches();
        }
        catch
        {
            controls = null;
            windowHandle = IntPtr.Zero;
        }
    }

    public void Update(AudioTrackInfo? track, bool isPlaying, TimeSpan position, TimeSpan duration)
    {
        if (controls is null)
        {
            return;
        }

        if (track is null)
        {
            Clear();
            return;
        }

        ApplyMetadata(track);
        ApplyTransportState(isPlaying, position);
        ApplyTimeline(position, duration);
    }

    public void Dispose()
    {
        ResetTransportControls();
        GC.SuppressFinalize(this);
    }

    private void ApplyMetadata(AudioTrackInfo track)
    {
        if (controls is null || ReferenceEquals(currentTrack, track))
        {
            return;
        }

        currentTrack = track;
        lastTimelinePosition = TimeSpan.MinValue;
        lastTimelineDuration = TimeSpan.MinValue;

        var updater = controls.DisplayUpdater;
        updater.ClearAll();
        updater.Type = MediaPlaybackType.Music;
        updater.MusicProperties.Title = track.DisplayName;
        updater.MusicProperties.Artist = track.Artist ?? string.Empty;
        updater.MusicProperties.AlbumTitle = track.Album ?? string.Empty;
        updater.Thumbnail = CreateThumbnail(track.AlbumArtBytes);
        updater.Update();
    }

    private void ApplyTransportState(bool isPlaying, TimeSpan position)
    {
        SetTransportEnabled(true);
        SetButtonStates(playEnabled: !isPlaying, pauseEnabled: isPlaying, stopEnabled: true);
        SetPlaybackStatus(isPlaying
            ? MediaPlaybackStatus.Playing
            : position <= TimeSpan.FromMilliseconds(250)
                ? MediaPlaybackStatus.Stopped
                : MediaPlaybackStatus.Paused);
    }

    private void ApplyTimeline(TimeSpan position, TimeSpan duration)
    {
        if (controls is null)
        {
            return;
        }

        var normalizedDuration = duration > TimeSpan.Zero ? duration : TimeSpan.Zero;
        var normalizedPosition = position < TimeSpan.Zero
            ? TimeSpan.Zero
            : position > normalizedDuration
                ? normalizedDuration
                : position;

        if (normalizedDuration == lastTimelineDuration &&
            Math.Abs((normalizedPosition - lastTimelinePosition).TotalMilliseconds) < 250)
        {
            return;
        }

        lastTimelineDuration = normalizedDuration;
        lastTimelinePosition = normalizedPosition;

        controls.UpdateTimelineProperties(new SystemMediaTransportControlsTimelineProperties
        {
            StartTime = TimeSpan.Zero,
            EndTime = normalizedDuration,
            MinSeekTime = TimeSpan.Zero,
            MaxSeekTime = normalizedDuration,
            Position = normalizedPosition
        });
    }

    private void Clear()
    {
        if (controls is null)
        {
            return;
        }

        if (currentTrack is null &&
            isEnabled == false &&
            playbackStatus == MediaPlaybackStatus.Closed)
        {
            return;
        }

        currentTrack = null;
        DisposeArtworkStream();
        SetButtonStates(playEnabled: false, pauseEnabled: false, stopEnabled: false);
        SetPlaybackStatus(MediaPlaybackStatus.Closed);

        var updater = controls.DisplayUpdater;
        updater.ClearAll();
        updater.Update();

        ClearTimeline();
        SetTransportEnabled(false);
    }

    private void ClearTimeline()
    {
        if (controls is null)
        {
            return;
        }

        lastTimelinePosition = TimeSpan.Zero;
        lastTimelineDuration = TimeSpan.Zero;

        controls.UpdateTimelineProperties(new SystemMediaTransportControlsTimelineProperties
        {
            StartTime = TimeSpan.Zero,
            EndTime = TimeSpan.Zero,
            MinSeekTime = TimeSpan.Zero,
            MaxSeekTime = TimeSpan.Zero,
            Position = TimeSpan.Zero
        });
    }

    private RandomAccessStreamReference? CreateThumbnail(byte[]? albumArtBytes)
    {
        DisposeArtworkStream();

        if (albumArtBytes is not { Length: > 0 })
        {
            return null;
        }

        artworkStream = new InMemoryRandomAccessStream();
        using var writer = new DataWriter(artworkStream);
        writer.WriteBytes(albumArtBytes);
        writer.StoreAsync().AsTask().GetAwaiter().GetResult();
        writer.DetachStream();
        artworkStream.Seek(0);
        return RandomAccessStreamReference.CreateFromStream(artworkStream);
    }

    private void SetTransportEnabled(bool enabled)
    {
        if (controls is null || isEnabled == enabled)
        {
            return;
        }

        controls.IsEnabled = enabled;
        isEnabled = enabled;
    }

    private void SetButtonStates(bool playEnabled, bool pauseEnabled, bool stopEnabled)
    {
        if (controls is null)
        {
            return;
        }

        if (isPlayEnabled != playEnabled)
        {
            controls.IsPlayEnabled = playEnabled;
            isPlayEnabled = playEnabled;
        }

        if (isPauseEnabled != pauseEnabled)
        {
            controls.IsPauseEnabled = pauseEnabled;
            isPauseEnabled = pauseEnabled;
        }

        if (isStopEnabled != stopEnabled)
        {
            controls.IsStopEnabled = stopEnabled;
            isStopEnabled = stopEnabled;
        }
    }

    private void SetPlaybackStatus(MediaPlaybackStatus status)
    {
        if (controls is null || playbackStatus == status)
        {
            return;
        }

        controls.PlaybackStatus = status;
        playbackStatus = status;
    }

    private void ResetTransportControls()
    {
        if (controls is not null)
        {
            try
            {
                controls.ButtonPressed -= Controls_ButtonPressed;
                controls.IsEnabled = false;
                controls.PlaybackStatus = MediaPlaybackStatus.Closed;
                controls.DisplayUpdater.ClearAll();
                controls.DisplayUpdater.Update();
                controls.UpdateTimelineProperties(new SystemMediaTransportControlsTimelineProperties());
            }
            catch
            {
            }
        }

        DisposeArtworkStream();
        controls = null;
        windowHandle = IntPtr.Zero;
        InvalidateCaches();
    }

    private void DisposeArtworkStream()
    {
        artworkStream?.Dispose();
        artworkStream = null;
    }

    private void InvalidateCaches()
    {
        currentTrack = null;
        isEnabled = null;
        isPlayEnabled = null;
        isPauseEnabled = null;
        isStopEnabled = null;
        playbackStatus = null;
        lastTimelinePosition = TimeSpan.MinValue;
        lastTimelineDuration = TimeSpan.MinValue;
    }

    private void Controls_ButtonPressed(
        SystemMediaTransportControls sender,
        SystemMediaTransportControlsButtonPressedEventArgs args)
    {
        switch (args.Button)
        {
            case SystemMediaTransportControlsButton.Play:
                CommandRequested?.Invoke(this, WindowsNowPlayingCommand.Play);
                break;
            case SystemMediaTransportControlsButton.Pause:
                CommandRequested?.Invoke(this, WindowsNowPlayingCommand.Pause);
                break;
            case SystemMediaTransportControlsButton.Stop:
                CommandRequested?.Invoke(this, WindowsNowPlayingCommand.Stop);
                break;
        }
    }
}
