using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using PocketConsole.Protocol;

namespace PocketConsole.GamepadDriver;

public sealed class VirtualGamepad : IDisposable
{
    private readonly IXbox360Controller _controller;
    private bool _disposed;

    public int ClientId { get; }

    public VirtualGamepad(ViGEmClient client, int clientId)
    {
        ClientId = clientId;
        _controller = client.CreateXbox360Controller();
        _controller.Connect();
    }

    public void Update(GamepadMessage msg) => GamepadMapper.Apply(_controller, msg);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _controller.Disconnect();
    }
}
