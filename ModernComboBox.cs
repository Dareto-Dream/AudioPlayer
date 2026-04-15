using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace AudioPlayer;

public sealed class ModernComboBox : ComboBox
{
    private static readonly Color SurfaceColor = Color.FromArgb(35, 30, 36);
    private static readonly Color SurfaceHoverColor = Color.FromArgb(41, 35, 42);
    private static readonly Color SurfaceOpenColor = Color.FromArgb(44, 37, 44);
    private static readonly Color SurfaceActiveColor = Color.FromArgb(52, 43, 49);
    private static readonly Color BorderColor = Color.FromArgb(82, 68, 58);
    private static readonly Color BorderHoverColor = Color.FromArgb(129, 98, 74);
    private static readonly Color ButtonColor = Color.FromArgb(45, 38, 44);
    private static readonly Color ButtonActiveColor = Color.FromArgb(60, 49, 55);
    private static readonly Color TextColor = Color.FromArgb(242, 236, 228);
    private static readonly Color MutedTextColor = Color.FromArgb(166, 149, 136);
    private static readonly Color CaretColor = Color.FromArgb(228, 176, 106);

    private bool isHovering;

    public ModernComboBox()
    {
        DrawMode = DrawMode.OwnerDrawFixed;
        DropDownStyle = ComboBoxStyle.DropDownList;
        FlatStyle = FlatStyle.Flat;
        IntegralHeight = false;
        MaxDropDownItems = 10;
        ItemHeight = 34;

        BackColor = SurfaceColor;
        ForeColor = TextColor;
        Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new DrawMode DrawMode
    {
        get => base.DrawMode;
        set => base.DrawMode = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new ComboBoxStyle DropDownStyle
    {
        get => base.DropDownStyle;
        set => base.DropDownStyle = value;
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        isHovering = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        isHovering = false;
        Invalidate();
    }

    protected override void OnDropDown(EventArgs e)
    {
        base.OnDropDown(e);
        Invalidate();
    }

    protected override void OnDropDownClosed(EventArgs e)
    {
        base.OnDropDownClosed(e);
        Invalidate();
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

    protected override void OnSelectedIndexChanged(EventArgs e)
    {
        base.OnSelectedIndexChanged(e);
        Invalidate();
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        Invalidate();
    }

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        if (e.Bounds.Width <= 0 || e.Bounds.Height <= 0)
            return;

        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        var isEditPortion =
            (e.State & DrawItemState.ComboBoxEdit) == DrawItemState.ComboBoxEdit ||
            (!DroppedDown && e.Bounds.Height >= Height - 4);
        var isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected && !isEditPortion;
        var itemText = e.Index >= 0
            ? GetItemText(Items[e.Index])
            : SelectedIndex >= 0 ? GetItemText(Items[SelectedIndex]) : Text;

        var backgroundColor = !Enabled
            ? Color.FromArgb(42, 38, 42)
            : isEditPortion
                ? GetSurfaceColor()
                : isSelected ? SurfaceActiveColor : SurfaceColor;

        using (var backgroundBrush = new SolidBrush(backgroundColor))
        {
            graphics.FillRectangle(backgroundBrush, e.Bounds);
        }

        if (!isEditPortion)
        {
            using var dividerPen = new Pen(Color.FromArgb(34, 255, 255, 255), 1f);
            graphics.DrawLine(dividerPen, e.Bounds.Left + 10, e.Bounds.Bottom - 1, e.Bounds.Right - 10, e.Bounds.Bottom - 1);
        }

        var textRect = Rectangle.Inflate(e.Bounds, -14, 0);
        if (isEditPortion)
            textRect.Width -= 30;

        TextRenderer.DrawText(
            graphics,
            itemText,
            Font,
            textRect,
            Enabled ? TextColor : MutedTextColor,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
    }

    protected override void WndProc(ref Message m)
    {
        base.WndProc(ref m);

        const int WM_PAINT = 0x000F;
        if (m.Msg == WM_PAINT && IsHandleCreated)
            PaintChrome();
    }

    private void PaintChrome()
    {
        using var graphics = Graphics.FromHwnd(Handle);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        var outer = new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f);
        if (outer.Width <= 0 || outer.Height <= 0)
            return;

        var radius = 6f;
        var buttonRect = new RectangleF(Math.Max(outer.Left, outer.Right - 34f), outer.Top + 1f, 33f, outer.Height - 2f);

        using (var clipPath = CreateRoundedPath(outer, radius))
        {
            graphics.SetClip(clipPath);
            using var buttonBrush = new LinearGradientBrush(
                buttonRect,
                DroppedDown ? ButtonActiveColor : ButtonColor,
                DroppedDown ? SurfaceOpenColor : SurfaceHoverColor,
                LinearGradientMode.Vertical);
            graphics.FillRectangle(buttonBrush, buttonRect);
            graphics.ResetClip();
        }

        using (var separatorPen = new Pen(Color.FromArgb(52, 255, 255, 255), 1f))
        {
            graphics.DrawLine(separatorPen, buttonRect.Left, outer.Top + 6f, buttonRect.Left, outer.Bottom - 6f);
        }

        using (var caretBrush = new SolidBrush(Enabled ? CaretColor : MutedTextColor))
        using (var caretPath = new GraphicsPath())
        {
            var centerX = buttonRect.Left + (buttonRect.Width / 2f);
            var centerY = buttonRect.Top + (buttonRect.Height / 2f) + 1f;
            caretPath.AddPolygon(
                [
                    new PointF(centerX - 4.5f, centerY - 2.5f),
                    new PointF(centerX + 4.5f, centerY - 2.5f),
                    new PointF(centerX, centerY + 3.5f)
                ]);
            graphics.FillPath(caretBrush, caretPath);
        }

        using (var borderPath = CreateRoundedPath(outer, radius))
        using (var borderPen = new Pen(GetBorderColor(), 1f))
        {
            graphics.DrawPath(borderPen, borderPath);
        }

        var inner = RectangleF.Inflate(outer, -1.25f, -1.25f);
        if (inner.Width > 0 && inner.Height > 0)
        {
            using var innerPath = CreateRoundedPath(inner, Math.Max(3f, radius - 1.25f));
            using var innerPen = new Pen(Color.FromArgb(18, 255, 255, 255), 1f);
            graphics.DrawPath(innerPen, innerPath);
        }
    }

    private Color GetSurfaceColor()
    {
        if (!Enabled)
            return Color.FromArgb(42, 38, 42);

        if (DroppedDown)
            return SurfaceOpenColor;

        if (Focused || isHovering)
            return SurfaceHoverColor;

        return SurfaceColor;
    }

    private Color GetBorderColor()
    {
        if (!Enabled)
            return Color.FromArgb(70, 60, 54);

        return Focused || DroppedDown || isHovering
            ? BorderHoverColor
            : BorderColor;
    }

    private static GraphicsPath CreateRoundedPath(RectangleF bounds, float radius)
    {
        var path = new GraphicsPath();
        var safeRadius = Math.Min(radius, Math.Min(bounds.Width, bounds.Height) / 2f);
        if (safeRadius < 0.5f)
        {
            path.AddRectangle(bounds);
            return path;
        }

        var diameter = safeRadius * 2f;
        path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
}
