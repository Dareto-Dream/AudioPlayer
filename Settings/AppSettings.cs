using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Spectrallis;

internal enum ThemeMode
{
    Dark,
    Light,
    Oled,
    Midnight
}

internal enum ThemeAccent
{
    Amber,
    Ocean,
    Rose,
    Forest,
    Violet,
    Crimson,
    Cyan,
    Mint,
    Sunset,
    Gold
}

internal sealed class AppSettings
{
    public ThemeMode ThemeMode { get; set; } = ThemeMode.Dark;
    public ThemeAccent ThemeAccent { get; set; } = ThemeAccent.Amber;
    public bool UseEmbeddedTrackThemes { get; set; } = true;
    public VisualizerMode DefaultVisualizer { get; set; } = VisualizerMode.MirrorSpectrum;
    public bool UseEmbeddedTrackVisualizers { get; set; } = true;
    public int PreferredSampleRate { get; set; }
    public int DefaultVolume { get; set; } = 85;
    public bool PeakHold { get; set; } = true;
    public int VisualizerSensitivity { get; set; } = 100;
    public bool EnableVisualizerAutoCycle { get; set; } = true;
    public int VisualizerCycleSeconds { get; set; } = 12;
    public bool AutoPlayOnOpen { get; set; } = true;
    public bool ShowMoreInfo { get; set; }

    public AppSettings Clone() =>
        new()
        {
            ThemeMode = ThemeMode,
            ThemeAccent = ThemeAccent,
            UseEmbeddedTrackThemes = UseEmbeddedTrackThemes,
            DefaultVisualizer = DefaultVisualizer,
            UseEmbeddedTrackVisualizers = UseEmbeddedTrackVisualizers,
            PreferredSampleRate = PreferredSampleRate,
            DefaultVolume = DefaultVolume,
            PeakHold = PeakHold,
            VisualizerSensitivity = VisualizerSensitivity,
            EnableVisualizerAutoCycle = EnableVisualizerAutoCycle,
            VisualizerCycleSeconds = VisualizerCycleSeconds,
            AutoPlayOnOpen = AutoPlayOnOpen,
            ShowMoreInfo = ShowMoreInfo
        };
}

internal sealed record SelectionOption<T>(string Label, T Value)
{
    public override string ToString() => Label;
}

internal static class AppSettingsStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    private static readonly int[] AllowedSampleRates = [0, 44100, 48000, 88200, 96000];

    private static string SettingsPath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Spectrallis",
            "settings.json");

    public static AppSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
                return Normalize(new AppSettings());

            var json = File.ReadAllText(SettingsPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, SerializerOptions);
            return Normalize(settings ?? new AppSettings());
        }
        catch
        {
            return Normalize(new AppSettings());
        }
    }

    public static void Save(AppSettings settings)
    {
        var normalized = Normalize(settings.Clone());
        var directory = Path.GetDirectoryName(SettingsPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(normalized, SerializerOptions));
    }

    public static AppSettings Normalize(AppSettings settings)
    {
        settings.ThemeMode = Enum.IsDefined(settings.ThemeMode) ? settings.ThemeMode : ThemeMode.Dark;
        settings.ThemeAccent = Enum.IsDefined(settings.ThemeAccent) ? settings.ThemeAccent : ThemeAccent.Amber;
        settings.DefaultVisualizer = Enum.IsDefined(settings.DefaultVisualizer)
            ? settings.DefaultVisualizer
            : VisualizerMode.MirrorSpectrum;
        settings.PreferredSampleRate = AllowedSampleRates.Contains(settings.PreferredSampleRate)
            ? settings.PreferredSampleRate
            : 0;
        settings.DefaultVolume = Math.Clamp(settings.DefaultVolume, 0, 100);
        settings.VisualizerSensitivity = Math.Clamp(settings.VisualizerSensitivity, 50, 200);
        settings.VisualizerCycleSeconds = Math.Clamp(settings.VisualizerCycleSeconds, 5, 60);
        return settings;
    }
}
