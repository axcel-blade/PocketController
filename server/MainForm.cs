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
            lstClients.ItemHeight = Math.Min(255, title.Height + sub.Height + RowGap + RowPadding * 2 + 6);

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

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
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

    // Positions inside the cards. WinForms has no flex layout, so this keeps
    // everything aligned as the window resizes.
    private void LayoutCards()
    {
        if (pnlConnection == null) return;
        const int pad = 18;

        // Header: logo, title with the version underneath, status pill on the right.
        // Sizes come from the controls themselves so this holds at any DPI scale.
        picLogo.Location    = new Point(0, (pnlHeader.Height - picLogo.Height) / 2);
        lblAppName.Location = new Point(picLogo.Right + 10, 8);
        lblVersion.Location = new Point(lblAppName.Left + 3, lblAppName.Bottom - 4);
        lblStatusText.Location = new Point(lblStatusDot.Right + 8, 0);
        pnlStatus.Size = new Size(lblStatusText.Right + 16, Math.Max(lblStatusText.Height + 14, 30));
        lblStatusText.Top  = (pnlStatus.Height - lblStatusText.Height) / 2;
        lblStatusDot.Location = new Point(14, (pnlStatus.Height - lblStatusDot.Height) / 2);
        lblStatusText.Left = lblStatusDot.Right + 8;
        pnlStatus.Location = new Point(pnlHeader.Width - pnlStatus.Width, (pnlHeader.Height - pnlStatus.Height) / 2);

        // Connection card: address | port | start button.
        int w = pnlConnection.Width;
        lblIpCaption.Location = new Point(pad, 14);
        lblIp.Location        = new Point(pad - 3, lblIpCaption.Bottom + 2);
        btnCopyIp.Location    = new Point(lblIp.Right + 8, lblIp.Top + (lblIp.Height - btnCopyIp.Height) / 2);

        pnlPortBox.Size     = new Size(numPort.Width + 20, numPort.Height + 14);
        numPort.Location    = new Point(10, (pnlPortBox.Height - numPort.Height) / 2);
        btnToggle.Location  = new Point(w - pad - btnToggle.Width, lblIp.Top + (lblIp.Height - btnToggle.Height) / 2);
        pnlPortBox.Location = new Point(btnToggle.Left - 20 - pnlPortBox.Width,
                                        lblIp.Top + (lblIp.Height - pnlPortBox.Height) / 2);
        lblPortCaption.Location = new Point(pnlPortBox.Left, lblIpCaption.Top);

        lblHint.MaximumSize = new Size(Math.Max(w - pad * 2, 100), 0);
        lblHint.Location    = new Point(pad, Math.Max(lblIp.Bottom, btnToggle.Bottom) + 8);
        pnlConnection.Height = lblHint.Bottom + 14;

        // Clients card.
        int cw = pnlClients.Width, ch = pnlClients.Height;
        lblClientCount.Location = new Point(cw - 16 - lblClientCount.Width, lblClientsHeader.Top);
        int listTop = lblClientsHeader.Bottom + 12;
        lstClients.SetBounds(10, listTop, cw - 20, Math.Max(ch - listTop - 12, 0));
        lblNoClients.SetBounds(10, listTop, cw - 20, Math.Max(ch - listTop - 12, 0));

        // Log card.
        int lw = pnlLog.Width, lh = pnlLog.Height;
        btnClearLog.Location = new Point(lw - 14 - btnClearLog.Width,
                                         lblLogHeader.Top + (lblLogHeader.Height - btnClearLog.Height) / 2);
        int logTop = Math.Max(lblLogHeader.Bottom, btnClearLog.Bottom) + 10;
        rtbLog.SetBounds(16, logTop, Math.Max(lw - 26, 0), Math.Max(lh - logTop - 12, 0));
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
            g.FillEllipse(dot, row.X + 13, row.Y + row.Height / 2 - 4, 8, 8);

        // Two lines, centred as a block, positioned from measured font heights
        // so they never overlap at higher Windows display scaling.
        using var titleFont = ClientTitleFont();
        using var subFont   = ClientSubFont();
        var name     = _server.GetDeviceName(s);
        var title    = name ?? $"Controller {s.Id}";
        var subtitle = name != null ? $"Controller {s.Id}  ·  {s.EndPoint.Address}" : s.EndPoint.Address.ToString();

        int textLeft  = (int)row.X + 30;
        int textWidth = (int)row.Right - textLeft - 10;
        int blockH    = titleFont.Height + RowGap + subFont.Height;
        int top       = (int)(row.Y + (row.Height - blockH) / 2);
        const TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine;

        TextRenderer.DrawText(g, title, titleFont,
            new Rectangle(textLeft, top, textWidth, titleFont.Height), Theme.Text, flags);
        TextRenderer.DrawText(g, subtitle, subFont,
            new Rectangle(textLeft, top + titleFont.Height + RowGap, textWidth, subFont.Height), Theme.TextDim, flags);
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
