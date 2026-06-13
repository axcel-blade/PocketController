namespace PocketController.Protocol;

/// <summary>
/// Single UDP packet carrying all controller state from the Android client.
/// Serialized to exactly 48 bytes by <see cref="MessageSerializer"/>.
/// </summary>
public struct GamepadMessage
{
    /// <summary>Packet intent — must be read before any other field.</summary>
    public MessageType Type;

    /// <summary>
    /// Button states packed into a bitmask.
    /// Bit positions: A=0, B=1, X=2, Y=3, LB=4, RB=5, Start=6, Back=7, LStick=8, RStick=9.
    /// </summary>
    public ushort Buttons;

    /// <summary>Left stick horizontal axis. Range: -1.0 (left) to 1.0 (right).</summary>
    public float LeftStickX;

    /// <summary>Left stick vertical axis. Range: -1.0 (down) to 1.0 (up).</summary>
    public float LeftStickY;

    /// <summary>Right stick horizontal axis. Range: -1.0 (left) to 1.0 (right).</summary>
    public float RightStickX;

    /// <summary>Right stick vertical axis. Range: -1.0 (down) to 1.0 (up).</summary>
    public float RightStickY;

    /// <summary>Left trigger pressure. Range: 0.0 (released) to 1.0 (fully pressed).</summary>
    public float LeftTrigger;

    /// <summary>Right trigger pressure. Range: 0.0 (released) to 1.0 (fully pressed).</summary>
    public float RightTrigger;

    /// <summary>
    /// D-pad directions packed into a bitmask.
    /// Bit positions: Up=0, Down=1, Left=2, Right=3.
    /// </summary>
    public byte DPad;

    /// <summary>Gyroscope rotation around the X axis (degrees/second). Optional — zero if unsupported.</summary>
    public float GyroX;

    /// <summary>Gyroscope rotation around the Y axis (degrees/second). Optional — zero if unsupported.</summary>
    public float GyroY;

    /// <summary>Gyroscope rotation around the Z axis (degrees/second). Optional — zero if unsupported.</summary>
    public float GyroZ;

    /// <summary>Client-side timestamp in milliseconds since epoch. Used for latency diagnostics.</summary>
    public long TimestampMs;

    /// <summary>Returns <c>true</c> if the button at the given <paramref name="buttonIndex"/> bit is pressed.</summary>
    public bool IsButtonPressed(int buttonIndex) => (Buttons & (1 << buttonIndex)) != 0;
}
