using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace AudioPlayer;

/// <summary>
/// Custom-drawn horizontal slider that replaces the system TrackBar.
/// Used for seeking, volume, and sensitivity controls.
/// </summary>
public sealed class ModernSlider : Control
{
    private int minimum;
    private int maximum = 100;
    private int currentValue;
    private bool isDragging;
    private bool isHovering;
    private float hoverAlpha;
    private readonly System.Windows.Forms.Timer animTimer;

    public event EventHandler? Scroll;

    public ModernSlider()
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
        AutoSize = false;

        animTimer = new System.Windows.Forms.Timer { Interval = 16 };
        animTimer.Tick += OnAnimTick;
    }

    /// <summary>When true, renders as a prominent seek bar with a larger thumb.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsLarge { get; set; } = false;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Minimum
    {
        get => minimum;
        set { minimum = value; Invalidate(); }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Maximum
    {
        get => maximum;
        set { maximum = Math.Max(minimum + 1, value); Invalidate(); }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Value
    {
        get => currentValue;
        set
        {
            currentValue = Math.Clamp(value, minimum, maximum);
            Invalidate();
        }
    }

    // Expose TickStyle as a no-op for Designer compatibility
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public System.Windows.Forms.TickStyle TickStyle { get; set; } = System.Windows.Forms.TickStyle.None;

    private void OnAnimTick(object? sender, EventArgs e)
    {
        var target = (isHovering || isDragging) ? 1f : 0f;
        hoverAlpha += (target - hoverAlpha) * 0.22f;
        if (Math.Abs(hoverAlpha - target) < 0.005f)
        {
            hoverAlpha = target;
            if (!isHovering && !isDragging)
                animTimer.Stop();
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
        if (!isDragging) animTimer.Start();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left && Enabled)
        {
            isDragging = true;
            Focus();
            SetValueFromX(e.X);
            Scroll?.Invoke(this, EventArgs.Empty);
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (isDragging)
        {
            SetValueFromX(e.X);
            Scroll?.Invoke(this, EventArgs.Empty);
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button == MouseButtons.Left)
        {
            isDragging = false;
            animTimer.Start();
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        var step = Math.Max(1, (maximum - minimum) / 20);
        switch (e.KeyCode)
        {
            case Keys.Left:
            case Keys.Down:
                Value = Math.Max(minimum, currentValue - step);
                Scroll?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
                break;
            case Keys.Right:
            case Keys.Up:
                Value = Math.Min(maximum, currentValue + step);
                Scroll?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
                break;
            case Keys.Home:
                Value = minimum;
                Scroll?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
                break;
            case Keys.End:
                Value = maximum;
                Scroll?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
                break;
        }
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        Invalidate();
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        Invalidate();
    }

    private void SetValueFromX(int mouseX)
    {
        var pad = GetPad();
        var trackWidth = Width - pad * 2;
        if (trackWidth <= 0) return;
        var fraction = Math.Clamp((mouseX - pad) / (float)trackWidth, 0f, 1f);
        currentValue = (int)Math.Round(minimum + fraction * (maximum - minimum));
        Invalidate();
    }

    private int GetPad() => IsLarge ? 10 : 5;

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

        var pad = GetPad();
        var trackLeft = pad;
        var trackRight = Width - pad;
        var trackWidth = trackRight - trackLeft;
        var centerY = Height / 2;

        // Track height and thumb size animate on hover
        var baseTrackH = IsLarge ? 3f : 3f;
        var activeTrackH = IsLarge ? 6f : 5f;
        var trackH = baseTrackH + (activeTrackH - baseTrackH) * hoverAlpha;

        var baseThumb = IsLarge ? 13 : 11;
        var hoverThumb = IsLarge ? 20 : 15;
        var thumbSize = (int)(baseThumb + (hoverThumb - baseThumb) * hoverAlpha);

        var fraction = maximum > minimum
            ? (currentValue - minimum) / (float)(maximum - minimum)
            : 0f;
        var thumbX = trackLeft + (int)(trackWidth * fraction);
        var trackY = centerY - trackH / 2f;

        // Track background
        DrawRoundedRect(g,
            new RectangleF(trackLeft, trackY, trackWidth, trackH),
            trackH / 2f,
            Color.FromArgb(70, 180, 210, 245));

        // Filled portion
        var filledW = trackWidth * fraction;
        if (filledW >= trackH && trackH > 0)
        {
            using var fillBrush = new LinearGradientBrush(
                new PointF(trackLeft, 0),
                new PointF(trackLeft + filledW + 1, 0),
                Color.FromArgb(100, 175, 255),
                Color.FromArgb(80, 255, 180));
            DrawRoundedRect(g,
                new RectangleF(trackLeft, trackY, filledW, trackH),
                trackH / 2f,
                fillBrush);
        }

        // Focus ring
        if (Focused)
        {
            using var focusPen = new Pen(Color.FromArgb(90, 120, 200, 255), 1f);
            g.DrawRectangle(focusPen, 1, 1, Width - 3, Height - 3);
        }

        // Thumb (only when enabled)
        if (Enabled)
        {
            // Glow
            var glowAlpha = (int)(25 + hoverAlpha * 90);
            var glowSize = thumbSize + 12;
            using var glowBrush = new SolidBrush(Color.FromArgb(glowAlpha, 90, 190, 255));
            g.FillEllipse(glowBrush,
                thumbX - glowSize / 2,
                centerY - glowSize / 2,
                glowSize, glowSize);

            // Outer thumb
            using var thumbBrush = new SolidBrush(Color.FromArgb(240, 220, 242, 255));
            g.FillEllipse(thumbBrush,
                thumbX - thumbSize / 2,
                centerY - thumbSize / 2,
                thumbSize, thumbSize);

            // Inner highlight
            var highlightSize = Math.Max(2, thumbSize / 3);
            using var hlBrush = new SolidBrush(Color.FromArgb(130, 255, 255, 255));
            g.FillEllipse(hlBrush,
                thumbX - highlightSize / 2,
                centerY - thumbSize / 2 + 2,
                highlightSize, highlightSize);
        }
    }

    private static void DrawRoundedRect(Graphics g, RectangleF rect, float radius, Color color)
    {
        if (rect.Width <= 0 || rect.Height <= 0) return;
        using var brush = new SolidBrush(color);
        DrawRoundedRect(g, rect, radius, brush);
    }

    private static void DrawRoundedRect(Graphics g, RectangleF rect, float radius, Brush brush)
    {
        if (rect.Width <= 0 || rect.Height <= 0) return;
        var r = Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2f);
        if (r < 0.5f)
        {
            g.FillRectangle(brush, rect);
            return;
        }
        var d = r * 2;
        using var path = new GraphicsPath();
        path.AddArc(rect.Left, rect.Top, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Top, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.Left, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        g.FillPath(brush, path);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) animTimer.Dispose();
        base.Dispose(disposing);
    }
}
