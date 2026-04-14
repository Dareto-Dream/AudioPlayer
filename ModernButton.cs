using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace AudioPlayer;

/// <summary>
/// Custom-drawn button. Supports solid, ghost, and pill variants.
/// </summary>
public sealed class ModernButton : Control
{
    private Color accentColor = Color.FromArgb(50, 74, 128);
    private bool isHovering;
    private bool isPressed;
    private float hoverAlpha;
    private readonly System.Windows.Forms.Timer animTimer;

    public ModernButton()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.UserPaint |
            ControlStyles.ResizeRedraw |
            ControlStyles.Selectable,
            true);

        Cursor = Cursors.Hand;
        TabStop = true;
        Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
        ForeColor = Color.FromArgb(210, 225, 255);

        animTimer = new System.Windows.Forms.Timer { Interval = 16 };
        animTimer.Tick += OnAnimTick;
    }

    /// <summary>Background fill color (ignored when IsGhost = true).</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color AccentColor
    {
        get => accentColor;
        set { accentColor = value; Invalidate(); }
    }

    /// <summary>When true, renders as a fully-rounded pill shape.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Pill { get; set; } = false;

    /// <summary>
    /// When true, renders transparent with a subtle border instead of a filled background.
    /// On hover a very light tint of AccentColor is shown.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsGhost { get; set; } = false;

    private void OnAnimTick(object? sender, EventArgs e)
    {
        var target = isHovering ? 1f : 0f;
        hoverAlpha += (target - hoverAlpha) * 0.25f;
        if (Math.Abs(hoverAlpha - target) < 0.005f)
        {
            hoverAlpha = target;
            if (!isHovering) animTimer.Stop();
        }
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        isHovering = true;
        animTimer.Start();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        isHovering = false;
        animTimer.Start();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left && Enabled)
        {
            isPressed = true;
            Focus();
            Invalidate();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button == MouseButtons.Left)
        {
            var wasPressed = isPressed;
            isPressed = false;
            Invalidate();
            if (wasPressed && ClientRectangle.Contains(e.Location))
                OnClick(EventArgs.Empty);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if ((e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space) && Enabled)
        {
            isPressed = true;
            Invalidate();
            e.Handled = true;
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
        {
            isPressed = false;
            Invalidate();
            if (Enabled) OnClick(EventArgs.Empty);
            e.Handled = true;
        }
    }

    protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
    protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

        var bounds = new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f);
        var radius = Pill ? Height / 2f : 8f;
        var pressShift = isPressed ? 1 : 0;
        var h = hoverAlpha;

        using var path = CreateRoundedPath(bounds, radius);

        if (IsGhost)
        {
            // Ghost: transparent → subtle tint on hover
            if (h > 0.01f)
            {
                var tintAlpha = (int)(h * 38);
                using var tintBrush = new SolidBrush(Color.FromArgb(tintAlpha, accentColor));
                g.FillPath(tintBrush, path);
            }

            // Border: more visible on hover
            var borderAlpha = (int)(55 + h * 110);
            using var borderPen = new Pen(Color.FromArgb(borderAlpha, 130, 155, 205), 1.5f);
            g.DrawPath(borderPen, path);
        }
        else
        {
            // Solid: base color that lightens on hover
            var r  = (int)(accentColor.R + (Math.Min(255, accentColor.R + 50) - accentColor.R) * h);
            var gr = (int)(accentColor.G + (Math.Min(255, accentColor.G + 42) - accentColor.G) * h);
            var b  = (int)(accentColor.B + (Math.Min(255, accentColor.B + 50) - accentColor.B) * h);
            var alpha = Enabled ? 255 : 120;

            using var fillBrush = new SolidBrush(Color.FromArgb(alpha, r, gr, b));
            g.FillPath(fillBrush, path);

            // Subtle top-edge gloss
            var halfBounds = new RectangleF(bounds.Left, bounds.Top, bounds.Width, bounds.Height * 0.5f);
            using var glossPath = CreateRoundedPath(halfBounds, radius);
            using var glossBrush = new LinearGradientBrush(
                new PointF(0, bounds.Top),
                new PointF(0, bounds.Top + bounds.Height * 0.5f),
                Color.FromArgb(35, 255, 255, 255),
                Color.FromArgb(0, 255, 255, 255));
            g.FillPath(glossBrush, glossPath);

            // Subtle border
            var borderAlpha = (int)(18 + h * 50);
            using var borderPen = new Pen(Color.FromArgb(borderAlpha, 220, 250, 255), 1f);
            g.DrawPath(borderPen, path);
        }

        // Focus ring
        if (Focused)
        {
            var fb = new RectangleF(bounds.Left - 2, bounds.Top - 2, bounds.Width + 4, bounds.Height + 4);
            using var focusPath = CreateRoundedPath(fb, radius + 2);
            using var focusPen = new Pen(Color.FromArgb(150, 140, 195, 255), 1.5f);
            g.DrawPath(focusPen, focusPath);
        }

        // Text
        var textAlpha = Enabled ? (IsGhost ? (int)(140 + h * 90) : 255) : 80;
        using var textBrush = new SolidBrush(Color.FromArgb(textAlpha, ForeColor));
        using var sf = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter
        };
        g.DrawString(Text, Font, textBrush,
            new RectangleF(pressShift, pressShift, Width - pressShift * 2, Height - pressShift * 2),
            sf);
    }

    private static GraphicsPath CreateRoundedPath(RectangleF bounds, float radius)
    {
        var path = new GraphicsPath();
        var r = Math.Min(radius, Math.Min(bounds.Width, bounds.Height) / 2f);
        if (r < 0.5f) { path.AddRectangle(bounds); return path; }
        var d = r * 2;
        path.AddArc(bounds.Left, bounds.Top, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Top, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.Left, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) animTimer.Dispose();
        base.Dispose(disposing);
    }
}
