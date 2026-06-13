using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using PocketController.Protocol;

namespace PocketController.GamepadDriver;

/// <summary>
/// Wraps a single ViGEm Xbox 360 virtual controller for one connected client.
/// Created on client connect, disposed on client disconnect.
/// </summary>
public sealed class VirtualGamepad : IDisposable
{
    private readonly IXbox360Controller _controller;
    private bool _disposed;

    /// <summary>The client ID this gamepad is bound to.</summary>
    public int ClientId { get; }

    /// <summary>
    /// Creates and connects a new virtual Xbox 360 controller via ViGEmBus.
    /// </summary>
    /// <param name="client">Shared ViGEm bus client.</param>
    /// <param name="clientId">ID of the network client that owns this controller.</param>
    public VirtualGamepad(ViGEmClient client, int clientId)
    {
        ClientId    = clientId;
        _controller = client.CreateXbox360Controller();
        _controller.Connect();
    }

    /// <summary>Applies an input packet to the virtual controller.</summary>
    public void Update(GamepadMessage msg) => GamepadMapper.Apply(_controller, msg);

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _controller.Disconnect();
    }
}
