using System.Net;
using PocketController.NetworkLayer;

namespace PocketController.Tests.NetworkLayer;

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

    [Fact]
    public void Remove_Twice_FiresDisconnectedOnce()
    {
        var mgr     = new ClientManager();
        var session = mgr.GetOrAdd(EP(9001))!;
        int fired   = 0;
        mgr.OnClientDisconnected += _ => fired++;

        mgr.Remove(session);
        mgr.Remove(session); // e.g. Disconnect packet and heartbeat timeout racing

        Assert.Equal(1, fired);
    }

    [Fact]
    public void Sessions_IsSnapshot_SafeToEnumerateWhileRemoving()
    {
        var mgr = new ClientManager();
        mgr.GetOrAdd(EP(9001));
        mgr.GetOrAdd(EP(9002));

        foreach (var s in mgr.Sessions)
            mgr.Remove(s);

        Assert.Empty(mgr.Sessions);
    }

    [Fact]
    public async Task ConcurrentAccess_DoesNotThrowOrCorruptState()
    {
        var mgr = new ClientManager();
        var workers = Enumerable.Range(0, 8).Select(w => Task.Run(() =>
        {
            for (int i = 0; i < 2000; i++)
            {
                var ep = EP(10000 + (i + w) % 6);
                var s  = mgr.GetOrAdd(ep);
                _ = mgr.Sessions.Count;
                if (s != null && i % 3 == 0) mgr.Remove(s);
            }
        })).ToArray();

        await Task.WhenAll(workers);

        Assert.True(mgr.Sessions.Count <= PocketController.Protocol.Constants.MaxClients);
        foreach (var s in mgr.Sessions)
        {
            Assert.Same(s, mgr.GetById(s.Id));
            Assert.Same(s, mgr.GetByEndpoint(s.EndPoint));
        }
    }
}
