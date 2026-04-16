using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace Spectrallis;

public sealed class SpectrumVisualizerControl : Control
{
    private static readonly EmbeddedVisualizerRenderer embeddedVisualizerRenderer = new();
    private readonly float[] spectrumLevels = new float[64];
    private readonly float[] peakHoldLevels = new float[64];
    private readonly float[] waveformPoints = new float[256];

    private VisualizerMode mode = VisualizerMode.MirrorSpectrum;
    private VisualizerTheme visualizerTheme = VisualizerTheme.Default;
    private bool showPeaks = true;
    private float sensitivity = 1;
    private float peakLevel;
    private float rmsLevel;
    private bool isActive;
    private Image? albumArt;
    private float diskAngle;
    private float animationPhase;
    private float playbackTimeSeconds;
    private EmbeddedVisualizerContext? embeddedVisualizerSource;
    private EmbeddedVisualizerSession? embeddedVisualizer;

    public SpectrumVisualizerControl()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.UserPaint |
            ControlStyles.ResizeRedraw,
            true);

        BackColor = visualizerTheme.BackgroundTopColor;
        ForeColor = visualizerTheme.BarStartColor;
    }

    internal void ApplyTheme(ThemePalette palette)
    {
        visualizerTheme = new VisualizerTheme(
            ThemePalette.Blend(palette.SurfaceBackColor, palette.WindowBackColor, palette.IsDark ? 0.30f : 0.08f),
            ThemePalette.Blend(palette.WindowBackColor, Color.Black, palette.IsDark ? 0.52f : 0.06f),
            palette.AccentPrimaryColor,
            ThemePalette.Blend(palette.BorderStrongColor, palette.AccentPrimaryColor, 0.22f),
            ThemePalette.Blend(palette.AccentPrimaryColor, palette.AccentSecondaryColor, 0.55f),
            ThemePalette.Blend(palette.AccentPrimaryColor, Color.White, palette.IsDark ? 0.10f : 0.04f),
            palette.AccentSecondaryColor,
            ThemePalette.Blend(palette.TextPrimaryColor, palette.AccentPrimaryColor, palette.IsDark ? 0.16f : 0.08f),
            palette.TextPrimaryColor,
            palette.TextSecondaryColor,
            palette.TextSoftColor,
            ThemePalette.Blend(palette.SurfaceRaisedColor, palette.WindowBackColor, palette.IsDark ? 0.36f : 0.08f),
            ThemePalette.Blend(palette.TextSoftColor, palette.AccentPrimaryColor, 0.20f),
            ThemePalette.Blend(palette.WindowBackColor, Color.Black, palette.IsDark ? 0.18f : 0.02f),
            palette.AccentSecondaryColor,
            ThemePalette.Blend(palette.BorderStrongColor, palette.AccentPrimaryColor, 0.30f),
            palette.TextMutedColor);

        BackColor = visualizerTheme.BackgroundTopColor;
        ForeColor = visualizerTheme.BarStartColor;
        Invalidate();
    }

    [DefaultValue(VisualizerMode.MirrorSpectrum)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public VisualizerMode Mode
    {
        get => mode;
        set
        {
            mode = value;
            Invalidate();
        }
    }

    [DefaultValue(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public bool ShowPeaks
    {
        get => showPeaks;
        set
        {
            showPeaks = value;
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Image? AlbumArt
    {
        get => albumArt;
        set
        {
            albumArt = value;
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public EmbeddedVisualizerContext? EmbeddedVisualizer
    {
        set
        {
            if (ReferenceEquals(embeddedVisualizerSource, value))
                return;

            embeddedVisualizerSource = value;
            embeddedVisualizer?.Dispose();
            embeddedVisualizer = EmbeddedVisualizerSession.TryCreate(value);
            Invalidate();
        }
    }

    [Browsable(false)]
    public bool UsesEmbeddedVisualizer => embeddedVisualizer is { IsFaulted: false };

    [DefaultValue(1f)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public float Sensitivity
    {
        get => sensitivity;
        set
        {
            sensitivity = Math.Clamp(value, 0.4f, 2.5f);
            Invalidate();
        }
    }

    public void UpdateFrame(VisualizerFrame frame, bool activePlayback, float playbackSeconds)
    {
        isActive = activePlayback;
        playbackTimeSeconds = Math.Max(0, playbackSeconds);

        for (var index = 0; index < spectrumLevels.Length; index++)
        {
            var incoming = index < frame.Spectrum.Length ? Math.Clamp(frame.Spectrum[index] * sensitivity, 0, 1.25f) : 0;
            spectrumLevels[index] = Math.Max(incoming, spectrumLevels[index] * (activePlayback ? 0.80f : 0.90f));
            peakHoldLevels[index] = showPeaks
                ? Math.Max(spectrumLevels[index], peakHoldLevels[index] - 0.03f)
                : 0;
        }

        for (var index = 0; index < waveformPoints.Length; index++)
        {
            var incoming = index < frame.Waveform.Length ? frame.Waveform[index] * sensitivity : 0;
            waveformPoints[index] = (waveformPoints[index] * 0.35f) + (incoming * 0.65f);
        }

        peakLevel = Math.Max(frame.PeakLevel * sensitivity, peakLevel * (activePlayback ? 0.90f : 0.95f));
        rmsLevel = Math.Max(frame.RmsLevel * sensitivity, rmsLevel * (activePlayback ? 0.92f : 0.96f));

        if (activePlayback)
        {
            if (mode == VisualizerMode.SpinningDisk)
                diskAngle = (diskAngle + 0.38f) % 360f;

            var phaseStep = mode switch
            {
                VisualizerMode.RadialSpectrum => 0.85f,
                VisualizerMode.Graph3D => 1.05f,
                VisualizerMode.DancingColors => 1.65f,
                VisualizerMode.Sphere3D => 0.90f,
                _ => 0f
            };

            if (phaseStep > 0)
                animationPhase = (animationPhase + phaseStep + (frame.RmsLevel * 2.4f)) % 360f;
        }

        Invalidate();
    }

    public void ClearFrame()
    {
        Array.Clear(spectrumLevels);
        Array.Clear(peakHoldLevels);
        Array.Clear(waveformPoints);
        peakLevel = 0;
        rmsLevel = 0;
        isActive = false;
        diskAngle = 0;
        animationPhase = 0;
        playbackTimeSeconds = 0;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        var bounds = ClientRectangle;
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return;

        if (embeddedVisualizer is { IsFaulted: false } session)
        {
            var embeddedScene = CreateScene(session.DisplayLabel);
            var instructions = session.Render(embeddedScene);
            if (!session.IsFaulted)
            {
                embeddedVisualizerRenderer.Draw(e.Graphics, bounds, embeddedScene, instructions);
                return;
            }
        }

        var definition = VisualizerCatalog.GetDefinition(mode);
        definition.Renderer.Draw(e.Graphics, bounds, CreateScene(definition.Label));
    }

    private VisualizerScene CreateScene(string modeLabel) =>
        new()
        {
            Font = Font,
            ModeLabel = modeLabel,
            Theme = visualizerTheme,
            SpectrumLevels = spectrumLevels,
            PeakHoldLevels = peakHoldLevels,
            WaveformPoints = waveformPoints,
            PeakLevel = peakLevel,
            RmsLevel = rmsLevel,
            PlaybackTimeSeconds = playbackTimeSeconds,
            IsActive = isActive,
            ShowPeaks = showPeaks,
            AlbumArt = albumArt,
            DiskAngle = diskAngle,
            AnimationPhase = animationPhase
        };

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            embeddedVisualizerSource = null;
            embeddedVisualizer?.Dispose();
            embeddedVisualizer = null;
        }

        base.Dispose(disposing);
    }
}
