namespace Spectrallis;

public sealed record AudioTrackInfo(
    string FilePath,
    string DisplayName,
    string? Artist,
    string? Album,
    byte[]? AlbumArtBytes,
    LyricsDocument? Lyrics,
    EmbeddedVisualizerContext? EmbeddedVisualizer,
    EmbeddedThemeContext? EmbeddedTheme,
    string FormatName,
    int Channels,
    int SourceSampleRate,
    int BitsPerSample,
    TimeSpan Duration);
