using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using PocketConsole.Protocol;

namespace PocketConsole.GamepadDriver;

/// <summary>
/// Translates a <see cref="GamepadMessage"/> from the Android client into
/// ViGEm Xbox 360 controller property calls.
/// </summary>
public static class GamepadMapper
{
    // Bit positions within GamepadMessage.Buttons — must match the Android client.
    private const int BtnA = 0, BtnB = 1, BtnX = 2, BtnY = 3;
    private const int BtnLB = 4, BtnRB = 5, BtnStart = 6, BtnBack = 7;
    private const int BtnLStick = 8, BtnRStick = 9;

    /// <summary>
    /// Applies all fields of <paramref name="msg"/> to the given <paramref name="ctrl"/>.
    /// Called on every Input packet, so this must stay allocation-free.
    /// </summary>
    public static void Apply(IXbox360Controller ctrl, GamepadMessage msg)
    {
        // Face buttons
        ctrl.SetButtonState(Xbox360Button.A, msg.IsButtonPressed(BtnA));
        ctrl.SetButtonState(Xbox360Button.B, msg.IsButtonPressed(BtnB));
        ctrl.SetButtonState(Xbox360Button.X, msg.IsButtonPressed(BtnX));
        ctrl.SetButtonState(Xbox360Button.Y, msg.IsButtonPressed(BtnY));

        // Shoulder buttons and menu
        ctrl.SetButtonState(Xbox360Button.LeftShoulder,  msg.IsButtonPressed(BtnLB));
        ctrl.SetButtonState(Xbox360Button.RightShoulder, msg.IsButtonPressed(BtnRB));
        ctrl.SetButtonState(Xbox360Button.Start,         msg.IsButtonPressed(BtnStart));
        ctrl.SetButtonState(Xbox360Button.Back,          msg.IsButtonPressed(BtnBack));

        // Stick clicks
        ctrl.SetButtonState(Xbox360Button.LeftThumb,  msg.IsButtonPressed(BtnLStick));
        ctrl.SetButtonState(Xbox360Button.RightThumb, msg.IsButtonPressed(BtnRStick));

        // D-pad — each direction is an independent bit in DPad bitmask
        ctrl.SetButtonState(Xbox360Button.Up,    (msg.DPad & 1) != 0);
        ctrl.SetButtonState(Xbox360Button.Down,  (msg.DPad & 2) != 0);
        ctrl.SetButtonState(Xbox360Button.Left,  (msg.DPad & 4) != 0);
        ctrl.SetButtonState(Xbox360Button.Right, (msg.DPad & 8) != 0);

        // Analog sticks — ViGEm expects short range [-32768, 32767]
        ctrl.SetAxisValue(Xbox360Axis.LeftThumbX,  FloatToShort(msg.LeftStickX));
        ctrl.SetAxisValue(Xbox360Axis.LeftThumbY,  FloatToShort(msg.LeftStickY));
        ctrl.SetAxisValue(Xbox360Axis.RightThumbX, FloatToShort(msg.RightStickX));
        ctrl.SetAxisValue(Xbox360Axis.RightThumbY, FloatToShort(msg.RightStickY));

        // Triggers — ViGEm expects byte range [0, 255]
        ctrl.SetSliderValue(Xbox360Slider.LeftTrigger,  FloatToByte(msg.LeftTrigger));
        ctrl.SetSliderValue(Xbox360Slider.RightTrigger, FloatToByte(msg.RightTrigger));
    }

    /// <summary>Maps a normalized float [-1, 1] to the Xbox short axis range.</summary>
    private static short FloatToShort(float v)
        => (short)Math.Clamp((int)(v * short.MaxValue), short.MinValue, short.MaxValue);

    /// <summary>Maps a normalized float [0, 1] to a byte trigger value.</summary>
    private static byte FloatToByte(float v)
        => (byte)Math.Clamp((int)(v * 255), 0, 255);
}
