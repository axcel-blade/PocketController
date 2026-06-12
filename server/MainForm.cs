using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using PocketConsole.NetworkLayer;
using PocketConsole.Protocol;

namespace PocketConsoleServer;

public partial class MainForm : Form
{
    private readonly ServerController _server = new();
    private readonly AppSettings _settings = SettingsManager.Load();
    private TrayManager? _tray;

    public MainForm()
    {
        InitializeComponent();
        _server.OnLog += AppendLog;
        _server.OnClientConnected += _ => RefreshClientList();
        _server.OnClientDisconnected += _ => RefreshClientList();

        numPort.Value = _settings.Port;
        lblIp.Text = $"IP: {GetLocalIp()}";
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        _tray = new TrayManager(this);
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

    private void btnToggle_Click(object sender, EventArgs e)
    {
        if (_server.IsRunning)
        {
            _server.Stop();
            btnToggle.Text = "Start";
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
                btnToggle.Text = "Stop";
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

    private void AppendLog(string msg)
    {
        if (InvokeRequired) { Invoke(() => AppendLog(msg)); return; }
        rtbLog.AppendText(msg + Environment.NewLine);
        rtbLog.ScrollToCaret();
    }

    private void RefreshClientList()
    {
        if (InvokeRequired) { Invoke(RefreshClientList); return; }
        lstClients.Items.Clear();
        foreach (var s in _server.Sessions)
            lstClients.Items.Add($"#{s.Id}  {s.EndPoint}");
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
