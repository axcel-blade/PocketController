using System.Net;
using System.Net.Sockets;
using PocketConsole.Protocol;

namespace PocketConsole.NetworkLayer;

public class UdpServer : IDisposable
{
    private UdpClient? _udp;
    private CancellationTokenSource? _cts;
    private bool _disposed;

    public int Port { get; private set; }
    public bool IsRunning { get; private set; }

    public event Action<IPEndPoint, GamepadMessage>? OnMessageReceived;
    public event Action<Exception>? OnError;

    public void Start(int port)
    {
        if (IsRunning) return;
        Port = port;
        _udp = new UdpClient(port);
        _cts = new CancellationTokenSource();
        IsRunning = true;
        Task.Run(() => ReceiveLoop(_cts.Token));
    }

    public void Stop()
    {
        if (!IsRunning) return;
        IsRunning = false;
        _cts?.Cancel();
        _udp?.Close();
    }

    private async Task ReceiveLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var result = await _udp!.ReceiveAsync(ct);
                var msg = MessageSerializer.Deserialize(result.Buffer);
                OnMessageReceived?.Invoke(result.RemoteEndPoint, msg);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                OnError?.Invoke(ex);
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Stop();
        _udp?.Dispose();
        _cts?.Dispose();
    }
}
