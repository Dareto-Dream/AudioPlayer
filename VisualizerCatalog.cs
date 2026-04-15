namespace AudioPlayer;

internal sealed record VisualizerDefinition(
    VisualizerMode Mode,
    string Label,
    IVisualizerRenderer Renderer,
    bool RequiresAlbumArt = false);

// Register each visualizer once here so the form, settings dialog, and control all stay in sync.
internal static class VisualizerCatalog
{
    private static readonly VisualizerDefinition[] Definitions =
    [
        new(VisualizerMode.Spectrum, "Spectrum", new SpectrumBarsVisualizerRenderer(mirrored: false)),
        new(VisualizerMode.MirrorSpectrum, "Mirror Spectrum", new SpectrumBarsVisualizerRenderer(mirrored: true)),
        new(VisualizerMode.Waveform, "Waveform", new WaveformVisualizerRenderer()),
        new(VisualizerMode.SpinningDisk, "Spinning Disk", new SpinningDiskVisualizerRenderer(), RequiresAlbumArt: true)
    ];

    private static readonly Dictionary<VisualizerMode, VisualizerDefinition> DefinitionsByMode =
        Definitions.ToDictionary(static definition => definition.Mode);

    public static IReadOnlyList<VisualizerDefinition> All => Definitions;

    public static VisualizerDefinition GetDefinition(VisualizerMode mode) =>
        DefinitionsByMode.TryGetValue(mode, out var definition)
            ? definition
            : DefinitionsByMode[VisualizerMode.MirrorSpectrum];

    public static SelectionOption<VisualizerMode>[] GetOptions(bool includeAlbumArtDependent) =>
        Definitions
            .Where(definition => includeAlbumArtDependent || !definition.RequiresAlbumArt)
            .Select(static definition => new SelectionOption<VisualizerMode>(definition.Label, definition.Mode))
            .ToArray();

    public static bool IsAvailable(VisualizerMode mode, bool hasAlbumArt)
    {
        var definition = GetDefinition(mode);
        return !definition.RequiresAlbumArt || hasAlbumArt;
    }

    public static VisualizerMode GetPreferredMode(VisualizerMode preferredMode, bool hasAlbumArt)
    {
        if (IsAvailable(preferredMode, hasAlbumArt))
            return preferredMode;

        return IsAvailable(VisualizerMode.MirrorSpectrum, hasAlbumArt)
            ? VisualizerMode.MirrorSpectrum
            : GetOptions(includeAlbumArtDependent: hasAlbumArt).First().Value;
    }
}
