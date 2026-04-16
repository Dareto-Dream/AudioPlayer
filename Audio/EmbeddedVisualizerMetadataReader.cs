using System.Text.Json.Nodes;
using TagLib;
using TagLib.Id3v2;

namespace AudioPlayer;

internal static class EmbeddedVisualizerMetadataReader
{
    private const string ModulePrefix = "DELTA_MODULE_";
    private const string BinaryPrefix = "DELTA_BIN_";
    private const string DataPrefix = "DELTA_DATA_";

    public static EmbeddedVisualizerContext? Read(TagLib.Id3v2.Tag? id3Tag)
    {
        if (id3Tag is null)
        {
            return null;
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

        foreach (var modulePayload in modulePayloads)
        {
            var module = TryParseModule(modulePayload, binaries);
            if (module is null || !binaries.TryGetValue(module.BinaryRef, out var binary))
            {
                continue;
            }

            return new EmbeddedVisualizerContext(module, binary, dataBlocks);
        }

        return null;
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
        var entry = ReadRequiredString(jsonObject, "entry");
        var binaryRef = ReadRequiredString(jsonObject, "binaryRef");
        if (string.IsNullOrWhiteSpace(id) ||
            string.IsNullOrWhiteSpace(type) ||
            string.IsNullOrWhiteSpace(runtime) ||
            string.IsNullOrWhiteSpace(entry) ||
            string.IsNullOrWhiteSpace(binaryRef))
        {
            return null;
        }

        if (!string.Equals(type, "visualizer", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(runtime, "wasm", StringComparison.OrdinalIgnoreCase) ||
            !binaries.ContainsKey(binaryRef))
        {
            return null;
        }

        return new EmbeddedVisualizerModule(
            id,
            type,
            runtime,
            entry,
            ReadDataRefs(jsonObject["dataRefs"]),
            binaryRef,
            ReadOptionalString(jsonObject, "version"));
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
}
