namespace Spectrallis;

public sealed class EmbeddedHtmlContext
{
    internal EmbeddedHtmlContext(string id, byte[] htmlBytes, string? version)
    {
        Id = id;
        HtmlBytes = htmlBytes;
        Version = version;
    }

    internal string Id { get; }
    internal byte[] HtmlBytes { get; }
    internal string? Version { get; }

    public string DisplayName => CreateDisplayLabel(Id);

    private static string CreateDisplayLabel(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "HTML Content";
        }

        return string.Join(
            ' ',
            value
                .Replace('_', ' ')
                .Replace('-', ' ')
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(static part => char.ToUpperInvariant(part[0]) + part[1..]));
    }
}
