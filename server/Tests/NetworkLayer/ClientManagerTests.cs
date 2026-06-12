using System.Net;
using PocketConsole.NetworkLayer;

namespace PocketConsole.Tests.NetworkLayer;

public class ClientManagerTests
{
    private static IPEndPoint EP(int port) => new(IPAddress.Loopback, port);

    [Fact]
    public void GetOrAdd_NewEndpoint_AssignsId()
    {
        var mgr     = new ClientManager();
        var session = mgr.GetOrAdd(EP(9001));

        Assert.NotNull(session);
        Assert.Equal(1, session!.Id);
        Assert.Equal(EP(9001), session.EndPoint);
    }

    [Fact]
    public void GetOrAdd_SameEndpoint_ReturnsSameSession()
    {
        var mgr = new ClientManager();
        var ep  = EP(9001);

        var s1 = mgr.GetOrAdd(ep);
        var s2 = mgr.GetOrAdd(ep);

        Assert.Same(s1, s2);
        Assert.Single(mgr.Sessions);
    }

    [Fact]
    public void GetOrAdd_MaxClients_ReturnsNull()
    {
        var mgr = new ClientManager();
        for (int i = 0; i < 4; i++)
            mgr.GetOrAdd(EP(9000 + i));

        var overflow = mgr.GetOrAdd(EP(9999));

        Assert.Null(overflow);
        Assert.Equal(4, mgr.Sessions.Count);
    }

    [Fact]
    public void Remove_FiresDisconnectedEvent()
    {
        var mgr = new ClientManager();
        ClientSession? fired = null;
        mgr.OnClientDisconnected += s => fired = s;

        var session = mgr.GetOrAdd(EP(9001))!;
        mgr.Remove(session);

        Assert.Same(session, fired);
        Assert.Empty(mgr.Sessions);
    }

    [Fact]
    public void GetOrAdd_FiresConnectedEvent()
    {
        var mgr = new ClientManager();
        ClientSession? fired = null;
        mgr.OnClientConnected += s => fired = s;

        var session = mgr.GetOrAdd(EP(9001));

        Assert.Same(session, fired);
    }

    [Fact]
    public void GetById_ReturnsCorrectSession()
    {
        var mgr     = new ClientManager();
        var session = mgr.GetOrAdd(EP(9001))!;

        Assert.Same(session, mgr.GetById(session.Id));
        Assert.Null(mgr.GetById(999));
    }

    [Fact]
    public void GetByEndpoint_ReturnsCorrectSession()
    {
        var mgr     = new ClientManager();
        var ep      = EP(9001);
        var session = mgr.GetOrAdd(ep)!;

        Assert.Same(session, mgr.GetByEndpoint(ep));
        Assert.Null(mgr.GetByEndpoint(EP(9999)));
    }

    [Fact]
    public void IdsAreUnique_AcrossMultipleClients()
    {
        var mgr = new ClientManager();
        var ids = Enumerable.Range(0, 4)
            .Select(i => mgr.GetOrAdd(EP(9000 + i))!.Id)
            .ToList();

        Assert.Equal(ids.Distinct().Count(), ids.Count);
    }

    [Fact]
    public void Remove_ThenAdd_AcceptsNewClient()
    {
        var mgr = new ClientManager();
        for (int i = 0; i < 4; i++) mgr.GetOrAdd(EP(9000 + i));

        mgr.Remove(mgr.GetByEndpoint(EP(9000))!);
        var newSession = mgr.GetOrAdd(EP(9010));

        Assert.NotNull(newSession);
        Assert.Equal(4, mgr.Sessions.Count);
    }
}
