using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using PocketConsole.Protocol;

namespace PocketConsole.GamepadDriver;

public static class GamepadMapper
{
    private const int BtnA = 0, BtnB = 1, BtnX = 2, BtnY = 3;
    private const int BtnLB = 4, BtnRB = 5, BtnStart = 6, BtnBack = 7;
    private const int BtnLStick = 8, BtnRStick = 9;

    public static void Apply(IXbox360Controller ctrl, GamepadMessage msg)
    {
        ctrl.SetButtonState(Xbox360Button.A, msg.IsButtonPressed(BtnA));
        ctrl.SetButtonState(Xbox360Button.B, msg.IsButtonPressed(BtnB));
        ctrl.SetButtonState(Xbox360Button.X, msg.IsButtonPressed(BtnX));
        ctrl.SetButtonState(Xbox360Button.Y, msg.IsButtonPressed(BtnY));
        ctrl.SetButtonState(Xbox360Button.LeftShoulder, msg.IsButtonPressed(BtnLB));
        ctrl.SetButtonState(Xbox360Button.RightShoulder, msg.IsButtonPressed(BtnRB));
        ctrl.SetButtonState(Xbox360Button.Start, msg.IsButtonPressed(BtnStart));
        ctrl.SetButtonState(Xbox360Button.Back, msg.IsButtonPressed(BtnBack));
        ctrl.SetButtonState(Xbox360Button.LeftThumb, msg.IsButtonPressed(BtnLStick));
        ctrl.SetButtonState(Xbox360Button.RightThumb, msg.IsButtonPressed(BtnRStick));

        ctrl.SetButtonState(Xbox360Button.Up, (msg.DPad & 1) != 0);
        ctrl.SetButtonState(Xbox360Button.Down, (msg.DPad & 2) != 0);
        ctrl.SetButtonState(Xbox360Button.Left, (msg.DPad & 4) != 0);
        ctrl.SetButtonState(Xbox360Button.Right, (msg.DPad & 8) != 0);

        ctrl.SetAxisValue(Xbox360Axis.LeftThumbX, FloatToShort(msg.LeftStickX));
        ctrl.SetAxisValue(Xbox360Axis.LeftThumbY, FloatToShort(msg.LeftStickY));
        ctrl.SetAxisValue(Xbox360Axis.RightThumbX, FloatToShort(msg.RightStickX));
        ctrl.SetAxisValue(Xbox360Axis.RightThumbY, FloatToShort(msg.RightStickY));

        ctrl.SetSliderValue(Xbox360Slider.LeftTrigger, FloatToByte(msg.LeftTrigger));
        ctrl.SetSliderValue(Xbox360Slider.RightTrigger, FloatToByte(msg.RightTrigger));
    }

    private static short FloatToShort(float v)
        => (short)Math.Clamp((int)(v * short.MaxValue), short.MinValue, short.MaxValue);

    private static byte FloatToByte(float v)
        => (byte)Math.Clamp((int)(v * 255), 0, 255);
}
