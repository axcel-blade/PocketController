using System.Net;
using PocketController.GamepadDriver;
using PocketController.NetworkLayer;
using PocketController.Protocol;

namespace PocketControllerServer;

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
    private readonly DiscoveryService _discovery;
    private readonly Dictionary<IPAddress, DateTime> _lastProbeLog = new();
    private int _port;
    private bool _disposed;

    /// <summary><c>true</c> while the UDP server is actively listening.</summary>
    public bool IsRunning => _udp.IsRunning;

    /// <summary><c>true</c> while phones can find this server automatically.</summary>
    public bool IsDiscoverable => _discovery.IsRunning;

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

        _discovery = new DiscoveryService(() => new DiscoveryInfo(
            Environment.MachineName, _port, _clients.Sessions.Count, Constants.MaxClients));
        _discovery.OnProbe += LogProbe;
        _discovery.OnError += ex => Log($"Discovery error: {ex.Message}");

        _udp.OnMessageReceived += HandleMessage;
        _udp.OnError           += ex => Log($"UDP error: {ex.Message}");

        _clients.OnClientConnected += session =>
        {
            bool created;
            try
            {
                created = _gamepads.AddController(session.Id);
            }
            catch (Exception ex)
            {
                Log($"Could not create a virtual controller for client {session.Id}: {ex.Message}");
                created = false;
            }

            if (!created)
            {
                // Drop the session so the phone is told the server is full instead of "connected".
                _clients.Remove(session);
                return;
            }

            Log($"Client {session.Id} connected from {session.EndPoint}");
            OnClientConnected?.Invoke(session);
        };

        _clients.OnClientDisconnected += session =>
        {
            if (!_gamepads.Controllers.ContainsKey(session.Id)) return; // never fully connected
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
        _port = port;
        _udp.Start(port);
        _heartbeat.Start();
        Log($"Server started on port {port}");

        // Discovery is a convenience: if its port is taken, phones can still connect by IP.
        try
        {
            _discovery.Start();
            Log($"Discoverable on the local network (UDP {Constants.DiscoveryPort}/{Constants.AnnouncePort})");
        }
        catch (System.Net.Sockets.SocketException ex)
        {
            Log($"Auto-discovery unavailable ({ex.Message}). Phones can still connect by entering the IP.");
        }
    }

    /// <summary>Stops listening, ends heartbeat checks, and removes all active sessions.</summary>
    public void Stop()
    {
        _discovery.Stop();
        _heartbeat.Stop();
        _udp.Stop();

        // Sessions is a snapshot, so removing while iterating is safe.
        foreach (var s in _clients.Sessions)
            _clients.Remove(s);

        Log("Server stopped");
    }

    private void HandleMessage(IPEndPoint ep, GamepadMessage msg)
    {
        // Connect packets register new sessions; all others require an existing one.
        var session = msg.Type == MessageType.Connect
            ? _clients.GetOrAdd(ep)
            : _clients.GetByEndpoint(ep);

        // GetOrAdd removes the session again if its virtual controller could not be created.
        if (session != null && _clients.GetById(session.Id) == null)
            session = null;

        if (session == null)
        {
            // Tell the client why it has no session so the app never shows a false "connected" state.
            if (msg.Type == MessageType.Connect)
                Reply(ep, MessageType.ServerFull, msg.TimestampMs);
            else if (msg.Type == MessageType.Ping)
                Reply(ep, MessageType.NotConnected, msg.TimestampMs);
            return;
        }

        session.Touch();

        switch (msg.Type)
        {
            case MessageType.Connect:
                Reply(ep, MessageType.ConnectAck, msg.TimestampMs);
                break;
            case MessageType.Disconnect:
                _clients.Remove(session);
                break;
            case MessageType.Input:
                _gamepads.UpdateController(session.Id, msg);
                break;
            case MessageType.Ping:
                Reply(ep, MessageType.Pong, msg.TimestampMs);
                break;
        }
    }

    /// <summary>Sends a control reply, echoing the client's timestamp so it can measure round-trip latency.</summary>
    private void Reply(IPEndPoint ep, MessageType type, long echoTimestampMs)
        => _udp.Send(ep, new GamepadMessage { Type = type, TimestampMs = echoTimestampMs });

    // Phones probe every couple of seconds while searching; log each phone at most every 30 s.
    private void LogProbe(IPEndPoint ep, string device)
    {
        lock (_lastProbeLog)
        {
            if (_lastProbeLog.TryGetValue(ep.Address, out var last) && DateTime.UtcNow - last < TimeSpan.FromSeconds(30))
                return;
            _lastProbeLog[ep.Address] = DateTime.UtcNow;
        }
        if (_clients.Sessions.Any(s => s.EndPoint.Address.Equals(ep.Address))) return;
        Log($"Phone \"{device}\" at {ep.Address} is looking for servers");
    }

    private void Log(string msg) => OnLog?.Invoke($"[{DateTime.Now:HH:mm:ss}] {msg}");

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Stop();
        _heartbeat.Dispose();
        _discovery.Dispose();
        _gamepads.Dispose();
        _udp.Dispose();
    }
}
