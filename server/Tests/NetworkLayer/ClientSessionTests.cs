using System.Net;
using PocketController.NetworkLayer;

namespace PocketController.Tests.NetworkLayer;

public class ClientSessionTests
{
    [Fact]
    public void Touch_UpdatesLastSeen()
    {
        var session = new ClientSession(1, new IPEndPoint(IPAddress.Loopback, 9000));
        var before  = session.LastSeen;

        Thread.Sleep(10);
        session.Touch();

        Assert.True(session.LastSeen > before);
    }

    [Fact]
    public void NewSession_LastSeenIsRecent()
    {
        var before  = DateTime.UtcNow;
        var session = new ClientSession(1, new IPEndPoint(IPAddress.Loopback, 9000));
        var after   = DateTime.UtcNow;

        Assert.InRange(session.LastSeen, before, after);
    }
}
