using System.Net;

namespace PocketController.NetworkLayer;

/// <summary>
/// Represents a single connected Android client.
/// Holds the network endpoint, server-assigned ID, and the last time a packet was received.
/// </summary>
public class ClientSession
{
    /// <summary>Unique ID assigned by <see cref="ClientManager"/> at connect time. Stable for the session lifetime.</summary>
    public int Id { get; }

    /// <summary>The client's remote IP address and UDP port.</summary>
    public IPEndPoint EndPoint { get; }

    /// <summary>UTC timestamp of the most recently received packet. Used by <see cref="HeartbeatMonitor"/> to detect timeouts.</summary>
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;

    /// <summary>Creates a new session with the given <paramref name="id"/> and <paramref name="endPoint"/>.</summary>
    public ClientSession(int id, IPEndPoint endPoint)
    {
        Id       = id;
        EndPoint = endPoint;
    }

    /// <summary>Updates <see cref="LastSeen"/> to the current UTC time. Called on every received packet.</summary>
    public void Touch() => LastSeen = DateTime.UtcNow;
}
