using System.Text.Json.Nodes;
using TagLib;
using TagLib.Id3v2;

namespace Spectrallis;

internal static class EmbeddedVisualizerMetadataReader
{
    private const string ModulePrefix = "DELTA_MODULE_";
    private const string BinaryPrefix = "DELTA_BIN_";
    private const string DataPrefix = "DELTA_DATA_";

    /// <summary>
    /// Reads all embedded modules from ID3v2 tags and returns them sorted by type.
    /// </summary>
    public static (EmbeddedVisualizerContext? Visualizer, EmbeddedHtmlContext? Html, EmbeddedMarkdownContext? Markdown, EmbeddedVideoContext? Video) TryReadAll(TagLib.Id3v2.Tag? id3Tag)
    {
        if (id3Tag is null)
        {
            return (null, null, null, null);
        }

        var modulePayloads = new List<string>();
        var binaries = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        var dataBlocks = new Dictionary<string, EmbeddedDataBlock>(StringComparer.OrdinalIgnoreCase);

        foreach (var frame in id3Tag.GetFrames<UserTextInformationFrame>())
        {
            if (frame.TextEncoding != StringType.UTF8)
            {
                continue;
            }

            var description = Normalize(frame.Description);
            var payload = JoinFrameText(frame.Text);
            if (string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(payload))
            {
                continue;
            }

            if (description.StartsWith(ModulePrefix, StringComparison.OrdinalIgnoreCase))
            {
                modulePayloads.Add(payload);
                continue;
            }

            if (description.StartsWith(BinaryPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var binaryId = description[BinaryPrefix.Length..].Trim();
                if (!string.IsNullOrWhiteSpace(binaryId) && TryDecodeBinary(payload, out var bytes))
                {
                    binaries[binaryId] = bytes;
                }

                continue;
            }

            if (description.StartsWith(DataPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var dataId = description[DataPrefix.Length..].Trim();
                if (!string.IsNullOrWhiteSpace(dataId))
                {
                    dataBlocks[dataId] = new EmbeddedDataBlock(dataId, payload, TryParseJson(payload));
                }
            }
        }

        EmbeddedVisualizerContext? visualizer = null;
        EmbeddedHtmlContext? html = null;
        EmbeddedMarkdownContext? markdown = null;
        EmbeddedVideoContext? video = null;

        foreach (var modulePayload in modulePayloads)
        {
            var module = TryParseModule(modulePayload, binaries);
            if (module is null || !binaries.TryGetValue(module.BinaryRef, out var binary))
            {
                continue;
            }

            var moduleType = module.Type?.ToLowerInvariant() ?? "";

            switch (moduleType)
            {
                case "visualizer":
                    if (module.Runtime?.ToLowerInvariant() == "wasm")
                    {
                        visualizer = new EmbeddedVisualizerContext(module, binary, dataBlocks);
                    }
                    break;

                case "html":
                    html = new EmbeddedHtmlContext(module.Id, binary, module.Version);
                    break;

                case "markdown":
                    var cssOverride = GetOptionalDataBlock(module, "style", dataBlocks);
                    markdown = new EmbeddedMarkdownContext(module.Id, binary, cssOverride, module.Version);
                    break;

                case "video":
                    video = new EmbeddedVideoContext(
                        module.Id,
                        module.Runtime ?? "h264",
                        binary,
                        module.Width,
                        module.Height,
                        module.Autoplay,
                        module.Loop,
                        module.Version);
                    break;
            }
        }

        return (visualizer, html, markdown, video);
    }

    public static EmbeddedVisualizerContext? Read(TagLib.Id3v2.Tag? id3Tag)
    {
        var (visualizer, _, _, _) = TryReadAll(id3Tag);
        return visualizer;
    }

    private static EmbeddedVisualizerModule? TryParseModule(
        string payload,
        IReadOnlyDictionary<string, byte[]> binaries)
    {
        if (TryParseJson(payload) is not JsonObject jsonObject)
        {
            return null;
        }

        var id = ReadRequiredString(jsonObject, "id");
        var type = ReadRequiredString(jsonObject, "type");
        var runtime = ReadRequiredString(jsonObject, "runtime");
        var entry = ReadOptionalString(jsonObject, "entry");
        var binaryRef = ReadRequiredString(jsonObject, "binaryRef");

        // For WASM visualizers, entry is required. For other types, it's optional.
        var isVisualizer = string.Equals(type, "visualizer", StringComparison.OrdinalIgnoreCase);
        if (isVisualizer && string.IsNullOrWhiteSpace(entry))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(id) ||
            string.IsNullOrWhiteSpace(type) ||
            string.IsNullOrWhiteSpace(runtime) ||
            string.IsNullOrWhiteSpace(binaryRef) ||
            !binaries.ContainsKey(binaryRef))
        {
            return null;
        }

        // Parse optional video/HTML dimension and playback attributes
        var width = TryReadInteger(jsonObject, "width");
        var height = TryReadInteger(jsonObject, "height");
        var autoplay = TryReadBoolean(jsonObject, "autoplay", false);
        var loop = TryReadBoolean(jsonObject, "loop", true);

        return new EmbeddedVisualizerModule(
            id,
            type,
            runtime,
            entry ?? "",
            ReadDataRefs(jsonObject["dataRefs"]),
            binaryRef,
            ReadOptionalString(jsonObject, "version"),
            width,
            height,
            autoplay,
            loop);
    }

    private static IReadOnlyDictionary<string, string> ReadDataRefs(JsonNode? node)
    {
        if (node is not JsonObject jsonObject)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        var dataRefs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in jsonObject)
        {
            if (property.Value is JsonValue jsonValue &&
                jsonValue.TryGetValue<string>(out var value) &&
                !string.IsNullOrWhiteSpace(property.Key) &&
                !string.IsNullOrWhiteSpace(value))
            {
                dataRefs[property.Key] = value.Trim();
            }
        }

        return dataRefs;
    }

    private static string? ReadOptionalString(JsonObject jsonObject, string propertyName)
    {
        if (jsonObject[propertyName] is JsonValue jsonValue &&
            jsonValue.TryGetValue<string>(out var stringValue) &&
            !string.IsNullOrWhiteSpace(stringValue))
        {
            return stringValue.Trim();
        }

        return null;
    }

    private static string? ReadRequiredString(JsonObject jsonObject, string propertyName) =>
        ReadOptionalString(jsonObject, propertyName);

    private static JsonNode? TryParseJson(string value)
    {
        try
        {
            return JsonNode.Parse(value);
        }
        catch
        {
            return null;
        }
    }

    private static bool TryDecodeBinary(string payload, out byte[] bytes)
    {
        try
        {
            bytes = Convert.FromBase64String(payload.Trim());
            return bytes.Length > 0;
        }
        catch
        {
            bytes = Array.Empty<byte>();
            return false;
        }
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

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static int? TryReadInteger(JsonObject jsonObject, string propertyName)
    {
        if (jsonObject[propertyName] is JsonValue jsonValue &&
            jsonValue.TryGetValue<int>(out var intValue))
        {
            return intValue;
        }

        return null;
    }

    private static bool TryReadBoolean(JsonObject jsonObject, string propertyName, bool defaultValue)
    {
        if (jsonObject[propertyName] is JsonValue jsonValue &&
            jsonValue.TryGetValue<bool>(out var boolValue))
        {
            return boolValue;
        }

        return defaultValue;
    }

    private static string? GetOptionalDataBlock(
        EmbeddedVisualizerModule module,
        string bindingName,
        IReadOnlyDictionary<string, EmbeddedDataBlock> dataBlocks)
    {
        if (module.DataRefs.TryGetValue(bindingName, out var dataBlockId) &&
            dataBlocks.TryGetValue(dataBlockId, out var dataBlock))
        {
            return dataBlock.RawText;
        }

        return null;
    }
}
