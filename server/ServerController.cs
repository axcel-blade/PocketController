using System.Net;
using PocketConsole.GamepadDriver;
using PocketConsole.NetworkLayer;
using PocketConsole.Protocol;

namespace PocketConsoleServer;

public sealed class ServerController : IDisposable
{
    private readonly UdpServer _udp = new();
    private readonly ClientManager _clients = new();
    private readonly HeartbeatMonitor _heartbeat;
    private readonly VirtualGamepadManager _gamepads = new();
    private bool _disposed;

    public bool IsRunning => _udp.IsRunning;
    public IReadOnlyCollection<ClientSession> Sessions => _clients.Sessions;

    public event Action<string>? OnLog;
    public event Action<ClientSession>? OnClientConnected;
    public event Action<ClientSession>? OnClientDisconnected;

    public ServerController()
    {
        _heartbeat = new HeartbeatMonitor(_clients);

        _udp.OnMessageReceived += HandleMessage;
        _udp.OnError += ex => Log($"UDP error: {ex.Message}");

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

    public void Start(int port)
    {
        _udp.Start(port);
        _heartbeat.Start();
        Log($"Server started on port {port}");
    }

    public void Stop()
    {
        _heartbeat.Stop();
        _udp.Stop();
        foreach (var s in _clients.Sessions.ToList())
            _clients.Remove(s);
        Log("Server stopped");
    }

    private void HandleMessage(IPEndPoint ep, GamepadMessage msg)
    {
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
                break;
        }
    }

    private void Log(string msg) => OnLog?.Invoke($"[{DateTime.Now:HH:mm:ss}] {msg}");

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
