using Nefarius.ViGEm.Client;
using PocketConsole.Protocol;

namespace PocketConsole.GamepadDriver;

public sealed class VirtualGamepadManager : IDisposable
{
    private ViGEmClient? _client;
    private readonly Dictionary<int, VirtualGamepad> _pads = new();
    private bool _disposed;

    // Throws VigemBusNotFoundException if the ViGEmBus driver is not installed.
    public void Initialize() => _client ??= new ViGEmClient();

    public IReadOnlyDictionary<int, VirtualGamepad> Controllers => _pads;

    public bool AddController(int clientId)
    {
        if (_pads.ContainsKey(clientId) || _pads.Count >= Constants.MaxClients)
            return false;

        _pads[clientId] = new VirtualGamepad(_client!, clientId);
        return true;
    }

    public void RemoveController(int clientId)
    {
        if (_pads.TryGetValue(clientId, out var pad))
        {
            pad.Dispose();
            _pads.Remove(clientId);
        }
    }

    public void UpdateController(int clientId, GamepadMessage msg)
    {
        if (_pads.TryGetValue(clientId, out var pad))
            pad.Update(msg);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        foreach (var pad in _pads.Values) pad.Dispose();
        _pads.Clear();
        _client?.Dispose();
    }
}
