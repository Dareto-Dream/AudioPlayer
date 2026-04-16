namespace AudioPlayer;

public partial class Form1
{
    private void InitializeNowPlaying()
    {
        nowPlaying.CommandRequested -= NowPlaying_CommandRequested;
        nowPlaying.CommandRequested += NowPlaying_CommandRequested;
        nowPlaying.Initialize(Handle);
        SyncNowPlayingState();
    }

    private void SyncNowPlayingState()
    {
        nowPlaying.Update(
            engine.CurrentTrack,
            engine.IsPlaying,
            TimeSpan.FromSeconds(engine.GetPosition()),
            TimeSpan.FromSeconds(engine.GetLength()));
    }

    private void NowPlaying_CommandRequested(object? sender, WindowsNowPlayingCommand command)
    {
        if (!IsHandleCreated || IsDisposed || Disposing)
        {
            return;
        }

        BeginInvoke(new Action(() =>
        {
            switch (command)
            {
                case WindowsNowPlayingCommand.Play:
                    if (engine.IsLoaded && !engine.IsPlaying)
                    {
                        engine.Toggle();
                        UpdateUiState();
                    }
                    break;

                case WindowsNowPlayingCommand.Pause:
                    if (engine.IsLoaded && engine.IsPlaying)
                    {
                        engine.Toggle();
                        UpdateUiState();
                    }
                    break;

                case WindowsNowPlayingCommand.Stop:
                    if (engine.IsLoaded)
                    {
                        engine.Stop();
                        UpdateUiState();
                    }
                    break;
            }
        }));
    }
}
