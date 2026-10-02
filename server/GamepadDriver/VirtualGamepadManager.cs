using Nefarius.ViGEm.Client;
using PocketController.Protocol;

namespace PocketController.GamepadDriver;

/// <summary>
/// Manages a pool of up to <see cref="Constants.MaxClients"/> virtual Xbox 360 controllers,
/// one per connected client. The ViGEmBus client is lazy-initialized on first <see cref="Initialize"/> call
/// so the app can start without the driver installed.
/// </summary>
public sealed class VirtualGamepadManager : IDisposable
{
    private ViGEmClient? _client;
    private readonly Dictionary<int, VirtualGamepad> _pads = new();
    private readonly object _lock = new();
    private bool _disposed;

    /// <summary>A snapshot of the active virtual controllers, keyed by client ID.</summary>
    public IReadOnlyDictionary<int, VirtualGamepad> Controllers
    {
        get { lock (_lock) return new Dictionary<int, VirtualGamepad>(_pads); }
    }

    /// <summary>
    /// Connects to the ViGEmBus driver. Must be called before <see cref="AddController"/>.
    /// </summary>
    /// <exception cref="Nefarius.ViGEm.Client.Exceptions.VigemBusNotFoundException">
    /// Thrown when the ViGEmBus driver is not installed on this machine.
    /// </exception>
    public void Initialize() => _client ??= new ViGEmClient();

    /// <summary>
    /// Creates a virtual controller for the given <paramref name="clientId"/>.
    /// Returns <c>false</c> if the client already has a controller or the pool is full.
    /// </summary>
    public bool AddController(int clientId)
    {
        lock (_lock)
        {
            if (_client == null || _pads.ContainsKey(clientId) || _pads.Count >= Constants.MaxClients)
                return false;

            _pads[clientId] = new VirtualGamepad(_client, clientId);
            return true;
        }
    }

    /// <summary>Disconnects and removes the virtual controller for the given <paramref name="clientId"/>.</summary>
    public void RemoveController(int clientId)
    {
        lock (_lock)
        {
            if (_pads.Remove(clientId, out var pad))
                pad.Dispose();
        }
    }

    /// <summary>Forwards an input packet to the virtual controller owned by <paramref name="clientId"/>.</summary>
    public void UpdateController(int clientId, GamepadMessage msg)
    {
        lock (_lock)
        {
            if (_pads.TryGetValue(clientId, out var pad))
                pad.Update(msg);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        lock (_lock)
        {
            foreach (var pad in _pads.Values) pad.Dispose();
            _pads.Clear();
        }
        _client?.Dispose();
    }
}
