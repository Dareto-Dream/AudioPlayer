using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace AudioPlayer;

public sealed class SpectrumVisualizerControl : Control
{
    private readonly float[] spectrumLevels = new float[64];
    private readonly float[] peakHoldLevels = new float[64];
    private readonly float[] waveformPoints = new float[256];

    private VisualizerMode mode = VisualizerMode.MirrorSpectrum;
    private bool showPeaks = true;
    private float sensitivity = 1;
    private float peakLevel;
    private float rmsLevel;
    private bool isActive;

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

        BackColor = Color.FromArgb(8, 12, 24);
        ForeColor = Color.FromArgb(98, 242, 199);
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

    public void UpdateFrame(VisualizerFrame frame, bool activePlayback)
    {
        isActive = activePlayback;

        for (var i = 0; i < spectrumLevels.Length; i++)
        {
            var incoming = i < frame.Spectrum.Length ? Math.Clamp(frame.Spectrum[i] * sensitivity, 0, 1.25f) : 0;
            spectrumLevels[i] = Math.Max(incoming, spectrumLevels[i] * (activePlayback ? 0.80f : 0.90f));
            peakHoldLevels[i] = showPeaks
                ? Math.Max(spectrumLevels[i], peakHoldLevels[i] - 0.03f)
                : 0;
        }

        for (var i = 0; i < waveformPoints.Length; i++)
        {
            var incoming = i < frame.Waveform.Length ? frame.Waveform[i] * sensitivity : 0;
            waveformPoints[i] = (waveformPoints[i] * 0.35f) + (incoming * 0.65f);
        }

        peakLevel = Math.Max(frame.PeakLevel * sensitivity, peakLevel * (activePlayback ? 0.90f : 0.95f));
        rmsLevel = Math.Max(frame.RmsLevel * sensitivity, rmsLevel * (activePlayback ? 0.92f : 0.96f));

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
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        var bounds = ClientRectangle;
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return;
        }

        DrawBackground(e.Graphics, bounds);
        DrawGrid(e.Graphics, bounds);

        switch (mode)
        {
            case VisualizerMode.Spectrum:
                DrawSpectrumBars(e.Graphics, bounds, mirrored: false);
                break;
            case VisualizerMode.MirrorSpectrum:
                DrawSpectrumBars(e.Graphics, bounds, mirrored: true);
                break;
            case VisualizerMode.Waveform:
                DrawWaveform(e.Graphics, bounds);
                break;
        }

        DrawHud(e.Graphics, bounds);

        if (IsNearSilence())
        {
            DrawPlaceholder(e.Graphics, bounds);
        }
    }

    private void DrawBackground(Graphics graphics, Rectangle bounds)
    {
        var glowStrength = Math.Clamp((peakLevel * 0.45f) + (rmsLevel * 0.95f), 0.05f, 0.75f);

        using var backgroundBrush = new LinearGradientBrush(
            bounds,
            Color.FromArgb(9, 13, 26),
            Color.FromArgb(2, 6, 16),
            LinearGradientMode.Vertical);
        graphics.FillRectangle(backgroundBrush, bounds);

        using var accentBrush = new PathGradientBrush(
            new[]
            {
                new Point(bounds.Width / 2, bounds.Height / 2),
                new Point(bounds.Right, bounds.Height / 3),
                new Point(bounds.Right - 20, bounds.Bottom - 20),
                new Point(bounds.Left + 20, bounds.Bottom - 10),
                new Point(bounds.Left, bounds.Height / 3)
            })
        {
            CenterColor = Color.FromArgb((int)(80 * glowStrength), 64, 204, 255),
            SurroundColors = new[]
            {
                Color.FromArgb(0, 64, 204, 255),
                Color.FromArgb(0, 64, 204, 255),
                Color.FromArgb(0, 64, 204, 255),
                Color.FromArgb(0, 64, 204, 255),
                Color.FromArgb(0, 64, 204, 255)
            }
        };
        graphics.FillRectangle(accentBrush, bounds);
    }

    private void DrawGrid(Graphics graphics, Rectangle bounds)
    {
        using var horizontalPen = new Pen(Color.FromArgb(22, 120, 160, 190));
        using var verticalPen = new Pen(Color.FromArgb(14, 120, 160, 190));

        for (var index = 1; index < 5; index++)
        {
            var y = bounds.Top + (index * bounds.Height / 5);
            graphics.DrawLine(horizontalPen, bounds.Left, y, bounds.Right, y);
        }

        for (var index = 1; index < 10; index++)
        {
            var x = bounds.Left + (index * bounds.Width / 10);
            graphics.DrawLine(verticalPen, x, bounds.Top, x, bounds.Bottom);
        }
    }

    private void DrawSpectrumBars(Graphics graphics, Rectangle bounds, bool mirrored)
    {
        var contentBounds = Rectangle.Inflate(bounds, -18, -18);
        var displayBars = Math.Clamp(contentBounds.Width / 14, 18, spectrumLevels.Length);
        var gap = Math.Max(3, contentBounds.Width / (displayBars * 7));
        var totalGapWidth = gap * (displayBars - 1);
        var barWidth = Math.Max(5, (contentBounds.Width - totalGapWidth) / displayBars);
        var cornerRadius = Math.Max(4, barWidth / 2);
        var centerY = contentBounds.Top + (contentBounds.Height / 2);

        using var glowBrush = new SolidBrush(Color.FromArgb(18, 56, 234, 242));
        using var fillBrush = new LinearGradientBrush(
            contentBounds,
            Color.FromArgb(48, 213, 255),
            Color.FromArgb(108, 255, 160),
            LinearGradientMode.Vertical);
        using var peakPen = new Pen(Color.FromArgb(210, 255, 246, 158), 2);

        for (var index = 0; index < displayBars; index++)
        {
            var level = SampleRange(spectrumLevels, index, displayBars);
            var barHeight = Math.Max(6, (int)((mirrored ? contentBounds.Height / 2f : contentBounds.Height) * level));
            var x = contentBounds.Left + (index * (barWidth + gap));

            if (mirrored)
            {
                var upperRect = new Rectangle(x, centerY - barHeight, barWidth, Math.Max(2, barHeight - 2));
                var lowerRect = new Rectangle(x, centerY + 2, barWidth, Math.Max(2, barHeight - 2));

                graphics.FillRectangle(glowBrush, Rectangle.Inflate(upperRect, 0, 4));
                graphics.FillRectangle(glowBrush, Rectangle.Inflate(lowerRect, 0, 4));

                using var upperPath = CreateRoundedRectangle(upperRect, cornerRadius);
                using var lowerPath = CreateRoundedRectangle(lowerRect, cornerRadius);
                graphics.FillPath(fillBrush, upperPath);
                graphics.FillPath(fillBrush, lowerPath);

                if (showPeaks)
                {
                    var peakHeight = (int)((contentBounds.Height / 2f) * SampleRange(peakHoldLevels, index, displayBars));
                    var peakY = centerY - peakHeight - 4;
                    graphics.DrawLine(peakPen, x + 1, peakY, x + barWidth - 1, peakY);
                    graphics.DrawLine(peakPen, x + 1, centerY + peakHeight + 4, x + barWidth - 1, centerY + peakHeight + 4);
                }
            }
            else
            {
                var y = contentBounds.Bottom - barHeight;
                var barRect = new Rectangle(x, y, barWidth, barHeight);

                graphics.FillRectangle(glowBrush, Rectangle.Inflate(barRect, 0, 4));

                using var path = CreateRoundedRectangle(barRect, cornerRadius);
                graphics.FillPath(fillBrush, path);

                if (showPeaks)
                {
                    var peakHeight = (int)(contentBounds.Height * SampleRange(peakHoldLevels, index, displayBars));
                    var peakY = contentBounds.Bottom - peakHeight;
                    graphics.DrawLine(peakPen, x + 1, peakY, x + barWidth - 1, peakY);
                }
            }
        }
    }

    private void DrawWaveform(Graphics graphics, Rectangle bounds)
    {
        var contentBounds = Rectangle.Inflate(bounds, -18, -24);
        var centerY = contentBounds.Top + (contentBounds.Height / 2f);

        using var glowPen = new Pen(Color.FromArgb(36, 64, 224, 255), 8)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        using var wavePen = new Pen(Color.FromArgb(115, 255, 176), 3)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        using var centerPen = new Pen(Color.FromArgb(80, 255, 255, 255), 1.5f);

        graphics.DrawLine(centerPen, contentBounds.Left, centerY, contentBounds.Right, centerY);

        using var wavePath = BuildWaveformPath(contentBounds, centerY);
        graphics.DrawPath(glowPen, wavePath);
        graphics.DrawPath(wavePen, wavePath);
    }

    private void DrawHud(Graphics graphics, Rectangle bounds)
    {
        var hudBounds = new Rectangle(bounds.Left + 18, bounds.Top + 14, bounds.Width - 36, 40);
        var meterWidth = 120;
        var meterHeight = 8;
        var meterRect = new Rectangle(hudBounds.Right - meterWidth, hudBounds.Top + 12, meterWidth, meterHeight);

        using var labelBrush = new SolidBrush(Color.FromArgb(210, 228, 239, 255));
        using var infoBrush = new SolidBrush(Color.FromArgb(164, 173, 194, 220));
        using var meterBackBrush = new SolidBrush(Color.FromArgb(42, 255, 255, 255));
        using var meterFillBrush = new LinearGradientBrush(
            meterRect,
            Color.FromArgb(50, 223, 255),
            Color.FromArgb(112, 255, 179),
            LinearGradientMode.Horizontal);

        graphics.DrawString(GetModeLabel(), Font, labelBrush, hudBounds.Left, hudBounds.Top);
        graphics.DrawString(
            isActive ? "Live" : "Idle",
            Font,
            infoBrush,
            hudBounds.Left,
            hudBounds.Top + 18);

        graphics.FillRectangle(meterBackBrush, meterRect);
        graphics.FillRectangle(
            meterFillBrush,
            meterRect.Left,
            meterRect.Top,
            Math.Max(6, (int)(meterRect.Width * Math.Clamp(peakLevel, 0, 1))),
            meterRect.Height);
        graphics.DrawString($"Peak {(int)(Math.Clamp(peakLevel, 0, 1) * 100)}%", Font, infoBrush, meterRect.Left - 64, meterRect.Top - 6);
    }

    private void DrawPlaceholder(Graphics graphics, Rectangle bounds)
    {
        using var textBrush = new SolidBrush(Color.FromArgb(180, 191, 208, 230));
        using var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        graphics.DrawString("Drop audio here or use Open to start playback", Font, textBrush, bounds, format);
    }

    private GraphicsPath BuildWaveformPath(Rectangle contentBounds, float centerY)
    {
        var path = new GraphicsPath();
        var points = new PointF[waveformPoints.Length];

        for (var index = 0; index < waveformPoints.Length; index++)
        {
            var x = contentBounds.Left + (index / (float)(waveformPoints.Length - 1) * contentBounds.Width);
            var y = centerY - (waveformPoints[index] * (contentBounds.Height * 0.45f));
            points[index] = new PointF(x, y);
        }

        if (points.Length > 1)
        {
            path.AddCurve(points, 0.35f);
        }

        return path;
    }

    private string GetModeLabel() =>
        mode switch
        {
            VisualizerMode.Spectrum => "Spectrum",
            VisualizerMode.MirrorSpectrum => "Mirror Spectrum",
            VisualizerMode.Waveform => "Waveform",
            _ => "Visualizer"
        };

    private bool IsNearSilence() =>
        spectrumLevels.All(level => level < 0.01f) &&
        waveformPoints.All(point => Math.Abs(point) < 0.01f);

    private static float SampleRange(float[] source, int index, int displayBars)
    {
        var start = index * source.Length / displayBars;
        var end = Math.Max(start + 1, ((index + 1) * source.Length) / displayBars);
        float total = 0;

        for (var position = start; position < end; position++)
        {
            total += source[position];
        }

        return total / (end - start);
    }

    private static GraphicsPath CreateRoundedRectangle(Rectangle bounds, int radius)
    {
        var safeRadius = Math.Min(radius, Math.Min(bounds.Width, bounds.Height) / 2);
        var diameter = Math.Max(2, safeRadius * 2);
        var path = new GraphicsPath();

        path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        return path;
    }
}
