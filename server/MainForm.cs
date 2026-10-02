using System.Net.NetworkInformation;
using System.Net.Sockets;
using PocketController.NetworkLayer;

namespace PocketControllerServer;

public partial class MainForm : Form
{
    private readonly ServerController _server = new();
    private readonly AppSettings _settings = SettingsManager.Load();
    private readonly Icon _appIcon = Theme.CreateAppIcon();
    private TrayManager? _tray;

    public MainForm()
    {
        InitializeComponent();

        Icon = _appIcon;
        picLogo.Image = _appIcon.ToBitmap();

        // The designer stores "Cascadia Mono"; swap in the best monospace font this PC has.
        lblIp.Font   = Theme.Mono(lblIp.Font.Size, FontStyle.Bold);
        numPort.Font = Theme.Mono(numPort.Font.Size);
        rtbLog.Font  = Theme.Mono(rtbLog.Font.Size);

        _server.OnLog += AppendLog;
        _server.OnClientConnected    += _ => RefreshClientList();
        _server.OnClientDisconnected += _ => RefreshClientList();

        numPort.Value = Math.Clamp(_settings.Port, (int)numPort.Minimum, (int)numPort.Maximum);
        lblIp.Text    = GetLocalIp();

        using (var title = ClientTitleFont())
        using (var sub = ClientSubFont())
            lstClients.ItemHeight = Math.Min(255, title.Height + sub.Height + S(RowGap) + S(RowPadding) * 2 + 6);

        pnlClients.Resize += (_, _) => LayoutCardContents();
        pnlLog.Resize     += (_, _) => LayoutCardContents();

        SetStatus(running: false);
        RefreshClientList();
        LayoutCards();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        // Dark title bar to match the window (Windows 10 2004+ / Windows 11; ignored elsewhere).
        int on = 1;
        _ = NativeMethods.DwmSetWindowAttribute(Handle, 20, ref on, sizeof(int));
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        _tray = new TrayManager(this, _appIcon);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        LayoutCards();
    }

    protected override void OnDpiChanged(DpiChangedEventArgs e)
    {
        base.OnDpiChanged(e);
        LayoutCards();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Normally X hides to the tray so the server keeps running. Under the Visual Studio
        // debugger, really exit instead: a hidden instance would lock bin\ and break the next build.
        if (e.CloseReason == CloseReason.UserClosing && !System.Diagnostics.Debugger.IsAttached)
        {
            e.Cancel = true;
            Hide();
            return;
        }
        _server.Stop();
        _server.Dispose();
        _tray?.Dispose();
        base.OnFormClosing(e);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        _appIcon.Dispose();
    }

    // ── Layout ────────────────────────────────────────────────────────
    // WinForms has no flex/grid for these custom cards, so everything is placed here on one
    // spacing grid. All spacing goes through S() so it scales with Windows display settings
    // the same way the fonts do; controls are sized/centred from their measured sizes.

    /// <summary>Scales a 96-DPI design value to the current display.</summary>
    private int S(int value) => (int)Math.Round(value * DeviceDpi / 96f);

    private void LayoutCards()
    {
        if (pnlConnection == null) return;

        int pad     = S(18);   // inner padding of every card
        int gap     = S(12);   // gap between cards / sections
        int rowH    = S(44);   // height of the main control row (port box, Start)
        int smallH  = S(30);   // height of small buttons (Copy, Clear)

        Padding           = new Padding(S(20), S(6), S(20), 0);
        pnlSpacer.Height  = gap;
        pnlFooter.Height  = S(34);
        tblCards.ColumnStyles[0].Width = S(260);
        pnlClients.Margin = new Padding(0, 0, gap / 2, 0);
        pnlLog.Margin     = new Padding(gap / 2, 0, 0, 0);

        LayoutHeader();
        LayoutConnectionCard(pad, rowH, smallH);
        LayoutCardContents();
    }

    // Also runs when the cards resize, since their final size is only known after the
    // header and connection card above them have been laid out.
    private void LayoutCardContents()
    {
        int pad = S(18);
        LayoutCardHeader(pnlClients, lblClientsHeader, lblClientCount, pad);
        LayoutCardHeader(pnlLog, lblLogHeader, btnClearLog, pad, S(30));
        LayoutCardBodies(pad);
    }

    // Logo, title block and status pill share one vertical centre line.
    private void LayoutHeader()
    {
        int logo = S(40);
        picLogo.Size = new Size(logo, logo);

        // Status pill: [pad] dot [gap] text [pad]
        lblStatusDot.Size = new Size(S(10), S(10));
        int pillH = Math.Max(lblStatusText.Height + S(12), S(32));
        lblStatusDot.Location  = new Point(S(14), (pillH - lblStatusDot.Height) / 2);
        lblStatusText.Location = new Point(lblStatusDot.Right + S(8), (pillH - lblStatusText.Height) / 2);
        pnlStatus.Size         = new Size(lblStatusText.Right + S(16), pillH);
        pnlStatus.CornerRadius = pillH / 2;

        int titleBlockH = lblAppName.Height + lblVersion.Height - S(4);
        pnlHeader.Height = Math.Max(Math.Max(logo, titleBlockH), pillH) + S(24);
        int mid = pnlHeader.Height / 2;

        picLogo.Location    = new Point(0, mid - logo / 2);
        lblAppName.Location = new Point(picLogo.Right + S(12), mid - titleBlockH / 2);
        lblVersion.Location = new Point(lblAppName.Left + S(2), lblAppName.Bottom - S(4));
        pnlStatus.Location  = new Point(pnlHeader.Width - pnlStatus.Width, mid - pillH / 2);
    }

    // Captions on one line; IP, Copy, port box and Start all centred on one control row.
    private void LayoutConnectionCard(int pad, int rowH, int smallH)
    {
        int w = pnlConnection.Width;

        btnToggle.Size  = new Size(S(140), rowH);
        btnCopyIp.Size  = new Size(TextRenderer.MeasureText("Copied", btnCopyIp.Font).Width + S(28), smallH);
        pnlPortBox.Size = new Size(numPort.Width + S(24), rowH);
        pnlPortBox.CornerRadius = S(10);
        numPort.Location = new Point(S(12), (rowH - numPort.Height) / 2);

        // Caption line
        lblIpCaption.Location = new Point(pad, pad);
        int rowTop = lblIpCaption.Bottom + S(6);
        int rowMid = rowTop + Math.Max(rowH, lblIp.Height) / 2;

        // Right side: [port box] [gap] [Start]
        btnToggle.Location      = new Point(w - pad - btnToggle.Width, rowMid - btnToggle.Height / 2);
        pnlPortBox.Location     = new Point(btnToggle.Left - S(16) - pnlPortBox.Width, rowMid - rowH / 2);
        lblPortCaption.Location = new Point(pnlPortBox.Left, lblIpCaption.Top);

        // Left side: IP then Copy. Large text has a little built-in left bearing; nudge it
        // so its glyphs line up with the caption above.
        lblIp.Location     = new Point(pad - S(2), rowMid - lblIp.Height / 2);
        btnCopyIp.Location = new Point(lblIp.Right + S(10), rowMid - btnCopyIp.Height / 2);

        int rowBottom = rowMid + Math.Max(rowH, lblIp.Height) / 2;
        lblHint.MaximumSize = new Size(Math.Max(w - pad * 2, S(100)), 0);
        lblHint.Location    = new Point(pad, rowBottom + S(12));
        pnlConnection.Height = lblHint.Bottom + pad;
    }

    // Card title on the left, an action/count on the right, both on the same centre line.
    private void LayoutCardHeader(Control card, Label title, Control right, int pad, int? rightHeight = null)
    {
        if (rightHeight is int h) right.Height = h;
        if (right is PcButton b) b.Width = TextRenderer.MeasureText(b.Text, b.Font).Width + S(28);

        // Same header height in every card (room for a small button) so titles and the
        // content below them line up across cards, whether or not a card has a button.
        int headerH = Math.Max(Math.Max(title.Height, right.Height), S(30));
        int mid     = pad + headerH / 2;
        title.Location = new Point(pad, mid - title.Height / 2);
        right.Location = new Point(card.Width - pad - right.Width, mid - right.Height / 2);
        card.Tag = pad + headerH; // bottom of the header row, used by LayoutCardBodies
    }

    private void LayoutCardBodies(int pad)
    {
        int gap = S(12);

        // Clients: list rows draw their own 2px inset, so start 2px further out to keep
        // the row edges aligned with the card title.
        int cw = pnlClients.Width, ch = pnlClients.Height;
        int listTop = (pnlClients.Tag as int? ?? pad) + gap;
        var listBounds = new Rectangle(pad - 2, listTop, Math.Max(cw - pad * 2 + 4, 0), Math.Max(ch - listTop - pad, 0));
        lstClients.Bounds   = listBounds;
        lblNoClients.Bounds = listBounds;

        // Log: text starts on the same left edge as the title.
        int lw = pnlLog.Width, lh = pnlLog.Height;
        int logTop = (pnlLog.Tag as int? ?? pad) + gap;
        rtbLog.SetBounds(pad, logTop, Math.Max(lw - pad * 2 + S(6), 0), Math.Max(lh - logTop - pad, 0));
    }

    private void btnToggle_Click(object? sender, EventArgs e)
    {
        if (_server.IsRunning)
        {
            _server.Stop();
            SetStatus(running: false);
            return;
        }

        var port = (int)numPort.Value;
        _settings.Port = port;
        SettingsManager.Save(_settings);
        try
        {
            _server.Start(port);
            SetStatus(running: true);
        }
        catch (Exception ex) when (ex.GetType().Name == "VigemBusNotFoundException")
        {
            MessageBox.Show(
                "ViGEmBus driver not found.\n\n" +
                "Please install it from:\nhttps://github.com/nefarius/ViGEmBus/releases\n\n" +
                "Restart the app after installing.",
                "Driver Required",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        catch (SocketException ex)
        {
            _server.Stop();
            AppendLog($"[{DateTime.Now:HH:mm:ss}] Error: could not open UDP port {port} — {ex.Message}");
            MessageBox.Show(
                $"Could not listen on UDP port {port}.\n\n{ex.Message}\n\nTry a different port.",
                "Port Unavailable",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void btnCopyIp_Click(object? sender, EventArgs e)
    {
        try
        {
            Clipboard.SetText(lblIp.Text);
            btnCopyIp.Text = "Copied";
            var t = new System.Windows.Forms.Timer { Interval = 1500 };
            t.Tick += (_, _) => { btnCopyIp.Text = "Copy"; t.Dispose(); };
            t.Start();
        }
        catch (System.Runtime.InteropServices.ExternalException)
        {
            // Clipboard busy in another app; nothing to do.
        }
    }

    private void btnClearLog_Click(object? sender, EventArgs e) => rtbLog.Clear();

    private void LstClients_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0 || lstClients.Items[e.Index] is not ClientSession s) return;
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        using (var bg = new SolidBrush(Theme.Surface))
            g.FillRectangle(bg, e.Bounds);

        var row = new RectangleF(e.Bounds.X + 2, e.Bounds.Y + 3, e.Bounds.Width - 5, e.Bounds.Height - 7);
        bool selected = (e.State & DrawItemState.Selected) != 0;
        using (var path = Theme.RoundedRect(row, 10))
        using (var fill = new SolidBrush(Theme.SurfaceRaised))
        using (var pen  = new Pen(selected ? Theme.Lime : Theme.Outline))
        {
            g.FillPath(fill, path);
            g.DrawPath(pen, path);
        }

        using (var dot = new SolidBrush(Theme.Lime))
            g.FillEllipse(dot, row.X + S(12), row.Y + row.Height / 2 - S(4), S(8), S(8));

        // Two lines, centred as a block, positioned from measured font heights
        // so they never overlap at higher Windows display scaling.
        using var titleFont = ClientTitleFont();
        using var subFont   = ClientSubFont();
        var name     = _server.GetDeviceName(s);
        var title    = name ?? $"Controller {s.Id}";
        var subtitle = name != null ? $"Controller {s.Id}  ·  {s.EndPoint.Address}" : s.EndPoint.Address.ToString();

        int textLeft  = (int)row.X + S(30);
        int textWidth = (int)row.Right - textLeft - 10;
        int blockH    = titleFont.Height + S(RowGap) + subFont.Height;
        int top       = (int)(row.Y + (row.Height - blockH) / 2);
        const TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine;

        TextRenderer.DrawText(g, title, titleFont,
            new Rectangle(textLeft, top, textWidth, titleFont.Height), Theme.Text, flags);
        TextRenderer.DrawText(g, subtitle, subFont,
            new Rectangle(textLeft, top + titleFont.Height + S(RowGap), textWidth, subFont.Height), Theme.TextDim, flags);
    }

    private const int RowGap = 4;
    private const int RowPadding = 8;
    private static Font ClientTitleFont() => Theme.Ui(9.5f, FontStyle.Bold);
    private static Font ClientSubFont()   => Theme.Mono(8.5f);

    private void SetStatus(bool running)
    {
        lblStatusDot.DotColor   = running ? Theme.Lime : Theme.TextDim;
        lblStatusText.Text      = running ? "Running" : "Stopped";
        lblStatusText.ForeColor = running ? Theme.Lime : Theme.TextDim;
        pnlStatus.BorderColor   = running ? Color.FromArgb(120, Theme.Lime) : Theme.Outline;
        pnlStatus.Invalidate();

        btnToggle.Text           = running ? "■  Stop" : "▶  Start";
        btnToggle.ButtonStyle    = running ? PcButtonStyle.Danger : PcButtonStyle.Primary;
        btnToggle.AccessibleName = running ? "Stop server" : "Start server";
        numPort.Enabled          = !running;
    }

    private void AppendLog(string msg)
    {
        if (IsDisposed || !IsHandleCreated) return;
        if (InvokeRequired) { BeginInvoke(() => AppendLog(msg)); return; }

        // Colour-code by keyword, using the app palette.
        Color color = msg.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                      msg.Contains("could not", StringComparison.OrdinalIgnoreCase)
            ? Theme.Danger
            : msg.Contains("disconnect", StringComparison.OrdinalIgnoreCase) ||
              msg.Contains("stopped", StringComparison.OrdinalIgnoreCase)
            ? Theme.Warning
            : msg.Contains("looking for", StringComparison.OrdinalIgnoreCase) ||
          msg.Contains("discoverable", StringComparison.OrdinalIgnoreCase)
            ? Theme.Cyan
            : msg.Contains("started", StringComparison.OrdinalIgnoreCase) ||
              msg.Contains("connected", StringComparison.OrdinalIgnoreCase)
            ? Theme.Lime
            : Theme.Text;

        rtbLog.SelectionStart  = rtbLog.TextLength;
        rtbLog.SelectionLength = 0;
        rtbLog.SelectionColor  = color;
        rtbLog.AppendText(msg + "\n");
        rtbLog.ScrollToCaret();
    }

    private void RefreshClientList()
    {
        if (IsDisposed || !IsHandleCreated && InvokeRequired) return;
        if (InvokeRequired) { BeginInvoke(RefreshClientList); return; }

        var sessions = _server.Sessions; // one snapshot so the list and the count agree
        lstClients.BeginUpdate();
        lstClients.Items.Clear();
        foreach (var s in sessions.OrderBy(s => s.Id))
            lstClients.Items.Add(s);
        lstClients.EndUpdate();

        int count = sessions.Count;
        lblClientCount.Text      = $"{count} / {PocketController.Protocol.Constants.MaxClients}";
        lblClientCount.ForeColor = count > 0 ? Theme.Lime : Theme.TextDim;
        lblNoClients.Visible     = count == 0;
        lstClients.Visible       = count > 0;
        LayoutCards();
    }

    private static class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);
    }

    private static string GetLocalIp()
    {
        // Prefer an interface with a default gateway (the real LAN adapter, not a VPN/virtual switch).
        string? fallback = null;
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up) continue;
            if (ni.NetworkInterfaceType is NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel) continue;
            var props = ni.GetIPProperties();
            foreach (var addr in props.UnicastAddresses)
            {
                if (addr.Address.AddressFamily != AddressFamily.InterNetwork) continue;
                if (props.GatewayAddresses.Any(g => g.Address.AddressFamily == AddressFamily.InterNetwork))
                    return addr.Address.ToString();
                fallback ??= addr.Address.ToString();
            }
        }
        return fallback ?? "Unknown";
    }
}
