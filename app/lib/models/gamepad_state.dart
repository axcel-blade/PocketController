/// Bit positions within [GamepadState.buttons]. Must match the server's GamepadMessage.Buttons.
class Btn {
  static const a = 0, b = 1, x = 2, y = 3;
  static const lb = 4, rb = 5, start = 6, back = 7;
  static const lStick = 8, rStick = 9, guide = 10;
}

/// Bit positions within [GamepadState.dpad].
class Dpad {
  static const up = 0, down = 1, left = 2, right = 3;
}

/// Live state of all controller inputs, written by the UI and read by the bridge sender.
class GamepadState {
  int buttons = 0;
  int dpad = 0;

  /// Sticks are normalized to [-1, 1]; +Y is up.
  double leftStickX = 0, leftStickY = 0;
  double rightStickX = 0, rightStickY = 0;

  /// Triggers are normalized to [0, 1].
  double leftTrigger = 0, rightTrigger = 0;

  /// Called for discrete changes (button press/release, D-pad, stick/trigger release)
  /// so they are sent immediately instead of waiting for the next periodic packet.
  void Function()? onDiscreteChange;

  bool isPressed(int bit) => (buttons & (1 << bit)) != 0;

  void setButton(int bit, bool pressed) {
    final next = pressed ? buttons | (1 << bit) : buttons & ~(1 << bit);
    if (next == buttons) return;
    buttons = next;
    onDiscreteChange?.call();
  }

  void setDpad(int mask) {
    if (mask == dpad) return;
    dpad = mask;
    onDiscreteChange?.call();
  }

  void setLeftStick(double x, double y) {
    leftStickX = _clampAxis(x);
    leftStickY = _clampAxis(y);
    if (x == 0 && y == 0) onDiscreteChange?.call();
  }

  void setRightStick(double x, double y) {
    rightStickX = _clampAxis(x);
    rightStickY = _clampAxis(y);
    if (x == 0 && y == 0) onDiscreteChange?.call();
  }

  void setLeftTrigger(double v) {
    leftTrigger = v.clamp(0.0, 1.0);
    onDiscreteChange?.call();
  }

  void setRightTrigger(double v) {
    rightTrigger = v.clamp(0.0, 1.0);
    onDiscreteChange?.call();
  }

  /// Releases every input. Used when the app is backgrounded or the layout is edited.
  void reset() {
    buttons = 0;
    dpad = 0;
    leftStickX = leftStickY = rightStickX = rightStickY = 0;
    leftTrigger = rightTrigger = 0;
    onDiscreteChange?.call();
  }

  static double _clampAxis(double v) => v.clamp(-1.0, 1.0);
}
