import 'dart:async';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:sensors_plus/sensors_plus.dart';
import '../models/gamepad_state.dart';
import '../network/udp_client.dart';
import '../widgets/analog_stick.dart';
import '../widgets/dpad_widget.dart';
import '../widgets/face_buttons.dart';
import '../widgets/trigger_slider.dart';

// Button bit indices matching server GamepadMessage.Buttons bitmask
const int _kA = 0, _kB = 1, _kX = 2, _kY = 3;
const int _kLB = 4, _kRB = 5, _kStart = 6, _kBack = 7;

/// Main controller UI — landscape-locked Xbox-style layout.
class ControllerScreen extends StatefulWidget {
  final UdpClient client;
  final GamepadState state;

  const ControllerScreen({super.key, required this.client, required this.state});

  @override
  State<ControllerScreen> createState() => _ControllerScreenState();
}

class _ControllerScreenState extends State<ControllerScreen> {
  final Set<String> _pressedFace = {};
  bool _lbPressed = false;
  bool _rbPressed = false;
  StreamSubscription<GyroscopeEvent>? _gyroSub;

  @override
  void initState() {
    super.initState();
    SystemChrome.setPreferredOrientations([DeviceOrientation.landscapeLeft, DeviceOrientation.landscapeRight]);
    SystemChrome.setEnabledSystemUIMode(SystemUiMode.immersiveSticky);
    _gyroSub = gyroscopeEventStream().listen((e) {
      widget.state.gyroX = e.x;
      widget.state.gyroY = e.y;
      widget.state.gyroZ = e.z;
    });
  }

  @override
  void dispose() {
    _gyroSub?.cancel();
    widget.client.disconnect();
    SystemChrome.setPreferredOrientations([]);
    SystemChrome.setEnabledSystemUIMode(SystemUiMode.edgeToEdge);
    super.dispose();
  }

  void _onFaceButton(String button, bool pressed) {
    setState(() {
      pressed ? _pressedFace.add(button) : _pressedFace.remove(button);
    });
    final bit = {'A': _kA, 'B': _kB, 'X': _kX, 'Y': _kY}[button]!;
    widget.state.setButton(bit, pressed);
  }

  Widget _bumper(String label, bool pressed, void Function(bool) onChanged) {
    return GestureDetector(
      onTapDown: (_) { setState(() {}); onChanged(true); },
      onTapUp: (_) { setState(() {}); onChanged(false); },
      onTapCancel: () { setState(() {}); onChanged(false); },
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 60),
        width: 70,
        height: 28,
        decoration: BoxDecoration(
          color: pressed ? Colors.white30 : Colors.white12,
          borderRadius: BorderRadius.circular(14),
          border: Border.all(color: Colors.white30),
        ),
        child: Center(
          child: Text(label, style: const TextStyle(color: Colors.white70, fontWeight: FontWeight.bold, fontSize: 12)),
        ),
      ),
    );
  }

  Widget _menuButton(String label, int bit) {
    bool pressed = (widget.state.buttons & (1 << bit)) != 0;
    return GestureDetector(
      onTapDown: (_) { widget.state.setButton(bit, true); setState(() {}); },
      onTapUp: (_) { widget.state.setButton(bit, false); setState(() {}); },
      onTapCancel: () { widget.state.setButton(bit, false); setState(() {}); },
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 60),
        width: 44,
        height: 22,
        decoration: BoxDecoration(
          color: pressed ? Colors.white30 : Colors.white12,
          borderRadius: BorderRadius.circular(11),
          border: Border.all(color: Colors.white30),
        ),
        child: Center(
          child: Text(label, style: const TextStyle(color: Colors.white70, fontSize: 10, fontWeight: FontWeight.w600)),
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final size = MediaQuery.of(context).size;
    final h = size.height;
    final w = size.width;

    final stickSize = h * 0.38;
    final dpadSize = h * 0.30;
    final faceButtonSize = h * 0.095;
    final triggerW = 38.0;
    final triggerH = h * 0.52;

    return Scaffold(
      backgroundColor: const Color(0xFF0E0E0E),
      body: SafeArea(
        child: Stack(
          children: [
            // === LEFT SIDE ===
            Positioned(
              left: 0,
              top: 0,
              bottom: 0,
              width: w * 0.38,
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  // LB + LT row
                  Row(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      TriggerSlider(
                        label: 'LT',
                        width: triggerW,
                        height: triggerH,
                        onChanged: (v) => widget.state.leftTrigger = v,
                      ),
                      const SizedBox(width: 8),
                      Column(
                        mainAxisAlignment: MainAxisAlignment.start,
                        children: [
                          _bumper('LB', _lbPressed, (p) {
                            setState(() => _lbPressed = p);
                            widget.state.setButton(_kLB, p);
                          }),
                          SizedBox(height: triggerH - 40),
                        ],
                      ),
                    ],
                  ),
                  const SizedBox(height: 8),
                  // Left stick
                  AnalogStick(
                    size: stickSize,
                    onChanged: (x, y) { widget.state.leftStickX = x; widget.state.leftStickY = y; },
                  ),
                ],
              ),
            ),

            // === CENTER ===
            Positioned(
              left: w * 0.35,
              right: w * 0.35,
              top: 0,
              bottom: 0,
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  // Xbox guide button
                  Container(
                    width: 48,
                    height: 48,
                    decoration: const BoxDecoration(
                      color: Color(0xFF107C10),
                      shape: BoxShape.circle,
                    ),
                    child: const Center(
                      child: Text('X', style: TextStyle(color: Colors.white, fontSize: 22, fontWeight: FontWeight.bold)),
                    ),
                  ),
                  const SizedBox(height: 16),
                  // Back / Start
                  Row(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      _menuButton('Back', _kBack),
                      const SizedBox(width: 12),
                      _menuButton('Start', _kStart),
                    ],
                  ),
                  const SizedBox(height: 16),
                  // D-pad
                  DpadWidget(
                    size: dpadSize,
                    onChanged: (bits) { widget.state.dpad = bits; setState(() {}); },
                  ),
                ],
              ),
            ),

            // === RIGHT SIDE ===
            Positioned(
              right: 0,
              top: 0,
              bottom: 0,
              width: w * 0.38,
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  // RB + RT row
                  Row(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      Column(
                        mainAxisAlignment: MainAxisAlignment.start,
                        children: [
                          _bumper('RB', _rbPressed, (p) {
                            setState(() => _rbPressed = p);
                            widget.state.setButton(_kRB, p);
                          }),
                          SizedBox(height: triggerH - 40),
                        ],
                      ),
                      const SizedBox(width: 8),
                      TriggerSlider(
                        label: 'RT',
                        width: triggerW,
                        height: triggerH,
                        onChanged: (v) => widget.state.rightTrigger = v,
                      ),
                    ],
                  ),
                  const SizedBox(height: 8),
                  // Right stick
                  AnalogStick(
                    size: stickSize,
                    onChanged: (x, y) { widget.state.rightStickX = x; widget.state.rightStickY = y; },
                  ),
                ],
              ),
            ),

            // === ABXY cluster — right of center ===
            Positioned(
              right: w * 0.36,
              top: h * 0.1,
              child: FaceButtonCluster(
                buttonSize: faceButtonSize,
                pressed: _pressedFace,
                onChanged: _onFaceButton,
              ),
            ),
          ],
        ),
      ),
    );
  }
}
