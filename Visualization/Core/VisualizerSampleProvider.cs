using NAudio.Dsp;
using NAudio.Wave;

namespace Spectrallis;

public sealed class VisualizerSampleProvider : ISampleProvider
{
    private const int FftLength = 4096;
    private const int SpectrumBarCount = 64;
    private const int WaveformPointCount = 256;
    private const int MinimumDecibels = -72;

    private readonly ISampleProvider source;
    private readonly int channels;
    private readonly object syncRoot = new();
    private readonly Complex[] fftBuffer = new Complex[FftLength];
    private readonly float[] spectrumBars = new float[SpectrumBarCount];
    private readonly float[] waveformRing = new float[WaveformPointCount];

    private int fftPosition;
    private int waveformPosition;
    private float peakLevel;
    private float rmsLevel;
    private double rmsAccumulator;
    private int rmsAccumulatorCount;

    public VisualizerSampleProvider(ISampleProvider source)
    {
        this.source = source;
        channels = Math.Max(1, source.WaveFormat.Channels);
    }

    public WaveFormat WaveFormat => source.WaveFormat;

    public int Read(float[] buffer, int offset, int count)
    {
        var samplesRead = source.Read(buffer, offset, count);

        for (var index = 0; index < samplesRead; index += channels)
        {
            float monoSample = 0;
            var channelSamples = Math.Min(channels, samplesRead - index);

            for (var channel = 0; channel < channelSamples; channel++)
            {
                monoSample += buffer[offset + index + channel];
            }

            monoSample /= channelSamples;
            CaptureSample(monoSample);
        }

        return samplesRead;
    }

    public VisualizerFrame GetFrame()
    {
        lock (syncRoot)
        {
            var waveform = new float[waveformRing.Length];
            for (var i = 0; i < waveform.Length; i++)
            {
                waveform[i] = waveformRing[(waveformPosition + i) % waveformRing.Length];
            }

            return new VisualizerFrame((float[])spectrumBars.Clone(), waveform, peakLevel, rmsLevel);
        }
    }

    public void Clear()
    {
        lock (syncRoot)
        {
            Array.Clear(fftBuffer);
            Array.Clear(spectrumBars);
            Array.Clear(waveformRing);
            fftPosition = 0;
            waveformPosition = 0;
            peakLevel = 0;
            rmsLevel = 0;
            rmsAccumulator = 0;
            rmsAccumulatorCount = 0;
        }
    }

    private void CaptureSample(float sample)
    {
        lock (syncRoot)
        {
            waveformRing[waveformPosition] = sample;
            waveformPosition = (waveformPosition + 1) % waveformRing.Length;

            peakLevel = Math.Max(Math.Abs(sample), peakLevel * 0.94f);
            rmsAccumulator += sample * sample;
            rmsAccumulatorCount++;

            if (rmsAccumulatorCount >= 256)
            {
                rmsLevel = (float)Math.Sqrt(rmsAccumulator / rmsAccumulatorCount);
                rmsAccumulator = 0;
                rmsAccumulatorCount = 0;
            }

            fftBuffer[fftPosition].X = (float)(sample * FastFourierTransform.HammingWindow(fftPosition, FftLength));
            fftBuffer[fftPosition].Y = 0;
            fftPosition++;

            if (fftPosition < FftLength)
            {
                return;
            }

            FastFourierTransform.FFT(true, (int)Math.Log2(FftLength), fftBuffer);
            UpdateSpectrumBars();
            fftPosition = 0;
        }
    }

    private void UpdateSpectrumBars()
    {
        var nyquist = WaveFormat.SampleRate / 2.0;
        var minimumFrequency = 25.0;
        var maximumFrequency = Math.Min(18000.0, nyquist);
        var binCount = FftLength / 2;

        for (var barIndex = 0; barIndex < spectrumBars.Length; barIndex++)
        {
            var startFrequency = GetLogFrequency(minimumFrequency, maximumFrequency, barIndex / (double)spectrumBars.Length);
            var endFrequency = GetLogFrequency(minimumFrequency, maximumFrequency, (barIndex + 1) / (double)spectrumBars.Length);

            var startBin = Math.Clamp((int)(startFrequency / nyquist * binCount), 1, binCount - 1);
            var endBin = Math.Clamp((int)(endFrequency / nyquist * binCount), startBin + 1, binCount);

            double energy = 0;
            for (var bin = startBin; bin < endBin; bin++)
            {
                var real = fftBuffer[bin].X;
                var imaginary = fftBuffer[bin].Y;
                energy += Math.Sqrt((real * real) + (imaginary * imaginary));
            }

            var averageMagnitude = energy / (endBin - startBin);
            var decibels = 20 * Math.Log10(averageMagnitude + 1e-9);
            var normalized = (float)Math.Clamp((decibels - MinimumDecibels) / -MinimumDecibels, 0, 1);

            spectrumBars[barIndex] = Math.Max(normalized, spectrumBars[barIndex] * 0.84f);
        }
    }

    private static double GetLogFrequency(double minimumFrequency, double maximumFrequency, double ratio)
    {
        var safeRatio = Math.Clamp(ratio, 0, 1);
        return minimumFrequency * Math.Pow(maximumFrequency / minimumFrequency, safeRatio);
    }
}
