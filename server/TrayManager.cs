namespace PocketConsoleServer;

/// <summary>
/// Manages the Windows system tray icon for PocketConsole Server.
/// Closing the main window hides it rather than exiting; the tray icon
/// lets the user show the window again or exit the process entirely.
/// </summary>
public sealed class TrayManager : IDisposable
{
    private readonly NotifyIcon _trayIcon;
    private readonly Form _mainForm;

    /// <param name="mainForm">The form to show/hide via the tray menu.</param>
    public TrayManager(Form mainForm)
    {
        _mainForm = mainForm;

        var menu = new ContextMenuStrip();
        menu.Items.Add("Show", null, (_, _) => ShowForm());
        menu.Items.Add("Exit", null, (_, _) => Application.Exit());

        _trayIcon = new NotifyIcon
        {
            Text             = "PocketConsole Server",
            Icon             = SystemIcons.Application,
            ContextMenuStrip = menu,
            Visible          = true
        };

        _trayIcon.DoubleClick += (_, _) => ShowForm();
    }

    private void ShowForm()
    {
        _mainForm.Show();
        _mainForm.WindowState = FormWindowState.Normal;
        _mainForm.Activate();
    }

    /// <inheritdoc/>
    public void Dispose() => _trayIcon.Dispose();
}
