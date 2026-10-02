using System.Net;
using PocketController.Protocol;

namespace PocketController.NetworkLayer;

/// <summary>
/// Tracks active client sessions and enforces the <see cref="Constants.MaxClients"/> limit.
/// All lookups are O(1) via dual dictionaries keyed by endpoint and by ID.
/// Thread-safe: it is used from the UDP receive loop, the heartbeat timer and the UI thread.
/// Events are raised outside the lock.
/// </summary>
public class ClientManager
{
    // Two dictionaries so both endpoint→session and id→session lookups are O(1).
    private readonly Dictionary<IPEndPoint, ClientSession> _byEndpoint = new();
    private readonly Dictionary<int, ClientSession> _byId = new();
    private readonly object _lock = new();
    private int _nextId = 1;

    /// <summary>Raised on the thread that called <see cref="GetOrAdd"/> when a new client is registered.</summary>
    public event Action<ClientSession>? OnClientConnected;

    /// <summary>Raised on the thread that called <see cref="Remove"/> when a client is removed.</summary>
    public event Action<ClientSession>? OnClientDisconnected;

    /// <summary>A snapshot of the currently active sessions. Safe to enumerate while sessions change.</summary>
    public IReadOnlyCollection<ClientSession> Sessions
    {
        get { lock (_lock) return _byEndpoint.Values.ToList(); }
    }

    /// <summary>
    /// Returns the existing session for <paramref name="ep"/> (refreshing its timestamp),
    /// or registers a new session if the client limit has not been reached.
    /// Returns <c>null</c> when the server is full.
    /// </summary>
    public ClientSession? GetOrAdd(IPEndPoint ep)
    {
        ClientSession session;
        lock (_lock)
        {
            if (_byEndpoint.TryGetValue(ep, out var existing))
            {
                existing.Touch();
                return existing;
            }

            if (_byEndpoint.Count >= Constants.MaxClients)
                return null;

            session = new ClientSession(_nextId++, ep);
            _byEndpoint[ep] = session;
            _byId[session.Id] = session;
        }
        OnClientConnected?.Invoke(session);
        return session;
    }

    /// <summary>Returns the session with the given <paramref name="id"/>, or <c>null</c> if not found.</summary>
    public ClientSession? GetById(int id)
    {
        lock (_lock) return _byId.TryGetValue(id, out var s) ? s : null;
    }

    /// <summary>Returns the session for the given <paramref name="ep"/>, or <c>null</c> if not found.</summary>
    public ClientSession? GetByEndpoint(IPEndPoint ep)
    {
        lock (_lock) return _byEndpoint.TryGetValue(ep, out var s) ? s : null;
    }

    /// <summary>
    /// Removes <paramref name="session"/> from both indexes and raises <see cref="OnClientDisconnected"/>.
    /// Safe to call even if the session is already absent; the event is raised only once,
    /// even if a Disconnect packet and a heartbeat timeout race to remove the same session.
    /// </summary>
    public void Remove(ClientSession session)
    {
        lock (_lock)
        {
            if (!_byId.Remove(session.Id)) return;
            _byEndpoint.Remove(session.EndPoint);
        }
        OnClientDisconnected?.Invoke(session);
    }
}
