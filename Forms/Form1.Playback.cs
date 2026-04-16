using System.IO;

namespace Spectrallis;

public partial class Form1
{
    private void SeekRelative(float deltaSec)
    {
        if (!engine.IsLoaded)
            return;

        engine.Seek(Math.Clamp(engine.GetPosition() + deltaSec, 0, engine.GetLength()));
        UpdateUiState();
    }

    private void AdjustVolume(int delta)
    {
        trackBarVolume.Value = Math.Clamp(trackBarVolume.Value + delta, 0, 100);
        engine.Volume = trackBarVolume.Value / 100f;
        UpdateUiState();
    }

    private void btnPlayPause_Click(object sender, EventArgs e)
    {
        if (!engine.IsLoaded)
        {
            OpenAudioFile(startPlayback: true);
            return;
        }

        engine.Toggle();
        UpdateUiState();
    }

    private void btnStop_Click(object sender, EventArgs e)
    {
        engine.Stop();
        UpdateUiState();
    }

    private void btnMute_Click(object sender, EventArgs e)
    {
        if (isMuted)
        {
            isMuted = false;
            trackBarVolume.Value = Math.Max(1, (int)(preMuteVolume * 100));
        }
        else
        {
            preMuteVolume = Math.Max(0.01f, trackBarVolume.Value / 100f);
            isMuted = true;
            trackBarVolume.Value = 0;
        }

        engine.Volume = trackBarVolume.Value / 100f;
        UpdateUiState();
    }

    private void btnDefaultApp_Click(object sender, EventArgs e)
    {
        try
        {
            DefaultAppRegistrar.RegisterCurrentUser();
            DefaultAppRegistrar.OpenDefaultAppsSettings();

            MessageBox.Show(
                this,
                "Audio Player has been registered for the current Windows user. Choose Audio Player in the Default Apps window that just opened to make it the handler for supported audio files.",
                "Default App Registration",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            ShowError(
                $"Audio Player could not register itself for Windows file associations.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                "Registration Error");
        }
    }

    private void OpenAudioFile(bool startPlayback)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = SupportedAudioFormats.OpenFileDialogFilter,
            Title = "Select an audio file"
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
            LoadAudioFile(dialog.FileName, startPlayback);
    }

    private void LoadAudioFile(string path, bool startPlayback)
    {
        if (!File.Exists(path))
        {
            ShowError(
                $"The selected file could not be found:{Environment.NewLine}{Environment.NewLine}{path}",
                "Open Error");
            return;
        }

        var loaded = false;

        try
        {
            engine.Load(path);
            visualizerControl.ClearFrame();
            trackBarSeek.Value = 0;
            loaded = true;
        }
        catch (Exception ex)
        {
            ShowError(
                $"Unable to load the selected audio file.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                "Load Error");
        }

        if (loaded && startPlayback)
            engine.Toggle();

        UpdateUiState();
    }
}
