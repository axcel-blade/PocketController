using PocketConsole.Protocol;

namespace PocketConsole.NetworkLayer;

/// <summary>
/// Periodically checks all active sessions and removes any that have not sent
/// a packet within <see cref="Constants.ClientTimeoutMs"/> milliseconds.
/// Uses a <see cref="System.Threading.Timer"/> so it runs independently of the receive loop.
/// </summary>
public class HeartbeatMonitor : IDisposable
{
    private readonly ClientManager _clients;
    private System.Threading.Timer? _timer;

    /// <param name="clients">The session store to inspect and cull.</param>
    public HeartbeatMonitor(ClientManager clients) => _clients = clients;

    /// <summary>Starts the periodic check. Safe to call multiple times — subsequent calls are no-ops.</summary>
    public void Start()
    {
        _timer = new System.Threading.Timer(
            _ => CheckClients(),
            null,
            Constants.HeartbeatIntervalMs,
            Constants.HeartbeatIntervalMs);
    }

    /// <summary>Pauses the timer without disposing it so <see cref="Start"/> can be called again.</summary>
    public void Stop() => _timer?.Change(Timeout.Infinite, Timeout.Infinite);

    private void CheckClients()
    {
        var cutoff = DateTime.UtcNow.AddMilliseconds(-Constants.ClientTimeoutMs);

        // Snapshot to a list before iterating — Remove() modifies the Sessions collection.
        var dead = _clients.Sessions.Where(s => s.LastSeen < cutoff).ToList();
        foreach (var session in dead)
            _clients.Remove(session);
    }

    /// <inheritdoc/>
    public void Dispose() => _timer?.Dispose();
}
