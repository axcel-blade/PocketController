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
    }

    private async Task ReceiveLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var result = await _udp!.ReceiveAsync(ct);
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

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Stop();
        _udp?.Dispose();
        _cts?.Dispose();
    }
}
