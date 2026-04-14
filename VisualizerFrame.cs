namespace AudioPlayer;

public sealed record VisualizerFrame(float[] Spectrum, float[] Waveform, float PeakLevel, float RmsLevel)
{
    public static VisualizerFrame Empty { get; } = new(Array.Empty<float>(), Array.Empty<float>(), 0, 0);
}
