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
        width: 64,
        height: 26,
        decoration: BoxDecoration(
          color: pressed ? Colors.white30 : Colors.white12,
          borderRadius: BorderRadius.circular(13),
          border: Border.all(color: Colors.white30),
        ),
        child: Center(
          child: Text(label, style: const TextStyle(color: Colors.white70, fontWeight: FontWeight.bold, fontSize: 11)),
        ),
      ),
    );
  }

  Widget _menuButton(String label, int bit) {
    final pressed = (widget.state.buttons & (1 << bit)) != 0;
    return GestureDetector(
      onTapDown: (_) { widget.state.setButton(bit, true); setState(() {}); },
      onTapUp: (_) { widget.state.setButton(bit, false); setState(() {}); },
      onTapCancel: () { widget.state.setButton(bit, false); setState(() {}); },
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 60),
        width: 40,
        height: 40,
        decoration: BoxDecoration(
          shape: BoxShape.circle,
          color: pressed ? Colors.white30 : Colors.white12,
          border: Border.all(color: Colors.white30),
        ),
        child: Center(
          child: Text(label, style: const TextStyle(color: Colors.white70, fontSize: 9, fontWeight: FontWeight.w600)),
        ),
      ),
    );
  }

  Widget _guideButton() {
    return Container(
      width: 56,
      height: 56,
      decoration: const BoxDecoration(
        color: Color(0xFF107C10),
        shape: BoxShape.circle,
      ),
      child: const Center(
        child: Text('X', style: TextStyle(color: Colors.white, fontSize: 26, fontWeight: FontWeight.bold)),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final size = MediaQuery.of(context).size;
    final h = size.height;

    final stickSize = h * 0.31;
    final dpadSize = h * 0.22;
    final faceButtonSize = h * 0.086;
    final triggerW = 34.0;
    final triggerH = h * 0.20;

    return Scaffold(
      backgroundColor: const Color(0xFF0E0E0E),
      body: SafeArea(
        child: LayoutBuilder(
          builder: (context, constraints) {
            final bodyW = constraints.maxWidth;
            final bodyH = constraints.maxHeight;
            const shoulderGap = 8.0;
            const centerButtonGap = 18.0;
            const menuButtonSize = 40.0;
            const guideButtonSize = 56.0;

            // Anchor controls by visual centers so the Xbox layout scales
            // consistently across different landscape screen sizes.
            final centerRowWidth = menuButtonSize * 2 + guideButtonSize + centerButtonGap * 2;
            final leftStickX = bodyW * 0.20;
            final leftStickY = bodyH * 0.40;
            final dpadX = bodyW * 0.17;
            final dpadY = bodyH * 0.72;
            final faceButtonsX = bodyW * 0.81;
            final faceButtonsY = bodyH * 0.37;
            final rightStickX = bodyW * 0.73;
            final rightStickY = bodyH * 0.72;

            return Stack(
              children: [
                // --- Shoulders: LB LT | RT RB (Xbox diagram order) ---
                Positioned(
                  top: bodyH * 0.03,
                  left: bodyW * 0.06,
                  child: Row(
                    children: [
                      _bumper('LB', _lbPressed, (p) {
                        setState(() => _lbPressed = p);
                        widget.state.setButton(_kLB, p);
                      }),
                      const SizedBox(width: shoulderGap),
                      TriggerSlider(
                        label: 'LT',
                        width: triggerW,
                        height: triggerH,
                        onChanged: (v) => widget.state.leftTrigger = v,
                      ),
                    ],
                  ),
                ),
                Positioned(
                  top: bodyH * 0.03,
                  right: bodyW * 0.06,
                  child: Row(
                    children: [
                      TriggerSlider(
                        label: 'RT',
                        width: triggerW,
                        height: triggerH,
                        onChanged: (v) => widget.state.rightTrigger = v,
                      ),
                      const SizedBox(width: shoulderGap),
                      _bumper('RB', _rbPressed, (p) {
                        setState(() => _rbPressed = p);
                        widget.state.setButton(_kRB, p);
                      }),
                    ],
                  ),
                ),

                // --- Center: Back | Guide | Start ---
                Positioned(
                  top: bodyH * 0.24,
                  left: (bodyW - centerRowWidth) / 2,
                  child: Row(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      _menuButton('Back', _kBack),
                      const SizedBox(width: centerButtonGap),
                      _guideButton(),
                      const SizedBox(width: centerButtonGap),
                      _menuButton('Start', _kStart),
                    ],
                  ),
                ),

                // --- Left stick (LSB) — upper-left ---
                Positioned(
                  left: leftStickX - stickSize / 2,
                  top: leftStickY - stickSize / 2,
                  child: AnalogStick(
                    size: stickSize,
                    onChanged: (x, y) {
                      widget.state.leftStickX = x;
                      widget.state.leftStickY = y;
                    },
                  ),
                ),

                // --- D-pad — below left stick, slightly toward center ---
                Positioned(
                  left: dpadX - dpadSize / 2,
                  top: dpadY - dpadSize / 2,
                  child: DpadWidget(
                    size: dpadSize,
                    onChanged: (bits) {
                      widget.state.dpad = bits;
                      setState(() {});
                    },
                  ),
                ),

                // --- ABXY — upper-right diamond ---
                Positioned(
                  left: faceButtonsX - (faceButtonSize * 2 + faceButtonSize * 0.12) / 2,
                  top: faceButtonsY - (faceButtonSize * 2 + faceButtonSize * 0.12) / 2,
                  child: FaceButtonCluster(
                    buttonSize: faceButtonSize,
                    pressed: _pressedFace,
                    onChanged: _onFaceButton,
                  ),
                ),

                // --- Right stick (RSB) — below ABXY, slightly toward center ---
                Positioned(
                  left: rightStickX - stickSize / 2,
                  top: rightStickY - stickSize / 2,
                  child: AnalogStick(
                    size: stickSize,
                    onChanged: (x, y) {
                      widget.state.rightStickX = x;
                      widget.state.rightStickY = y;
                    },
                  ),
                ),
              ],
            );
          },
        ),
      ),
    );
  }
}
