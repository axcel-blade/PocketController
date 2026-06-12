namespace PocketConsoleServer;

public sealed class TrayManager : IDisposable
{
    private readonly NotifyIcon _trayIcon;
    private readonly Form _mainForm;

    public TrayManager(Form mainForm)
    {
        _mainForm = mainForm;

        var menu = new ContextMenuStrip();
        menu.Items.Add("Show", null, (_, _) => ShowForm());
        menu.Items.Add("Exit", null, (_, _) => Application.Exit());

        _trayIcon = new NotifyIcon
        {
            Text = "PocketConsole Server",
            Icon = SystemIcons.Application,
            ContextMenuStrip = menu,
            Visible = true
        };

        _trayIcon.DoubleClick += (_, _) => ShowForm();
    }

    private void ShowForm()
    {
        _mainForm.Show();
        _mainForm.WindowState = FormWindowState.Normal;
        _mainForm.Activate();
    }

    public void Dispose() => _trayIcon.Dispose();
}
