using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using PocketConsole.NetworkLayer;

namespace PocketConsoleServer;

public partial class MainForm : Form
{
    private readonly ServerController _server = new();
    private readonly AppSettings _settings = SettingsManager.Load();
    private TrayManager? _tray;

    private static readonly Color ColorAccent  = Color.FromArgb(99,  102, 241);
    private static readonly Color ColorGreen   = Color.FromArgb(34,  197, 94);
    private static readonly Color ColorRed     = Color.FromArgb(239, 68,  68);
    private static readonly Color ColorTextSec = Color.FromArgb(148, 163, 184);
    private static readonly Color ColorTextPri = Color.FromArgb(226, 232, 240);
    private static readonly Color ColorSurface = Color.FromArgb(24,  24,  37);

    public MainForm()
    {
        InitializeComponent();

        _server.OnLog += AppendLog;
        _server.OnClientConnected    += _ => RefreshClientList();
        _server.OnClientDisconnected += _ => RefreshClientList();

        numPort.Value = _settings.Port;
        lblIp.Text    = $"IP: {GetLocalIp()}";

        SetStatus(running: false);
        LayoutCards();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        _tray = new TrayManager(this);
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

    // Manually lay out the two side-by-side cards because WinForms
    // doesn't have a flex/grid container.
    private void LayoutCards()
    {
        var b = pnlCards.ClientRectangle;
        const int clientW = 220;
        const int gap     = 10;
        int logW = b.Width - clientW - gap;

        pnlClients.SetBounds(0, 0, clientW, b.Height);
        pnlLog.SetBounds(clientW + gap, 0, Math.Max(logW, 0), b.Height);

        // inner controls that anchor isn't handling perfectly
        lstClients.SetBounds(10, 56, clientW - 20, pnlClients.Height - 70);

        int logInnerW = pnlLog.ClientSize.Width;
        btnClearLog.Location = new Point(logInnerW - 70, 10);
        rtbLog.SetBounds(10, 44, logInnerW - 20, pnlLog.Height - 58);

        // status panel anchored top-right of header
        pnlStatus.Location = new Point(pnlHeader.Width - 170, 12);
    }

    private void btnToggle_Click(object sender, EventArgs e)
    {
        if (_server.IsRunning)
        {
            _server.Stop();
            SetStatus(running: false);
            numPort.Enabled = true;
        }
        else
        {
            var port = (int)numPort.Value;
            _settings.Port = port;
            SettingsManager.Save(_settings);
            try
            {
                _server.Start(port);
                SetStatus(running: true);
                numPort.Enabled = false;
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
        }
    }

    private void SetStatus(bool running)
    {
        if (running)
        {
            lblStatusDot.BackColor = ColorGreen;
            lblStatusText.Text     = "Running";
            lblStatusText.ForeColor = ColorGreen;
            btnToggle.Text         = "■  Stop";
            btnToggle.BackColor    = Color.FromArgb(185, 28, 28);
            btnToggle.FlatAppearance.MouseOverBackColor = Color.FromArgb(153, 27, 27);
        }
        else
        {
            lblStatusDot.BackColor = ColorRed;
            lblStatusText.Text     = "Stopped";
            lblStatusText.ForeColor = ColorTextSec;
            btnToggle.Text         = "▶  Start";
            btnToggle.BackColor    = ColorAccent;
            btnToggle.FlatAppearance.MouseOverBackColor = Color.FromArgb(79, 82, 221);
        }
        lblStatusDot.Invalidate();
    }

    private void AppendLog(string msg)
    {
        if (InvokeRequired) { Invoke(() => AppendLog(msg)); return; }

        // Colour-code by keyword
        Color color = msg.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                      msg.Contains("disconnect", StringComparison.OrdinalIgnoreCase)
            ? Color.FromArgb(252, 165, 165)   // red-300
            : msg.Contains("started", StringComparison.OrdinalIgnoreCase) ||
              msg.Contains("connected", StringComparison.OrdinalIgnoreCase)
            ? Color.FromArgb(134, 239, 172)   // green-300
            : ColorTextPri;

        rtbLog.SelectionStart  = rtbLog.TextLength;
        rtbLog.SelectionLength = 0;
        rtbLog.SelectionColor  = color;
        rtbLog.AppendText(msg + "\n");
        rtbLog.ScrollToCaret();
    }

    private void RefreshClientList()
    {
        if (InvokeRequired) { Invoke(RefreshClientList); return; }
        lstClients.Items.Clear();
        foreach (var s in _server.Sessions)
            lstClients.Items.Add($"{s.EndPoint}  #{s.Id}");
        int count = _server.Sessions.Count;
        lblClientCount.Text = $"{count} / 4 connected";
        lblClientCount.ForeColor = count > 0 ? ColorGreen : ColorTextSec;
    }

    private static string GetLocalIp()
    {
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up) continue;
            if (ni.NetworkInterfaceType is NetworkInterfaceType.Loopback) continue;
            foreach (var addr in ni.GetIPProperties().UnicastAddresses)
            {
                if (addr.Address.AddressFamily == AddressFamily.InterNetwork)
                    return addr.Address.ToString();
            }
        }
        return "Unknown";
    }
}
