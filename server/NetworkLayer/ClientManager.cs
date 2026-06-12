using System.Net;
using PocketConsole.Protocol;

namespace PocketConsole.NetworkLayer;

public class ClientManager
{
    private readonly Dictionary<IPEndPoint, ClientSession> _byEndpoint = new();
    private readonly Dictionary<int, ClientSession> _byId = new();
    private int _nextId = 1;

    public event Action<ClientSession>? OnClientConnected;
    public event Action<ClientSession>? OnClientDisconnected;

    public IReadOnlyCollection<ClientSession> Sessions => _byEndpoint.Values;

    public ClientSession? GetOrAdd(IPEndPoint ep)
    {
        if (_byEndpoint.TryGetValue(ep, out var session))
        {
            session.Touch();
            return session;
        }

        if (_byEndpoint.Count >= Constants.MaxClients)
            return null;

        session = new ClientSession(_nextId++, ep);
        _byEndpoint[ep] = session;
        _byId[session.Id] = session;
        OnClientConnected?.Invoke(session);
        return session;
    }

    public ClientSession? GetById(int id)
        => _byId.TryGetValue(id, out var s) ? s : null;

    public ClientSession? GetByEndpoint(IPEndPoint ep)
        => _byEndpoint.TryGetValue(ep, out var s) ? s : null;

    public void Remove(ClientSession session)
    {
        _byEndpoint.Remove(session.EndPoint);
        _byId.Remove(session.Id);
        OnClientDisconnected?.Invoke(session);
    }
}
