using PocketConsole.Protocol;

namespace PocketConsole.NetworkLayer;

public class HeartbeatMonitor : IDisposable
{
    private readonly ClientManager _clients;
    private System.Threading.Timer? _timer;

    public HeartbeatMonitor(ClientManager clients) => _clients = clients;

    public void Start()
    {
        _timer = new System.Threading.Timer(
            _ => CheckClients(),
            null,
            Constants.HeartbeatIntervalMs,
            Constants.HeartbeatIntervalMs);
    }

    public void Stop() => _timer?.Change(Timeout.Infinite, Timeout.Infinite);

    private void CheckClients()
    {
        var cutoff = DateTime.UtcNow.AddMilliseconds(-Constants.ClientTimeoutMs);
        var dead = _clients.Sessions.Where(s => s.LastSeen < cutoff).ToList();
        foreach (var session in dead)
            _clients.Remove(session);
    }

    public void Dispose() => _timer?.Dispose();
}
