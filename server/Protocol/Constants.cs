namespace PocketConsole.Protocol;

/// <summary>Shared constants used by both the server and the Android client.</summary>
public static class Constants
{
    /// <summary>Default UDP port the server listens on.</summary>
    public const int DefaultPort = 5555;

    /// <summary>Maximum number of simultaneous clients (one virtual controller each).</summary>
    public const int MaxClients = 4;

    /// <summary>Maximum packet size in bytes. All <see cref="GamepadMessage"/> packets are 48 bytes.</summary>
    public const int BufferSize = 256;

    /// <summary>How often the heartbeat monitor checks for dead clients (milliseconds).</summary>
    public const int HeartbeatIntervalMs = 3000;

    /// <summary>A client is dropped if no packet is received within this window (milliseconds).</summary>
    public const int ClientTimeoutMs = 5000;
}
