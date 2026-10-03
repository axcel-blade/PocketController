using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace PocketControllerServer;

/// <summary>
/// Colours and fonts shared by the server window. Matches the mobile app
/// (app/lib/theme.dart): dark graphite surfaces, lime accent, restrained cyan detail.
/// </summary>
internal static class Theme
{
    public static readonly Color Background    = Color.FromArgb(0x12, 0x14, 0x17);
    public static readonly Color Surface       = Color.FromArgb(0x1B, 0x1E, 0x23);
    public static readonly Color SurfaceRaised = Color.FromArgb(0x25, 0x29, 0x30);
    public static readonly Color Outline       = Color.FromArgb(0x36, 0x3B, 0x44);
    public static readonly Color Lime          = Color.FromArgb(0xC4, 0xF8, 0x2A);
    public static readonly Color LimeHover     = Color.FromArgb(0xD4, 0xFF, 0x5A);
    public static readonly Color OnLime        = Color.FromArgb(0x15, 0x19, 0x0A);
    public static readonly Color Cyan          = Color.FromArgb(0x5C, 0xD6, 0xE6);
    public static readonly Color Text          = Color.FromArgb(0xE8, 0xEB, 0xEF);
    public static readonly Color TextDim       = Color.FromArgb(0x8E, 0x96, 0xA3);
    public static readonly Color Danger        = Color.FromArgb(0xFF, 0x6B, 0x6B);
    public static readonly Color Warning       = Color.FromArgb(0xFF, 0xC8, 0x57);

    public const int Radius = 14;

    public static Font Ui(float size, FontStyle style = FontStyle.Regular) => new("Segoe UI", size, style);

    /// <summary>Monospace font for addresses and the log, with fallbacks for older Windows installs.</summary>
    public static Font Mono(float size, FontStyle style = FontStyle.Regular)
    {
        using var installed = new InstalledFontCollection();
        foreach (var name in new[] { "Cascadia Mono", "Cascadia Code", "Consolas" })
        {
            if (installed.Families.Any(f => f.Name == name))
                return new Font(name, size, style);
        }
        return new Font(FontFamily.GenericMonospace, size, style);
    }

    public static GraphicsPath RoundedRect(RectangleF r, float radius)
    {
        var d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
        var path = new GraphicsPath();
        if (d <= 0)
        {
            path.AddRectangle(r);
            return path;
        }
        path.AddArc(r.X, r.Y, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    /// <summary>App icon loaded from the embedded Assets/app.ico.</summary>
    public static Icon CreateAppIcon()
    {
        using var stream = typeof(Theme).Assembly.GetManifestResourceStream("PocketController.app.ico")
            ?? throw new InvalidOperationException("Embedded app icon not found.");
        return new Icon(stream, 256, 256);
    }
}
