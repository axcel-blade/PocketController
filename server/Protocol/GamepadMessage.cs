namespace PocketConsole.Protocol;

public struct GamepadMessage
{
    public MessageType Type;

    // Buttons bitmask (A=0, B=1, X=2, Y=3, LB=4, RB=5, Start=6, Back=7, LStick=8, RStick=9)
    public ushort Buttons;

    // Left joystick [-1.0, 1.0]
    public float LeftStickX;
    public float LeftStickY;

    // Right joystick [-1.0, 1.0]
    public float RightStickX;
    public float RightStickY;

    // Triggers [0.0, 1.0]
    public float LeftTrigger;
    public float RightTrigger;

    // D-pad (bitmask: Up=0, Down=1, Left=2, Right=3)
    public byte DPad;

    // Gyro (optional, degrees/second)
    public float GyroX;
    public float GyroY;
    public float GyroZ;

    public long TimestampMs;

    public bool IsButtonPressed(int buttonIndex) => (Buttons & (1 << buttonIndex)) != 0;
}
