using System.Net;
using System.Net.Sockets;
using System.Text;
using PocketController.NetworkLayer;
using PocketController.Protocol;

namespace PocketController.Tests.NetworkLayer;

public class DiscoveryTests
{
    [Fact]
    public void Probe_RoundTrips_DeviceName()
    {
        var bytes = DiscoveryMessage.BuildProbe("Pixel 8");
        Assert.Equal("PCTRL?1|Pixel 8", Encoding.UTF8.GetString(bytes));
        Assert.Equal("Pixel 8", DiscoveryMessage.ParseProbe(bytes));
    }

    [Fact]
    public void Announcement_RoundTrips_IdAndAddresses()
    {
        var info  = new DiscoveryInfo("GAMING-PC", 5555, 1, 4, "abc123", ["192.168.0.5", "172.20.0.1"]);
        var text  = Encoding.UTF8.GetString(DiscoveryMessage.BuildAnnouncement(info));
        Assert.Contains("\"id\":\"abc123\"", text);
        Assert.Contains("\"ips\":[\"192.168.0.5\",\"172.20.0.1\"]", text);
        Assert.Equal(info, DiscoveryMessage.ParseAnnouncement(Encoding.UTF8.GetBytes(text)));
    }

    [Fact]
    public void Announcement_RoundTrips_Info()
    {
        var info  = new DiscoveryInfo("GAMING-PC", 5555, 1, 4);
        var bytes = DiscoveryMessage.BuildAnnouncement(info);
        Assert.StartsWith("PCTRL!1|{", Encoding.UTF8.GetString(bytes));
        Assert.Equal(info, DiscoveryMessage.ParseAnnouncement(bytes));
    }

    [Theory]
    [InlineData("")]
    [InlineData("hello")]
    [InlineData("PCTRL!1|not json")]
    [InlineData("PCTRL!1|{\"name\":\"x\",\"port\":0,\"clients\":0,\"max\":4}")]
    public void ParseAnnouncement_RejectsInvalid(string text)
        => Assert.Null(DiscoveryMessage.ParseAnnouncement(Encoding.UTF8.GetBytes(text)));

    [Fact]
    public void ParseProbe_IgnoresGamepadPackets()
        => Assert.Null(DiscoveryMessage.ParseProbe(MessageSerializer.Serialize(new GamepadMessage())));

    [Fact]
    public async Task Service_AnswersProbe_WithCurrentInfo()
    {
        int port = FreeUdpPort();
        using var service = new DiscoveryService(() => new DiscoveryInfo("TEST-PC", 6000, 2, 4));
        string? probedBy = null;
        service.OnProbe += (_, name) => probedBy = name;
        service.Start(port);

        using var phone = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        var probe = DiscoveryMessage.BuildProbe("Test Phone");
        await phone.SendAsync(probe, probe.Length, new IPEndPoint(IPAddress.Loopback, port));

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        var reply = await phone.ReceiveAsync(cts.Token);

        Assert.Equal(new DiscoveryInfo("TEST-PC", 6000, 2, 4), DiscoveryMessage.ParseAnnouncement(reply.Buffer));
        await Task.Delay(50);
        Assert.Equal("Test Phone", probedBy);
    }

    private static int FreeUdpPort()
    {
        using var s = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        return ((IPEndPoint)s.Client.LocalEndPoint!).Port;
    }
}
