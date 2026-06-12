namespace PocketConsole.Protocol;

/// <summary>
/// Identifies the intent of a <see cref="GamepadMessage"/> packet.
/// Stored as the first byte of every UDP datagram.
/// </summary>
public enum MessageType : byte
{
    /// <summary>Sent once by the client when it first connects. Triggers session registration.</summary>
    Connect = 0,

    /// <summary>Sent by the client when it disconnects gracefully. Triggers immediate session removal.</summary>
    Disconnect = 1,

    /// <summary>Carries live controller state (buttons, sticks, triggers, gyro).</summary>
    Input = 2,

    /// <summary>Keep-alive packet. Resets the server-side idle timer without sending input data.</summary>
    Ping = 3
}
