namespace AudioPlayer;

public sealed record AudioTrackInfo(
    string FilePath,
    string DisplayName,
    string FormatName,
    int Channels,
    int SourceSampleRate,
    int BitsPerSample,
    TimeSpan Duration);
