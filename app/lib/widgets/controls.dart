import 'dart:math' as math;
import 'package:flutter/material.dart';
import 'package:flutter/semantics.dart';
import 'package:flutter/services.dart';
import '../models/gamepad_state.dart';
import '../theme.dart';

/// Haptic feedback, globally switchable from settings.
class Haptics {
  static bool enabled = true;
  static void press() {
    if (enabled) HapticFeedback.lightImpact();
  }

  static void tick() {
    if (enabled) HapticFeedback.selectionClick();
  }

  static void strong() {
    if (enabled) HapticFeedback.mediumImpact();
  }
}

/// Multi-touch-safe press tracker. Uses raw pointer events (not gestures) so several
/// controls can be held at once without fighting in the gesture arena.
class _PressListener extends StatefulWidget {
  final ValueChanged<bool> onChanged;
  final Widget Function(bool pressed) builder;
  const _PressListener({required this.onChanged, required this.builder});

  @override
  State<_PressListener> createState() => _PressListenerState();
}

class _PressListenerState extends State<_PressListener> {
  final Set<int> _pointers = {};

  void _update(VoidCallback mutate) {
    final was = _pointers.isNotEmpty;
    mutate();
    final now = _pointers.isNotEmpty;
    if (was != now) {
      setState(() {});
      if (now) Haptics.press();
      widget.onChanged(now);
    }
  }

  @override
  void dispose() {
    if (_pointers.isNotEmpty) widget.onChanged(false);
    super.dispose();
  }

  @override
  Widget build(BuildContext context) => Listener(
        behavior: HitTestBehavior.opaque,
        onPointerDown: (e) => _update(() => _pointers.add(e.pointer)),
        onPointerUp: (e) => _update(() => _pointers.remove(e.pointer)),
        onPointerCancel: (e) => _update(() => _pointers.remove(e.pointer)),
        child: widget.builder(_pointers.isNotEmpty),
      );
}

enum PadShape { circle, pill }

/// A single digital button: sends press on touch-down and release on touch-up.
class PadButton extends StatelessWidget {
  final String label;
  final String semanticLabel;
  final PadShape shape;
  final Color accent;
  final ValueChanged<bool> onChanged;
  final double fontSize;

  const PadButton({
    super.key,
    required this.label,
    required this.semanticLabel,
    required this.onChanged,
    this.shape = PadShape.pill,
    this.accent = PcColors.lime,
    this.fontSize = 14,
  });

  @override
  Widget build(BuildContext context) {
    return Semantics(
      button: true,
      label: semanticLabel,
      onTap: () {
        onChanged(true);
        onChanged(false);
      },
      child: ExcludeSemantics(
        child: _PressListener(
          onChanged: onChanged,
          builder: (pressed) => AnimatedScale(
            scale: pressed ? 0.92 : 1,
            duration: const Duration(milliseconds: 60),
            child: AnimatedContainer(
              duration: const Duration(milliseconds: 70),
              decoration: BoxDecoration(
                color: pressed ? accent : PcColors.surfaceRaised,
                shape: shape == PadShape.circle ? BoxShape.circle : BoxShape.rectangle,
                borderRadius: shape == PadShape.pill ? BorderRadius.circular(999) : null,
                border: Border.all(color: pressed ? accent : PcColors.outline, width: 1.5),
                boxShadow: pressed
                    ? [BoxShadow(color: accent.withValues(alpha: 0.45), blurRadius: 14)]
                    : const [BoxShadow(color: Colors.black54, offset: Offset(0, 3), blurRadius: 4)],
              ),
              alignment: Alignment.center,
              child: FittedBox(
                child: Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 8),
                  child: Text(
                    label,
                    style: TextStyle(
                      color: pressed ? PcColors.onLime : PcColors.text,
                      fontWeight: FontWeight.w700,
                      fontSize: fontSize,
                      letterSpacing: 0.5,
                    ),
                  ),
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}

/// A/B/X/Y diamond. Each button is independent so chords (e.g. A+X) work.
class FaceButtons extends StatelessWidget {
  final GamepadState state;
  const FaceButtons({super.key, required this.state});

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(builder: (context, c) {
      final s = math.min(c.maxWidth, c.maxHeight);
      final d = s * 0.36;
      Widget btn(String l, int bit, Alignment a) => Align(
            alignment: a,
            child: SizedBox.square(
              dimension: d,
              child: PadButton(
                label: l,
                semanticLabel: '$l button',
                shape: PadShape.circle,
                fontSize: d * 0.38,
                onChanged: (p) => state.setButton(bit, p),
              ),
            ),
          );
      return SizedBox.square(
        dimension: s,
        child: Stack(children: [
          btn('Y', Btn.y, Alignment.topCenter),
          btn('X', Btn.x, Alignment.centerLeft),
          btn('B', Btn.b, Alignment.centerRight),
          btn('A', Btn.a, Alignment.bottomCenter),
        ]),
      );
    });
  }
}

/// Eight-way D-pad. Sliding the thumb between directions updates without lifting.
class DpadControl extends StatefulWidget {
  final GamepadState state;
  const DpadControl({super.key, required this.state});

  @override
  State<DpadControl> createState() => _DpadControlState();
}

class _DpadControlState extends State<DpadControl> {
  int _mask = 0;
  int? _pointer;

  void _set(int mask) {
    if (mask == _mask) return;
    if (mask != 0) Haptics.tick();
    setState(() => _mask = mask);
    widget.state.setDpad(mask);
  }

  int _maskFor(Offset local, double size) {
    final v = local - Offset(size / 2, size / 2);
    if (v.distance < size * 0.12) return 0;
    // 8 sectors of 45°, so diagonals press two directions.
    final angle = math.atan2(-v.dy, v.dx); // 0 = right, +90° = up
    final sector = ((angle / (math.pi / 4)).round() + 8) % 8;
    const masks = [
      1 << Dpad.right,
      1 << Dpad.right | 1 << Dpad.up,
      1 << Dpad.up,
      1 << Dpad.up | 1 << Dpad.left,
      1 << Dpad.left,
      1 << Dpad.left | 1 << Dpad.down,
      1 << Dpad.down,
      1 << Dpad.down | 1 << Dpad.right,
    ];
    return masks[sector];
  }

  @override
  void dispose() {
    if (_mask != 0) widget.state.setDpad(0);
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(builder: (context, c) {
      final s = math.min(c.maxWidth, c.maxHeight);
      void dir(int bit) {
        _set(1 << bit);
        _set(0);
      }

      return Semantics(
        label: 'Directional pad',
        hint: 'Slide between directions; diagonals press two',
        customSemanticsActions: {
          const CustomSemanticsAction(label: 'Up'): () => dir(Dpad.up),
          const CustomSemanticsAction(label: 'Down'): () => dir(Dpad.down),
          const CustomSemanticsAction(label: 'Left'): () => dir(Dpad.left),
          const CustomSemanticsAction(label: 'Right'): () => dir(Dpad.right),
        },
        child: Listener(
          behavior: HitTestBehavior.opaque,
          onPointerDown: (e) {
            if (_pointer != null) return;
            _pointer = e.pointer;
            _set(_maskFor(e.localPosition, s));
          },
          onPointerMove: (e) {
            if (e.pointer == _pointer) _set(_maskFor(e.localPosition, s));
          },
          onPointerUp: (e) {
            if (e.pointer != _pointer) return;
            _pointer = null;
            _set(0);
          },
          onPointerCancel: (e) {
            if (e.pointer != _pointer) return;
            _pointer = null;
            _set(0);
          },
          child: CustomPaint(size: Size.square(s), painter: _DpadPainter(_mask)),
        ),
      );
    });
  }
}

class _DpadPainter extends CustomPainter {
  final int mask;
  _DpadPainter(this.mask);

  @override
  void paint(Canvas canvas, Size size) {
    final s = size.width;
    final arm = s * 0.34;
    final c = Offset(s / 2, s / 2);
    canvas.drawCircle(c, s / 2, Paint()..color = PcColors.surface);
    canvas.drawCircle(c, s / 2 - 0.75, Paint()
      ..style = PaintingStyle.stroke
      ..color = PcColors.outline
      ..strokeWidth = 1.5);

    final rects = {
      Dpad.up: Rect.fromLTWH(c.dx - arm / 2, s * 0.08, arm, c.dy - s * 0.08),
      Dpad.down: Rect.fromLTWH(c.dx - arm / 2, c.dy, arm, c.dy - s * 0.08),
      Dpad.left: Rect.fromLTWH(s * 0.08, c.dy - arm / 2, c.dx - s * 0.08, arm),
      Dpad.right: Rect.fromLTWH(c.dx, c.dy - arm / 2, c.dx - s * 0.08, arm),
    };
    final base = Paint()..color = PcColors.surfaceRaised;
    final cross = Path()
      ..addRRect(RRect.fromRectAndRadius(rects[Dpad.up]!.expandToInclude(rects[Dpad.down]!), Radius.circular(arm * 0.22)))
      ..addRRect(RRect.fromRectAndRadius(rects[Dpad.left]!.expandToInclude(rects[Dpad.right]!), Radius.circular(arm * 0.22)));
    canvas.drawPath(cross, base);
    for (final e in rects.entries) {
      if (mask & (1 << e.key) != 0) {
        canvas.drawRRect(RRect.fromRectAndRadius(e.value, Radius.circular(arm * 0.22)), Paint()..color = PcColors.lime);
      }
    }
    // Direction chevrons.
    final chevron = Paint()
      ..color = PcColors.textDim
      ..style = PaintingStyle.stroke
      ..strokeWidth = math.max(1.5, s * 0.012)
      ..strokeCap = StrokeCap.round;
    for (final e in rects.entries) {
      final pressed = mask & (1 << e.key) != 0;
      chevron.color = pressed ? PcColors.onLime : PcColors.textDim;
      final angle = switch (e.key) { Dpad.up => -math.pi / 2, Dpad.down => math.pi / 2, Dpad.left => math.pi, _ => 0.0 };
      final dirV = Offset(math.cos(angle), math.sin(angle));
      final tip = c + dirV * (s * 0.34);
      final back = c + dirV * (s * 0.26);
      final perp = Offset(-dirV.dy, dirV.dx) * (s * 0.06);
      canvas.drawPath(Path()
        ..moveTo((back + perp).dx, (back + perp).dy)
        ..lineTo(tip.dx, tip.dy)
        ..lineTo((back - perp).dx, (back - perp).dy), chevron);
    }
  }

  @override
  bool shouldRepaint(_DpadPainter old) => old.mask != mask;
}

/// Analog stick. Reports normalized [-1, 1] axes (+Y up) and snaps back to centre on release.
class AnalogStick extends StatefulWidget {
  final String label;
  final void Function(double x, double y) onChanged;
  const AnalogStick({super.key, required this.label, required this.onChanged});

  @override
  State<AnalogStick> createState() => _AnalogStickState();
}

class _AnalogStickState extends State<AnalogStick> {
  Offset _thumb = Offset.zero; // normalized, screen orientation (+y down)
  int? _pointer;

  void _move(Offset local, double s) {
    final r = s / 2 - s * 0.16; // travel radius keeps the thumb inside the ring
    var v = (local - Offset(s / 2, s / 2)) / r;
    if (v.distance > 1) v = v / v.distance;
    setState(() => _thumb = v);
    widget.onChanged(v.dx, -v.dy);
  }

  void _release() {
    _pointer = null;
    setState(() => _thumb = Offset.zero);
    widget.onChanged(0, 0);
  }

  @override
  void dispose() {
    if (_pointer != null) widget.onChanged(0, 0);
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(builder: (context, c) {
      final s = math.min(c.maxWidth, c.maxHeight);
      final active = _pointer != null;
      final thumbD = s * 0.42;
      final travel = s / 2 - s * 0.16;
      return Semantics(
        label: widget.label,
        hint: 'Drag to move; returns to centre when released',
        excludeSemantics: true,
        child: Listener(
          behavior: HitTestBehavior.opaque,
          onPointerDown: (e) {
            if (_pointer != null) return;
            _pointer = e.pointer;
            Haptics.press();
            _move(e.localPosition, s);
          },
          onPointerMove: (e) {
            if (e.pointer == _pointer) _move(e.localPosition, s);
          },
          onPointerUp: (e) {
            if (e.pointer == _pointer) _release();
          },
          onPointerCancel: (e) {
            if (e.pointer == _pointer) _release();
          },
          child: SizedBox.square(
            dimension: s,
            child: Stack(alignment: Alignment.center, children: [
              Container(
                decoration: BoxDecoration(
                  shape: BoxShape.circle,
                  color: PcColors.surface,
                  border: Border.all(color: active ? PcColors.cyan : PcColors.outline, width: 1.5),
                ),
              ),
              // Travel ring
              Container(
                width: travel * 2,
                height: travel * 2,
                decoration: BoxDecoration(
                  shape: BoxShape.circle,
                  border: Border.all(color: PcColors.cyan.withValues(alpha: active ? 0.35 : 0.12)),
                ),
              ),
              Transform.translate(
                offset: _thumb * travel,
                child: AnimatedContainer(
                  duration: Duration(milliseconds: active ? 0 : 90),
                  width: thumbD,
                  height: thumbD,
                  decoration: BoxDecoration(
                    shape: BoxShape.circle,
                    color: active ? PcColors.lime : PcColors.surfaceRaised,
                    border: Border.all(color: active ? PcColors.lime : PcColors.outline, width: 1.5),
                    boxShadow: active
                        ? [BoxShadow(color: PcColors.lime.withValues(alpha: 0.4), blurRadius: 16)]
                        : const [BoxShadow(color: Colors.black54, offset: Offset(0, 3), blurRadius: 6)],
                  ),
                  alignment: Alignment.center,
                  child: Text(
                    widget.label.startsWith('Left') ? 'L' : 'R',
                    style: TextStyle(
                      color: active ? PcColors.onLime : PcColors.textDim,
                      fontWeight: FontWeight.w700,
                      fontSize: thumbD * 0.3,
                    ),
                  ),
                ),
              ),
            ]),
          ),
        ),
      );
    });
  }
}

/// Analog trigger pad. Touch anywhere to pull; touching lower pulls harder
/// (top edge ≈ 25%, bottom edge = 100%). Releases to 0.
class TriggerPad extends StatefulWidget {
  final String label;
  final String semanticLabel;
  final ValueChanged<double> onChanged;
  const TriggerPad({super.key, required this.label, required this.semanticLabel, required this.onChanged});

  @override
  State<TriggerPad> createState() => _TriggerPadState();
}

class _TriggerPadState extends State<TriggerPad> {
  static const minPull = 0.25;
  double _value = 0;
  int? _pointer;

  void _set(double v) {
    if (v == _value) return;
    if (_value == 0 && v > 0) Haptics.strong();
    setState(() => _value = v);
    widget.onChanged(v);
  }

  double _valueAt(Offset local, double h) => minPull + (1 - minPull) * (local.dy / h).clamp(0.0, 1.0);

  @override
  void dispose() {
    if (_value != 0) widget.onChanged(0);
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(builder: (context, c) {
      final h = c.maxHeight;
      final active = _value > 0;
      return Semantics(
        button: true,
        label: widget.semanticLabel,
        hint: 'Press lower for a stronger pull',
        value: '${(_value * 100).round()} percent',
        excludeSemantics: true,
        onTap: () {
          widget.onChanged(1);
          widget.onChanged(0);
        },
        child: Listener(
          behavior: HitTestBehavior.opaque,
          onPointerDown: (e) {
            if (_pointer != null) return;
            _pointer = e.pointer;
            _set(_valueAt(e.localPosition, h));
          },
          onPointerMove: (e) {
            if (e.pointer == _pointer) _set(_valueAt(e.localPosition, h));
          },
          onPointerUp: (e) {
            if (e.pointer != _pointer) return;
            _pointer = null;
            _set(0);
          },
          onPointerCancel: (e) {
            if (e.pointer != _pointer) return;
            _pointer = null;
            _set(0);
          },
          child: ClipRRect(
            borderRadius: BorderRadius.circular(14),
            child: Container(
              decoration: BoxDecoration(
                color: PcColors.surface,
                borderRadius: BorderRadius.circular(14),
                border: Border.all(color: active ? PcColors.lime : PcColors.outline, width: 1.5),
              ),
              child: Stack(children: [
                Align(
                  alignment: Alignment.topCenter,
                  child: FractionallySizedBox(
                    heightFactor: _value,
                    widthFactor: 1,
                    child: Container(color: PcColors.lime.withValues(alpha: 0.85)),
                  ),
                ),
                Center(
                  child: FittedBox(
                    child: Padding(
                      padding: const EdgeInsets.all(6),
                      child: Text(
                        widget.label,
                        style: TextStyle(
                          color: active ? PcColors.onLime : PcColors.text,
                          fontWeight: FontWeight.w800,
                          fontSize: 16,
                        ),
                      ),
                    ),
                  ),
                ),
                // Pull-depth ticks
                Positioned(
                  right: 6,
                  top: 6,
                  bottom: 6,
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: List.generate(
                      4,
                      (_) => Container(width: 5, height: 1.5, color: PcColors.cyan.withValues(alpha: 0.5)),
                    ),
                  ),
                ),
              ]),
            ),
          ),
        ),
      );
    });
  }
}
