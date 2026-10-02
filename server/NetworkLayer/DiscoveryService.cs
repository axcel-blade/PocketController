using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using PocketController.Protocol;

namespace PocketController.NetworkLayer;

/// <summary>
/// Makes the server findable on the local network.
/// <list type="bullet">
/// <item>Answers phone probes sent to <see cref="Constants.DiscoveryPort"/> with a unicast announcement.</item>
/// <item>Broadcasts an announcement to <see cref="Constants.AnnouncePort"/> every
/// <see cref="Constants.AnnounceIntervalMs"/> so listening phones find it without asking.</item>
/// </list>
/// </summary>
public sealed class DiscoveryService : IDisposable
{
    private readonly Func<DiscoveryInfo> _info;
    private UdpClient? _udp;
    private CancellationTokenSource? _cts;
    private System.Threading.Timer? _beacon;
    private bool _disposed;

    /// <param name="info">Called whenever an announcement is sent, so client counts stay current.</param>
    public DiscoveryService(Func<DiscoveryInfo> info) => _info = info;

    public bool IsRunning { get; private set; }

    /// <summary>Raised on a background thread when a phone probes for servers. Args: sender, device name.</summary>
    public event Action<IPEndPoint, string>? OnProbe;

    /// <summary>Raised when a socket error occurs.</summary>
    public event Action<Exception>? OnError;

    /// <summary>Starts answering probes and broadcasting announcements.</summary>
    /// <exception cref="SocketException">The discovery port is already in use.</exception>
    public void Start(int discoveryPort = Constants.DiscoveryPort)
    {
        if (IsRunning) return;
        var udp = new UdpClient(AddressFamily.InterNetwork);
        try
        {
            udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udp.Client.Bind(new IPEndPoint(IPAddress.Any, discoveryPort));
            udp.EnableBroadcast = true;
        }
        catch
        {
            udp.Dispose();
            throw;
        }

        _udp = udp;
        _cts = new CancellationTokenSource();
        IsRunning = true;
        Task.Run(() => ReceiveLoop(udp, _cts.Token));
        _beacon = new System.Threading.Timer(_ => Broadcast(), null, 0, Constants.AnnounceIntervalMs);
    }

    public void Stop()
    {
        if (!IsRunning) return;
        IsRunning = false;
        _beacon?.Dispose();
        _beacon = null;
        _cts?.Cancel();
        _udp?.Close();
        _udp = null;
        _cts?.Dispose();
        _cts = null;
    }

    private async Task ReceiveLoop(UdpClient udp, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var result = await udp.ReceiveAsync(ct);
                var device = DiscoveryMessage.ParseProbe(result.Buffer);
                if (device == null) continue;

                var reply = DiscoveryMessage.BuildAnnouncement(_info());
                await udp.SendAsync(reply, result.RemoteEndPoint, ct);
                OnProbe?.Invoke(result.RemoteEndPoint, device);
            }
            catch (OperationCanceledException) { break; }
            catch (ObjectDisposedException) { break; }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
            {
                // ICMP "port unreachable" from a phone that already stopped listening; harmless.
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                OnError?.Invoke(ex);
            }
        }
    }

    private void Broadcast()
    {
        var udp = _udp;
        if (!IsRunning || udp == null) return;
        var packet = DiscoveryMessage.BuildAnnouncement(_info());
        foreach (var target in BroadcastAddresses())
        {
            try
            {
                udp.Send(packet, packet.Length, new IPEndPoint(target, Constants.AnnouncePort));
            }
            catch (SocketException)
            {
                // Some adapters (VPNs, disconnected NICs) reject broadcasts; skip them.
            }
            catch (ObjectDisposedException)
            {
                return;
            }
        }
    }

    /// <summary>This PC's IPv4 addresses on active, non-loopback adapters.</summary>
    public static IReadOnlyList<string> LocalAddresses()
    {
        var result = new List<string>();
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up ||
                ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
            foreach (var ua in ni.GetIPProperties().UnicastAddresses)
            {
                if (ua.Address.AddressFamily == AddressFamily.InterNetwork)
                    result.Add(ua.Address.ToString());
            }
        }
        return result;
    }

    /// <summary>The limited broadcast address plus each active IPv4 interface's subnet broadcast.</summary>
    internal static IEnumerable<IPAddress> BroadcastAddresses()
    {
        var result = new HashSet<IPAddress> { IPAddress.Broadcast };
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up ||
                ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
            foreach (var ua in ni.GetIPProperties().UnicastAddresses)
            {
                if (ua.Address.AddressFamily != AddressFamily.InterNetwork || ua.IPv4Mask == null) continue;
                var ip   = ua.Address.GetAddressBytes();
                var mask = ua.IPv4Mask.GetAddressBytes();
                var bc   = new byte[4];
                for (int i = 0; i < 4; i++) bc[i] = (byte)(ip[i] | ~mask[i]);
                result.Add(new IPAddress(bc));
            }
        }
        return result;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Stop();
    }
}
