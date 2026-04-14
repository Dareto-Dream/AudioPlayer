using System.Linq;
using TagLib;
using TagLib.Id3v2;

namespace AudioPlayer;

internal static class AudioMetadataReader
{
    public static AudioFileMetadata Read(string path)
    {
        try
        {
            using var file = TagLib.File.Create(path);
            var tag = file.Tag;

            return new AudioFileMetadata(
                Normalize(tag.Title),
                FirstNonEmpty(JoinDistinct(tag.Performers), JoinDistinct(tag.AlbumArtists), JoinDistinct(tag.Composers)),
                Normalize(tag.Album),
                ExtractAlbumArt(tag.Pictures),
                ExtractLyrics(file, path));
        }
        catch
        {
            return AudioFileMetadata.Empty;
        }
    }

    private static string? JoinDistinct(string[] values)
    {
        var normalized = values
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(static value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return normalized.Length == 0 ? null : string.Join(", ", normalized);
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return null;
    }

    private static byte[]? ExtractAlbumArt(IPicture[] pictures)
    {
        var picture = pictures.FirstOrDefault(static candidate => candidate.Type == PictureType.FrontCover)
            ?? pictures.FirstOrDefault();

        return picture?.Data?.Data is { Length: > 0 } data
            ? data.ToArray()
            : null;
    }

    private static LyricsDocument? ExtractLyrics(TagLib.File file, string path)
    {
        var embeddedLyrics = ExtractEmbeddedLyrics(file);
        if (embeddedLyrics is not null)
        {
            return embeddedLyrics;
        }

        var sidecarPath = Path.ChangeExtension(path, ".lrc");
        if (!System.IO.File.Exists(sidecarPath))
        {
            return null;
        }

        try
        {
            return LrcParser.Parse(System.IO.File.ReadAllText(sidecarPath), "Sidecar LRC");
        }
        catch
        {
            return null;
        }
    }

    private static LyricsDocument? ExtractEmbeddedLyrics(TagLib.File file)
    {
        var id3Tag = file.GetTag(TagTypes.Id3v2, false) as TagLib.Id3v2.Tag;
        if (id3Tag is null)
        {
            return null;
        }

        var embeddedLrc = id3Tag
            .GetFrames<UserTextInformationFrame>()
            .FirstOrDefault(static frame =>
                string.Equals(frame.Description, "LRC_SYNC", StringComparison.OrdinalIgnoreCase));

        var parsedEmbeddedLrc = LrcParser.Parse(JoinFrameText(embeddedLrc?.Text), "Embedded LRC");
        if (parsedEmbeddedLrc is not null)
        {
            return parsedEmbeddedLrc;
        }

        var unsynchronisedLyrics = id3Tag
            .GetFrames<UnsynchronisedLyricsFrame>()
            .Select(static frame => frame.Text)
            .FirstOrDefault(static text => !string.IsNullOrWhiteSpace(text));

        return LrcParser.Parse(unsynchronisedLyrics, "Embedded lyrics");
    }

    private static string? JoinFrameText(string[]? values)
    {
        if (values is null || values.Length == 0)
        {
            return null;
        }

        var normalized = values
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(static value => value.Trim())
            .ToArray();

        return normalized.Length == 0 ? null : string.Join(Environment.NewLine, normalized);
    }
}

internal sealed record AudioFileMetadata(
    string? Title,
    string? Artist,
    string? Album,
    byte[]? AlbumArtBytes,
    LyricsDocument? Lyrics)
{
    public static AudioFileMetadata Empty { get; } = new(null, null, null, null, null);
}
