using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace AudioPlayer;

public sealed class SpectrumVisualizerControl : Control
{
    private Color backgroundTopColor = Color.FromArgb(24, 19, 24);
    private Color backgroundBottomColor = Color.FromArgb(10, 8, 12);
    private Color ambientGlowColor = Color.FromArgb(244, 152, 82);
    private Color ambientGridColor = Color.FromArgb(159, 121, 88);
    private Color barGlowColor = Color.FromArgb(238, 144, 94);
    private Color barStartColor = Color.FromArgb(248, 188, 98);
    private Color barEndColor = Color.FromArgb(226, 111, 80);
    private Color peakColor = Color.FromArgb(255, 235, 189);
    private Color hudLabelColor = Color.FromArgb(244, 236, 227);
    private Color hudInfoColor = Color.FromArgb(191, 174, 159);
    private Color placeholderColor = Color.FromArgb(188, 175, 161);
    private Color diskFillColor = Color.FromArgb(34, 29, 35);
    private Color diskGrooveColor = Color.FromArgb(168, 143, 120);
    private Color hubColor = Color.FromArgb(20, 17, 22);
    private Color hubDotColor = Color.FromArgb(176, 132, 93);
    private Color ringColor = Color.FromArgb(173, 124, 90);
    private Color idleLabelColor = Color.FromArgb(170, 156, 145);

    private readonly float[] spectrumLevels = new float[64];
    private readonly float[] peakHoldLevels = new float[64];
    private readonly float[] waveformPoints = new float[256];

    private VisualizerMode mode = VisualizerMode.MirrorSpectrum;
    private bool showPeaks = true;
    private float sensitivity = 1;
    private float peakLevel;
    private float rmsLevel;
    private bool isActive;
    private Image? albumArt;
    private float diskAngle;

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

        BackColor = backgroundTopColor;
        ForeColor = barStartColor;
    }

    internal void ApplyTheme(ThemePalette palette)
    {
        backgroundTopColor = ThemePalette.Blend(palette.SurfaceBackColor, palette.WindowBackColor, palette.IsDark ? 0.30f : 0.08f);
        backgroundBottomColor = ThemePalette.Blend(palette.WindowBackColor, Color.Black, palette.IsDark ? 0.52f : 0.06f);
        ambientGlowColor = palette.AccentPrimaryColor;
        ambientGridColor = ThemePalette.Blend(palette.BorderStrongColor, palette.AccentPrimaryColor, 0.22f);
        barGlowColor = ThemePalette.Blend(palette.AccentPrimaryColor, palette.AccentSecondaryColor, 0.55f);
        barStartColor = ThemePalette.Blend(palette.AccentPrimaryColor, Color.White, palette.IsDark ? 0.10f : 0.04f);
        barEndColor = palette.AccentSecondaryColor;
        peakColor = ThemePalette.Blend(palette.TextPrimaryColor, palette.AccentPrimaryColor, palette.IsDark ? 0.16f : 0.08f);
        hudLabelColor = palette.TextPrimaryColor;
        hudInfoColor = palette.TextSecondaryColor;
        placeholderColor = palette.TextSoftColor;
        diskFillColor = ThemePalette.Blend(palette.SurfaceRaisedColor, palette.WindowBackColor, palette.IsDark ? 0.36f : 0.08f);
        diskGrooveColor = ThemePalette.Blend(palette.TextSoftColor, palette.AccentPrimaryColor, 0.20f);
        hubColor = ThemePalette.Blend(palette.WindowBackColor, Color.Black, palette.IsDark ? 0.18f : 0.02f);
        hubDotColor = palette.AccentSecondaryColor;
        ringColor = ThemePalette.Blend(palette.BorderStrongColor, palette.AccentPrimaryColor, 0.30f);
        idleLabelColor = palette.TextMutedColor;

        BackColor = backgroundTopColor;
        ForeColor = barStartColor;
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

    /// <summary>Album art displayed in SpinningDisk mode. Not owned/disposed by this control.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Image? AlbumArt
    {
        get => albumArt;
        set { albumArt = value; Invalidate(); }
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

        if (mode == VisualizerMode.SpinningDisk && activePlayback)
            diskAngle = (diskAngle + 0.38f) % 360f;

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
                DrawHud(e.Graphics, bounds);
                if (IsNearSilence()) DrawPlaceholder(e.Graphics, bounds);
                break;
            case VisualizerMode.MirrorSpectrum:
                DrawSpectrumBars(e.Graphics, bounds, mirrored: true);
                DrawHud(e.Graphics, bounds);
                if (IsNearSilence()) DrawPlaceholder(e.Graphics, bounds);
                break;
            case VisualizerMode.Waveform:
                DrawWaveform(e.Graphics, bounds);
                DrawHud(e.Graphics, bounds);
                if (IsNearSilence()) DrawPlaceholder(e.Graphics, bounds);
                break;
            case VisualizerMode.SpinningDisk:
                DrawSpinningDisk(e.Graphics, bounds);
                break;
        }
    }

    private void DrawBackground(Graphics graphics, Rectangle bounds)
    {
        var glowStrength = Math.Clamp((peakLevel * 0.45f) + (rmsLevel * 0.95f), 0.05f, 0.75f);

        using var backgroundBrush = new LinearGradientBrush(
            bounds,
            backgroundTopColor,
            backgroundBottomColor,
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
            CenterColor = Color.FromArgb((int)(92 * glowStrength), ambientGlowColor),
            SurroundColors = new[]
            {
                Color.FromArgb(0, ambientGlowColor),
                Color.FromArgb(0, ambientGlowColor),
                Color.FromArgb(0, ambientGlowColor),
                Color.FromArgb(0, ambientGlowColor),
                Color.FromArgb(0, ambientGlowColor)
            }
        };
        graphics.FillRectangle(accentBrush, bounds);
    }

    private void DrawGrid(Graphics graphics, Rectangle bounds)
    {
        using var horizontalPen = new Pen(Color.FromArgb(22, ambientGridColor));
        using var verticalPen = new Pen(Color.FromArgb(14, ambientGridColor));

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

        using var glowBrush = new SolidBrush(Color.FromArgb(22, barGlowColor));
        using var fillBrush = new LinearGradientBrush(
            contentBounds,
            barStartColor,
            barEndColor,
            LinearGradientMode.Vertical);
        using var peakPen = new Pen(Color.FromArgb(210, peakColor), 2);

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

        using var glowPen = new Pen(Color.FromArgb(40, ambientGlowColor), 8)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        using var wavePen = new Pen(Color.FromArgb(240, barStartColor), 3)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        using var centerPen = new Pen(Color.FromArgb(72, hudLabelColor), 1.5f);

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

        using var labelBrush = new SolidBrush(hudLabelColor);
        using var infoBrush = new SolidBrush(hudInfoColor);
        using var meterBackBrush = new SolidBrush(Color.FromArgb(42, 255, 245, 230));
        using var meterFillBrush = new LinearGradientBrush(
            meterRect,
            barStartColor,
            barEndColor,
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
        using var textBrush = new SolidBrush(placeholderColor);
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

    private void DrawSpinningDisk(Graphics g, Rectangle bounds)
    {
        var size = Math.Min(bounds.Width, bounds.Height) - 40;
        if (size <= 0) return;
        var cx = bounds.Left + bounds.Width / 2;
        var cy = bounds.Top + bounds.Height / 2;
        var diskRect = new Rectangle(cx - size / 2, cy - size / 2, size, size);

        // --- rotated art (or solid dark fill) clipped to the disk circle ---
        using var clipPath = new GraphicsPath();
        clipPath.AddEllipse(diskRect);

        var phase1 = g.Save();
        g.SetClip(clipPath, CombineMode.Intersect);

        if (albumArt != null)
        {
            var phase2 = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(diskAngle);
            g.DrawImage(albumArt, -size / 2f, -size / 2f, size, size);
            g.Restore(phase2);

            // Slight darkening so grooves read on bright covers
            using var tint = new SolidBrush(Color.FromArgb(48, 0, 0, 0));
            g.FillEllipse(tint, diskRect);
        }
        else
        {
            using var fill = new SolidBrush(diskFillColor);
            g.FillEllipse(fill, diskRect);
        }

        // Vinyl groove lines
        var grooveAlpha = albumArt != null ? 20 : 45;
        using var groovePen = new Pen(Color.FromArgb(grooveAlpha, diskGrooveColor), 1f);
        for (var r = size / 5; r < size / 2 - 4; r += 10)
            g.DrawEllipse(groovePen, cx - r, cy - r, r * 2, r * 2);

        g.Restore(phase1);

        // --- center hub (drawn over clip, unclipped) ---
        var hub = Math.Max(22, size / 6);
        using var hubBrush = new SolidBrush(hubColor);
        g.FillEllipse(hubBrush, cx - hub / 2, cy - hub / 2, hub, hub);

        var dot = Math.Max(6, hub / 3);
        using var dotBrush = new SolidBrush(hubDotColor);
        g.FillEllipse(dotBrush, cx - dot / 2, cy - dot / 2, dot, dot);

        // Outer edge ring
        using var ring = new Pen(Color.FromArgb(72, ringColor), 2f);
        g.DrawEllipse(ring, diskRect);

        // Idle label when not playing
        if (!isActive)
        {
            using var tb = new SolidBrush(idleLabelColor);
            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far };
            g.DrawString("paused", Font, tb, new RectangleF(bounds.X, bounds.Y, bounds.Width, cy - hub / 2f - 6), sf);
        }
    }

    private string GetModeLabel() =>
        mode switch
        {
            VisualizerMode.Spectrum => "Spectrum",
            VisualizerMode.MirrorSpectrum => "Mirror Spectrum",
            VisualizerMode.Waveform => "Waveform",
            VisualizerMode.SpinningDisk => "Spinning Disk",
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
