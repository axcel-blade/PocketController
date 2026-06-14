/// Live state of all controller inputs, updated by the UI and read by the UDP sender.
class GamepadState {
  // Button bitmask: A=0, B=1, X=2, Y=3, LB=4, RB=5, Start=6, Back=7, LStick=8, RStick=9
  int buttons = 0;

  double leftStickX = 0.0;
  double leftStickY = 0.0;
  double rightStickX = 0.0;
  double rightStickY = 0.0;

  double leftTrigger = 0.0;
  double rightTrigger = 0.0;

  // DPad bitmask: Up=0, Down=1, Left=2, Right=3
  int dpad = 0;

  double gyroX = 0.0;
  double gyroY = 0.0;
  double gyroZ = 0.0;

  void setButton(int bit, bool pressed) {
    if (pressed) {
      buttons |= (1 << bit);
    } else {
      buttons &= ~(1 << bit);
    }
  }

  void setDpad(int bit, bool pressed) {
    if (pressed) {
      dpad |= (1 << bit);
    } else {
      dpad &= ~(1 << bit);
    }
  }
}
