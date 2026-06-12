using System.Net;
using PocketConsole.GamepadDriver;
using PocketConsole.NetworkLayer;
using PocketConsole.Protocol;

namespace PocketConsoleServer;

/// <summary>
/// Central coordinator that wires the network layer to the gamepad driver.
/// <see cref="MainForm"/> only calls <see cref="Start"/> and <see cref="Stop"/>;
/// all event routing happens here.
/// </summary>
public sealed class ServerController : IDisposable
{
    private readonly UdpServer _udp = new();
    private readonly ClientManager _clients = new();
    private readonly HeartbeatMonitor _heartbeat;
    private readonly VirtualGamepadManager _gamepads = new();
    private bool _disposed;

    /// <summary><c>true</c> while the UDP server is actively listening.</summary>
    public bool IsRunning => _udp.IsRunning;

    /// <summary>All currently connected client sessions.</summary>
    public IReadOnlyCollection<ClientSession> Sessions => _clients.Sessions;

    /// <summary>Raised on any thread when a log line is ready to display.</summary>
    public event Action<string>? OnLog;

    /// <summary>Raised when a new client session is registered.</summary>
    public event Action<ClientSession>? OnClientConnected;

    /// <summary>Raised when a client session is removed (disconnect or timeout).</summary>
    public event Action<ClientSession>? OnClientDisconnected;

    public ServerController()
    {
        _heartbeat = new HeartbeatMonitor(_clients);

        _udp.OnMessageReceived += HandleMessage;
        _udp.OnError           += ex => Log($"UDP error: {ex.Message}");

        _clients.OnClientConnected += session =>
        {
            _gamepads.AddController(session.Id);
            Log($"Client {session.Id} connected from {session.EndPoint}");
            OnClientConnected?.Invoke(session);
        };

        _clients.OnClientDisconnected += session =>
        {
            _gamepads.RemoveController(session.Id);
            Log($"Client {session.Id} disconnected");
            OnClientDisconnected?.Invoke(session);
        };
    }

    /// <summary>
    /// Initializes the ViGEmBus connection, starts the UDP listener, and begins heartbeat checks.
    /// </summary>
    /// <exception cref="Nefarius.ViGEm.Client.Exceptions.VigemBusNotFoundException">
    /// Thrown when the ViGEmBus driver is not installed. Caught and displayed by <see cref="MainForm"/>.
    /// </exception>
    public void Start(int port)
    {
        _gamepads.Initialize();
        _udp.Start(port);
        _heartbeat.Start();
        Log($"Server started on port {port}");
    }

    /// <summary>Stops listening, ends heartbeat checks, and removes all active sessions.</summary>
    public void Stop()
    {
        _heartbeat.Stop();
        _udp.Stop();

        // Copy to list first — Remove() modifies the Sessions collection mid-iteration.
        foreach (var s in _clients.Sessions.ToList())
            _clients.Remove(s);

        Log("Server stopped");
    }

    private void HandleMessage(IPEndPoint ep, GamepadMessage msg)
    {
        // Connect packets register new sessions; all others require an existing one.
        var session = msg.Type == MessageType.Connect
            ? _clients.GetOrAdd(ep)
            : _clients.GetByEndpoint(ep);

        if (session == null) return;

        session.Touch();

        switch (msg.Type)
        {
            case MessageType.Disconnect:
                _clients.Remove(session);
                break;
            case MessageType.Input:
                _gamepads.UpdateController(session.Id, msg);
                break;
            case MessageType.Ping:
                break; // Touch() above is sufficient — no further action needed.
        }
    }

    private void Log(string msg) => OnLog?.Invoke($"[{DateTime.Now:HH:mm:ss}] {msg}");

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Stop();
        _heartbeat.Dispose();
        _gamepads.Dispose();
        _udp.Dispose();
    }
}
