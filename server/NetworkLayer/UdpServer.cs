using System.Net;
using System.Net.Sockets;
using PocketController.Protocol;

namespace PocketController.NetworkLayer;

/// <summary>
/// Listens on a UDP port and raises <see cref="OnMessageReceived"/> for each
/// deserialized <see cref="GamepadMessage"/>. Runs the receive loop on a thread-pool thread.
/// </summary>
public class UdpServer : IDisposable
{
    private UdpClient? _udp;
    private CancellationTokenSource? _cts;
    private bool _disposed;

    /// <summary>The port this server is currently bound to.</summary>
    public int Port { get; private set; }

    /// <summary><c>true</c> between <see cref="Start"/> and <see cref="Stop"/>.</summary>
    public bool IsRunning { get; private set; }

    /// <summary>Raised on the receive loop thread for every valid incoming packet.</summary>
    public event Action<IPEndPoint, GamepadMessage>? OnMessageReceived;

    /// <summary>Raised when a non-cancellation exception occurs in the receive loop.</summary>
    public event Action<Exception>? OnError;

    /// <summary>Binds to <paramref name="port"/> and starts the background receive loop.</summary>
    public void Start(int port)
    {
        if (IsRunning) return;
        Port      = port;
        _udp      = new UdpClient(port);
        DisableUdpConnReset(_udp);
        _cts      = new CancellationTokenSource();
        IsRunning = true;
        Task.Run(() => ReceiveLoop(_cts.Token));
    }

    /// <summary>Signals the receive loop to stop and closes the socket.</summary>
    public void Stop()
    {
        if (!IsRunning) return;
        IsRunning = false;
        _cts?.Cancel();
        _udp?.Close();   // unblocks the pending ReceiveAsync
        _cts?.Dispose();
        _cts = null;
    }

    /// <summary>Sends a reply datagram to <paramref name="ep"/>. Failures are reported via <see cref="OnError"/>.</summary>
    public void Send(IPEndPoint ep, GamepadMessage msg)
    {
        var udp = _udp;
        if (!IsRunning || udp == null) return;
        try
        {
            var bytes = MessageSerializer.Serialize(msg);
            udp.Send(bytes, bytes.Length, ep);
        }
        catch (Exception ex)
        {
            OnError?.Invoke(ex);
        }
    }

    private async Task ReceiveLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var result = await _udp!.ReceiveAsync(ct);

                // Ignore stray or truncated datagrams instead of logging an exception for each one.
                if (result.Buffer.Length < MessageSerializer.PacketSize) continue;

                var msg    = MessageSerializer.Deserialize(result.Buffer);
                OnMessageReceived?.Invoke(result.RemoteEndPoint, msg);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                // Surface the error to the UI but keep the loop alive so one bad
                // packet doesn't bring down the entire server.
                OnError?.Invoke(ex);
            }
        }
    }

    /// <summary>
    /// On Windows, replying to a phone that has gone away makes the next receive throw
    /// WSAECONNRESET (from an ICMP "port unreachable"). Turn that off so one closed
    /// client cannot flood the log with errors.
    /// </summary>
    private static void DisableUdpConnReset(UdpClient udp)
    {
        if (!OperatingSystem.IsWindows()) return;
        const int SioUdpConnReset = -1744830452; // SIO_UDP_CONNRESET
        try
        {
            udp.Client.IOControl(SioUdpConnReset, [0, 0, 0, 0], null);
        }
        catch (SocketException)
        {
            // Not supported on this network stack; the receive loop still tolerates the error.
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Stop();
        _udp?.Dispose();
        GC.SuppressFinalize(this);
    }
}
