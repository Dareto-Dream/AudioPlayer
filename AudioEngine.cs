using System.IO;
using NAudio.Vorbis;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace AudioPlayer;

public sealed class AudioEngine : IDisposable
{
    private WaveStream? playbackStream;
    private WaveOutEvent? output;
    private VisualizerSampleProvider? visualizer;
    private int preferredSampleRate;
    private float volume = 0.85f;

    public AudioTrackInfo? CurrentTrack { get; private set; }

    public bool IsLoaded => playbackStream is not null && output is not null;

    public bool IsPlaying => output?.PlaybackState == PlaybackState.Playing;

    public int EffectiveSampleRate => visualizer?.WaveFormat.SampleRate ?? playbackStream?.WaveFormat.SampleRate ?? 0;

    public float Volume
    {
        get => volume;
        set
        {
            volume = Math.Clamp(value, 0, 1);

            if (output is not null)
            {
                output.Volume = volume;
            }
        }
    }

    public void Load(string path)
    {
        DisposePlayback();

        playbackStream = OpenPlaybackStream(path, out var formatName);
        CurrentTrack = BuildTrackInfo(path, playbackStream, formatName);
        CreateOutputChain(TimeSpan.Zero, resumePlayback: false);
    }

    public void Toggle()
    {
        if (!IsLoaded || output is null || playbackStream is null)
        {
            return;
        }

        if (output.PlaybackState == PlaybackState.Playing)
        {
            output.Pause();
            return;
        }

        if (output.PlaybackState == PlaybackState.Stopped &&
            playbackStream.TotalTime > TimeSpan.Zero &&
            playbackStream.CurrentTime >= playbackStream.TotalTime)
        {
            playbackStream.CurrentTime = TimeSpan.Zero;
        }

        output.Play();
    }

    public void Stop()
    {
        if (!IsLoaded || output is null || playbackStream is null)
        {
            return;
        }

        output.Stop();
        playbackStream.CurrentTime = TimeSpan.Zero;
        visualizer?.Clear();
    }

    public void SetPreferredSampleRate(int sampleRate)
    {
        var normalizedSampleRate = Math.Max(0, sampleRate);
        if (preferredSampleRate == normalizedSampleRate)
        {
            return;
        }

        preferredSampleRate = normalizedSampleRate;

        if (!IsLoaded || playbackStream is null || output is null)
        {
            return;
        }

        var currentPosition = playbackStream.CurrentTime;
        var resumePlayback = output.PlaybackState == PlaybackState.Playing;
        CreateOutputChain(currentPosition, resumePlayback);
    }

    public void Seek(float seconds)
    {
        if (playbackStream is null)
        {
            return;
        }

        var clampedSeconds = Math.Clamp(seconds, 0, GetLength());
        playbackStream.CurrentTime = TimeSpan.FromSeconds(clampedSeconds);
    }

    public float GetPosition() => (float)(playbackStream?.CurrentTime.TotalSeconds ?? 0);

    public float GetLength() => (float)(playbackStream?.TotalTime.TotalSeconds ?? 0);

    public VisualizerFrame GetVisualizerFrame() => visualizer?.GetFrame() ?? VisualizerFrame.Empty;

    public void Dispose()
    {
        DisposePlayback();
        GC.SuppressFinalize(this);
    }

    private void CreateOutputChain(TimeSpan currentPosition, bool resumePlayback)
    {
        if (playbackStream is null)
        {
            return;
        }

        output?.Dispose();
        output = null;

        playbackStream.CurrentTime = currentPosition;

        ISampleProvider sampleProvider = playbackStream.ToSampleProvider();
        if (preferredSampleRate > 0 && sampleProvider.WaveFormat.SampleRate != preferredSampleRate)
        {
            sampleProvider = new WdlResamplingSampleProvider(sampleProvider, preferredSampleRate);
        }

        visualizer = new VisualizerSampleProvider(sampleProvider);
        output = new WaveOutEvent
        {
            DesiredLatency = 70,
            NumberOfBuffers = 3,
            Volume = volume
        };
        output.Init(visualizer);

        if (resumePlayback)
        {
            output.Play();
        }
    }

    private static AudioTrackInfo BuildTrackInfo(string path, WaveStream stream, string formatName)
    {
        var displayName = Path.GetFileNameWithoutExtension(path);

        return new AudioTrackInfo(
            path,
            string.IsNullOrWhiteSpace(displayName) ? Path.GetFileName(path) : displayName,
            formatName,
            Math.Max(1, stream.WaveFormat.Channels),
            stream.WaveFormat.SampleRate,
            stream.WaveFormat.BitsPerSample,
            stream.TotalTime);
    }

    private static WaveStream OpenPlaybackStream(string path, out string formatName)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();

        try
        {
            switch (extension)
            {
                case ".wav":
                    formatName = "WAV";
                    return new WaveFileReader(path);
                case ".mp3":
                    formatName = "MP3";
                    return new Mp3FileReader(path);
                case ".aif":
                case ".aifc":
                case ".aiff":
                    formatName = "AIFF";
                    return new AiffFileReader(path);
                case ".ogg":
                case ".oga":
                    formatName = "Ogg Vorbis";
                    return new VorbisWaveReader(path);
            }
        }
        catch (Exception directReaderException)
        {
            throw new NotSupportedException("The selected audio file could not be decoded by its direct reader.", directReaderException);
        }

        try
        {
            formatName = GetContainerLabel(extension, "Windows codec");
            return new MediaFoundationReader(path);
        }
        catch (Exception mediaFoundationException)
        {
            try
            {
                formatName = GetContainerLabel(extension, "NAudio fallback");
                return new AudioFileReader(path);
            }
            catch (Exception fallbackException)
            {
                throw new NotSupportedException(
                    "The selected audio file could not be opened. This app supports many common formats directly and additional formats through installed Windows codecs.",
                    new AggregateException(mediaFoundationException, fallbackException));
            }
        }
    }

    private static string GetContainerLabel(string extension, string fallbackLabel) =>
        extension switch
        {
            ".aac" => "AAC",
            ".adts" => "AAC / ADTS",
            ".asf" => "ASF",
            ".flac" => "FLAC",
            ".m4a" => "M4A",
            ".m4b" => "M4B",
            ".m4p" => "M4P",
            ".mp4" => "MP4 audio",
            ".opus" => "Opus",
            ".webm" => "WebM audio",
            ".wma" => "WMA",
            ".3gp" => "3GP audio",
            _ => fallbackLabel
        };

    private void DisposePlayback()
    {
        output?.Stop();
        output?.Dispose();
        playbackStream?.Dispose();
        output = null;
        playbackStream = null;
        visualizer = null;
        CurrentTrack = null;
    }
}
