namespace PocketController.Protocol;

/// <summary>
/// Converts <see cref="GamepadMessage"/> structs to and from raw byte arrays
/// using little-endian binary encoding (BinaryWriter default).
/// Field order must stay in sync with the Android client.
/// </summary>
public static class MessageSerializer
{
    /// <summary>Serializes a <see cref="GamepadMessage"/> into a 48-byte array.</summary>
    public static byte[] Serialize(GamepadMessage msg)
    {
        using var ms = new MemoryStream(Constants.BufferSize);
        using var w = new BinaryWriter(ms);

        w.Write((byte)msg.Type);
        w.Write(msg.Buttons);
        w.Write(msg.LeftStickX);
        w.Write(msg.LeftStickY);
        w.Write(msg.RightStickX);
        w.Write(msg.RightStickY);
        w.Write(msg.LeftTrigger);
        w.Write(msg.RightTrigger);
        w.Write(msg.DPad);
        w.Write(msg.GyroX);
        w.Write(msg.GyroY);
        w.Write(msg.GyroZ);
        w.Write(msg.TimestampMs);

        return ms.ToArray();
    }

    /// <summary>
    /// Deserializes a raw byte array into a <see cref="GamepadMessage"/>.
    /// Throws <see cref="EndOfStreamException"/> if <paramref name="data"/> is too short.
    /// </summary>
    public static GamepadMessage Deserialize(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var r = new BinaryReader(ms);

        return new GamepadMessage
        {
            Type         = (MessageType)r.ReadByte(),
            Buttons      = r.ReadUInt16(),
            LeftStickX   = r.ReadSingle(),
            LeftStickY   = r.ReadSingle(),
            RightStickX  = r.ReadSingle(),
            RightStickY  = r.ReadSingle(),
            LeftTrigger  = r.ReadSingle(),
            RightTrigger = r.ReadSingle(),
            DPad         = r.ReadByte(),
            GyroX        = r.ReadSingle(),
            GyroY        = r.ReadSingle(),
            GyroZ        = r.ReadSingle(),
            TimestampMs  = r.ReadInt64()
        };
    }
}
