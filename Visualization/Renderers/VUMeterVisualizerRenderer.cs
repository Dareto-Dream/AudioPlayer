using System.Drawing;
using System.Drawing.Drawing2D;

namespace Spectrallis;

// Retro LED VU-meter bar graph. 12 channels, each with 18 stacked segments that
// light up green → yellow → red as level rises. Colors are blended from the theme
// accent so every accent still looks intentional.
internal sealed class VUMeterVisualizerRenderer : VisualizerRendererBase
{
    private const int ChannelCount = 12;
    private const int SegmentCount = 18;
    private const int SegmentGap = 3;

    public override void Draw(Graphics graphics, Rectangle bounds, VisualizerScene scene)
    {
        DrawBackground(graphics, bounds, scene);
        DrawMeters(graphics, bounds, scene);
        DrawHud(graphics, bounds, scene);

        if (IsNearSilence(scene))
            DrawPlaceholder(graphics, bounds, scene);
    }

    private static void DrawMeters(Graphics graphics, Rectangle bounds, VisualizerScene scene)
    {
        var contentBounds = Rectangle.Inflate(bounds, -20, -58);
        var gap = Math.Max(3, contentBounds.Width / (ChannelCount * 5));
        var meterWidth = (contentBounds.Width - (gap * (ChannelCount - 1))) / ChannelCount;
        var segH = (contentBounds.Height - (SegmentGap * (SegmentCount - 1))) / SegmentCount;
        segH = Math.Max(3, segH);

        for (var channel = 0; channel < ChannelCount; channel++)
        {
            var level = SampleRange(scene.SpectrumLevels, channel, ChannelCount);
            var activeSegs = (int)(level * SegmentCount);
            var x = contentBounds.Left + (channel * (meterWidth + gap));

            for (var seg = 0; seg < SegmentCount; seg++)
            {
                var segY = contentBounds.Bottom - ((seg + 1) * (segH + SegmentGap)) + SegmentGap;
                var segRect = new Rectangle(x, segY, meterWidth, segH);

                Color segColor;
                int alpha;

                if (seg >= SegmentCount - 2)
                {
                    // Top 2 segments: red zone
                    segColor = Color.FromArgb(218, 72, 72);
                    alpha = seg < activeSegs ? 228 : 24;
                }
                else if (seg >= SegmentCount - 5)
                {
                    // Next 3: yellow zone
                    segColor = Color.FromArgb(222, 192, 55);
                    alpha = seg < activeSegs ? 212 : 24;
                }
                else
                {
                    // Lower range: blended from theme accent
                    segColor = ThemePalette.Blend(
                        scene.Theme.BarStartColor,
                        scene.Theme.BarEndColor,
                        seg / (float)SegmentCount);
                    alpha = seg < activeSegs ? 210 : 20;
                }

                using var brush = new SolidBrush(Color.FromArgb(alpha, segColor));
                using var path = CreateRoundedRectangle(segRect, 2);
                graphics.FillPath(brush, path);
            }

            // Peak hold marker
            if (scene.ShowPeaks && activeSegs > 0)
            {
                var peakSeg = Math.Clamp(activeSegs - 1, 0, SegmentCount - 1);
                var peakY = contentBounds.Bottom - ((peakSeg + 1) * (segH + SegmentGap)) + SegmentGap;
                using var peakBrush = new SolidBrush(Color.FromArgb(235, scene.Theme.PeakColor));
                graphics.FillRectangle(peakBrush, x, peakY, meterWidth, 2);
            }
        }
    }
}
