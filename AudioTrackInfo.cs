namespace AudioPlayer;

public sealed record AudioTrackInfo(
    string FilePath,
    string DisplayName,
    string? Artist,
    string? Album,
    byte[]? AlbumArtBytes,
    string FormatName,
    int Channels,
    int SourceSampleRate,
    int BitsPerSample,
    TimeSpan Duration);
