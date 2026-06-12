namespace PocketConsole.Protocol;

public static class MessageSerializer
{
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

    public static GamepadMessage Deserialize(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var r = new BinaryReader(ms);

        return new GamepadMessage
        {
            Type = (MessageType)r.ReadByte(),
            Buttons = r.ReadUInt16(),
            LeftStickX = r.ReadSingle(),
            LeftStickY = r.ReadSingle(),
            RightStickX = r.ReadSingle(),
            RightStickY = r.ReadSingle(),
            LeftTrigger = r.ReadSingle(),
            RightTrigger = r.ReadSingle(),
            DPad = r.ReadByte(),
            GyroX = r.ReadSingle(),
            GyroY = r.ReadSingle(),
            GyroZ = r.ReadSingle(),
            TimestampMs = r.ReadInt64()
        };
    }
}
