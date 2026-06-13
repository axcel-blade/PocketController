using PocketController.Protocol;

namespace PocketController.Tests.Protocol;

public class GamepadMessageTests
{
    [Theory]
    [InlineData(0,  true)]   // A
    [InlineData(1,  true)]   // B
    [InlineData(4,  true)]   // LB
    [InlineData(9,  true)]   // RStick
    [InlineData(10, false)]  // bit 10 not set
    public void IsButtonPressed_ReturnsCorrectState(int bit, bool expected)
    {
        var msg = new GamepadMessage { Buttons = 0b11_0011_1111 };
        Assert.Equal(expected, msg.IsButtonPressed(bit));
    }

    [Fact]
    public void IsButtonPressed_AllZero_ReturnsFalse()
    {
        var msg = new GamepadMessage { Buttons = 0 };
        for (int i = 0; i < 10; i++)
            Assert.False(msg.IsButtonPressed(i));
    }

    [Fact]
    public void IsButtonPressed_AllSet_ReturnsTrue()
    {
        var msg = new GamepadMessage { Buttons = ushort.MaxValue };
        for (int i = 0; i < 10; i++)
            Assert.True(msg.IsButtonPressed(i));
    }
}
