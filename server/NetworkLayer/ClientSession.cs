using System.Net;

namespace PocketConsole.NetworkLayer;

public class ClientSession
{
    public int Id { get; }
    public IPEndPoint EndPoint { get; }
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;

    public ClientSession(int id, IPEndPoint endPoint)
    {
        Id = id;
        EndPoint = endPoint;
    }

    public void Touch() => LastSeen = DateTime.UtcNow;
}
