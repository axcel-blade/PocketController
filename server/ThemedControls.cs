using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace PocketControllerServer;

/// <summary>Panel with rounded corners, a fill and a 1px outline.</summary>
public sealed class RoundedPanel : Panel
{
    [Category("Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color FillColor { get; set; } = Theme.Surface;
    [Category("Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color BorderColor { get; set; } = Theme.Outline;
    [Category("Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public int CornerRadius { get; set; } = Theme.Radius;

    public RoundedPanel()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        BackColor = Theme.Background;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = Theme.RoundedRect(new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f), CornerRadius);
        using var fill = new SolidBrush(FillColor);
        using var pen  = new Pen(BorderColor, 1);
        e.Graphics.FillPath(fill, path);
        e.Graphics.DrawPath(pen, path);
    }
}

public enum PcButtonStyle { Primary, Danger, Ghost }

/// <summary>Flat, rounded button styled like the mobile app's buttons.</summary>
public sealed class PcButton : Button
{
    private bool _hover;
    private bool _down;
    private PcButtonStyle _style = PcButtonStyle.Primary;

    [Category("Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]

    public PcButtonStyle ButtonStyle
    {
        get => _style;
        set { _style = value; Invalidate(); }
    }

    /// <summary>Colour behind the button, used to paint outside the rounded corners.</summary>
    [Category("Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color SurroundColor { get; set; } = Theme.Surface;

    public PcButton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        Cursor = Cursors.Hand;
        Font = Theme.Ui(10f, FontStyle.Bold);
    }

    protected override void OnMouseEnter(EventArgs e) { _hover = true;  Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; _down = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { _down = true;  Invalidate(); base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e)   { _down = false; Invalidate(); base.OnMouseUp(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(SurroundColor);

        var (fill, border, text) = _style switch
        {
            PcButtonStyle.Primary => (_hover ? Theme.LimeHover : Theme.Lime, Theme.Lime, Theme.OnLime),
            PcButtonStyle.Danger  => (_hover ? Color.FromArgb(40, Theme.Danger) : SurroundColor, Theme.Danger, Theme.Danger),
            _                     => (_hover ? Theme.SurfaceRaised : SurroundColor, Theme.Outline, Theme.Text),
        };
        if (!Enabled) (fill, border, text) = (Theme.SurfaceRaised, Theme.Outline, Theme.TextDim);

        var inset = _down ? 1.5f : 0.5f; // tactile press
        var rect  = new RectangleF(inset, inset, Width - 1 - inset * 2, Height - 1 - inset * 2);
        using (var path  = Theme.RoundedRect(rect, Height / 2f))
        using (var brush = new SolidBrush(fill))
        using (var pen   = new Pen(border, 1.2f))
        {
            g.FillPath(brush, path);
            g.DrawPath(pen, path);
        }

        TextRenderer.DrawText(g, Text, Font, ClientRectangle, text,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);

        if (Focused && ShowFocusCues)
        {
            using var focus = new Pen(Theme.Cyan, 1.5f) { DashStyle = DashStyle.Dot };
            using var fp = Theme.RoundedRect(new RectangleF(3, 3, Width - 7, Height - 7), (Height - 6) / 2f);
            g.DrawPath(focus, fp);
        }
    }
}

/// <summary>A small filled circle used as a status indicator.</summary>
public sealed class StatusDot : Control
{
    private Color _color = Theme.TextDim;

    [Category("Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]

    public Color DotColor
    {
        get => _color;
        set { _color = value; Invalidate(); }
    }

    public StatusDot()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
        Size = new Size(10, 10);
        BackColor = Color.Transparent;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var b = new SolidBrush(_color);
        e.Graphics.FillEllipse(b, 0, 0, Width - 1, Height - 1);
    }
}
