using PocketController.Protocol;

namespace PocketController.Tests.Protocol;

public class MessageSerializerTests
{
    private static GamepadMessage BuildFull() => new()
    {
        Type         = MessageType.Input,
        Buttons      = 0b0000_0000_0000_1111,   // A B X Y
        LeftStickX   =  0.75f,
        LeftStickY   = -0.50f,
        RightStickX  =  0.25f,
        RightStickY  =  1.00f,
        LeftTrigger  =  0.80f,
        RightTrigger =  0.20f,
        DPad         = 0b0101,                   // Up + Left
        GyroX        =  1.1f,
        GyroY        = -2.2f,
        GyroZ        =  3.3f,
        TimestampMs  = 123456789L
    };

    [Fact]
    public void RoundTrip_PreservesAllFields()
    {
        var original = BuildFull();
        var bytes    = MessageSerializer.Serialize(original);
        var decoded  = MessageSerializer.Deserialize(bytes);

        Assert.Equal(original.Type,         decoded.Type);
        Assert.Equal(original.Buttons,      decoded.Buttons);
        Assert.Equal(original.LeftStickX,   decoded.LeftStickX,  precision: 5);
        Assert.Equal(original.LeftStickY,   decoded.LeftStickY,  precision: 5);
        Assert.Equal(original.RightStickX,  decoded.RightStickX, precision: 5);
        Assert.Equal(original.RightStickY,  decoded.RightStickY, precision: 5);
        Assert.Equal(original.LeftTrigger,  decoded.LeftTrigger,  precision: 5);
        Assert.Equal(original.RightTrigger, decoded.RightTrigger, precision: 5);
        Assert.Equal(original.DPad,         decoded.DPad);
        Assert.Equal(original.GyroX,        decoded.GyroX,  precision: 5);
        Assert.Equal(original.GyroY,        decoded.GyroY,  precision: 5);
        Assert.Equal(original.GyroZ,        decoded.GyroZ,  precision: 5);
        Assert.Equal(original.TimestampMs,  decoded.TimestampMs);
    }

    [Fact]
    public void Serialize_ProducesFixedSize()
    {
        var bytes = MessageSerializer.Serialize(BuildFull());
        // 1 + 2 + 4*6 + 1 + 4*3 + 8 = 48 bytes
        Assert.Equal(48, bytes.Length);
    }

    [Theory]
    [InlineData(MessageType.Connect)]
    [InlineData(MessageType.Disconnect)]
    [InlineData(MessageType.Input)]
    [InlineData(MessageType.Ping)]
    public void RoundTrip_PreservesMessageType(MessageType type)
    {
        var msg = new GamepadMessage { Type = type };
        var decoded = MessageSerializer.Deserialize(MessageSerializer.Serialize(msg));
        Assert.Equal(type, decoded.Type);
    }

    [Fact]
    public void RoundTrip_ZeroMessage_DoesNotThrow()
    {
        var msg     = new GamepadMessage();
        var decoded = MessageSerializer.Deserialize(MessageSerializer.Serialize(msg));
        Assert.Equal(MessageType.Connect, decoded.Type);
        Assert.Equal(0, decoded.Buttons);
        Assert.Equal(0f, decoded.LeftStickX);
    }

    [Fact]
    public void RoundTrip_MaxValues()
    {
        var msg = new GamepadMessage
        {
            Buttons      = ushort.MaxValue,
            LeftStickX   =  1f,
            LeftStickY   = -1f,
            LeftTrigger  =  1f,
            RightTrigger =  1f,
            DPad         = 0b1111,
            TimestampMs  = long.MaxValue
        };
        var decoded = MessageSerializer.Deserialize(MessageSerializer.Serialize(msg));
        Assert.Equal(ushort.MaxValue, decoded.Buttons);
        Assert.Equal(1f,  decoded.LeftStickX,  precision: 5);
        Assert.Equal(-1f, decoded.LeftStickY,  precision: 5);
        Assert.Equal(long.MaxValue, decoded.TimestampMs);
    }
}
