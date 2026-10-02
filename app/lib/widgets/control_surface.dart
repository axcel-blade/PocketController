import 'dart:math' as math;
import 'package:flutter/material.dart';
import '../models/control_layout.dart';
import '../models/gamepad_state.dart';
import '../theme.dart';
import 'controls.dart';

/// Minimum on-screen touch target, in logical pixels.
const double kMinTarget = 44;

/// Resolves a placement to an on-screen rectangle inside [area], enforcing
/// minimum touch targets and keeping the control fully visible.
Rect placementRect(ControlPlacement p, Size area) {
  // Clusters need a larger box so each inner button/arm still meets the target size.
  final minBox = switch (p.kind) {
    ControlKind.faceButtons => kMinTarget / 0.36, // each face button is 36% of the cluster
    ControlKind.dpad => kMinTarget / 0.34 * 0.84, // arms are 34% wide
    ControlKind.leftStick || ControlKind.rightStick => kMinTarget * 2,
    _ => kMinTarget,
  };
  final s = p.size * area.height;
  final a = p.kind.aspect;
  final w = math.min(math.max(s * a.width, minBox), area.width);
  final h = math.min(math.max(s * a.height, minBox), area.height);
  final cx = (p.x * area.width).clamp(w / 2, area.width - w / 2);
  final cy = (p.y * area.height).clamp(h / 2, area.height - h / 2);
  return Rect.fromCenter(center: Offset(cx, cy), width: w, height: h);
}

/// Lays out every control of a [ControlLayout]. In edit mode controls are
/// inert and can be dragged; tapping one selects it for resizing.
class ControlSurface extends StatelessWidget {
  final ControlLayout layout;
  final GamepadState state;
  final bool editing;
  final ControlKind? selected;
  final ValueChanged<ControlKind>? onSelect;
  final ValueChanged<ControlPlacement>? onMove;

  const ControlSurface({
    super.key,
    required this.layout,
    required this.state,
    this.editing = false,
    this.selected,
    this.onSelect,
    this.onMove,
  });

  Widget _control(ControlKind kind) => switch (kind) {
        ControlKind.leftStick => AnalogStick(label: 'Left stick', onChanged: state.setLeftStick),
        ControlKind.rightStick => AnalogStick(label: 'Right stick', onChanged: state.setRightStick),
        ControlKind.dpad => DpadControl(state: state),
        ControlKind.faceButtons => FaceButtons(state: state),
        ControlKind.leftTrigger =>
          TriggerPad(label: 'LT', semanticLabel: 'Left trigger', onChanged: state.setLeftTrigger),
        ControlKind.rightTrigger =>
          TriggerPad(label: 'RT', semanticLabel: 'Right trigger', onChanged: state.setRightTrigger),
        ControlKind.leftBumper => PadButton(
            label: 'LB', semanticLabel: 'Left bumper', onChanged: (p) => state.setButton(Btn.lb, p)),
        ControlKind.rightBumper => PadButton(
            label: 'RB', semanticLabel: 'Right bumper', onChanged: (p) => state.setButton(Btn.rb, p)),
        ControlKind.back => PadButton(
            label: 'Back', semanticLabel: 'Back button', accent: PcColors.cyan,
            onChanged: (p) => state.setButton(Btn.back, p)),
        ControlKind.start => PadButton(
            label: 'Start', semanticLabel: 'Start button', accent: PcColors.cyan,
            onChanged: (p) => state.setButton(Btn.start, p)),
        ControlKind.home => PadButton(
            label: '⌂', semanticLabel: 'Home button', shape: PadShape.circle, fontSize: 22,
            accent: PcColors.cyan, onChanged: (p) => state.setButton(Btn.guide, p)),
      };

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(builder: (context, c) {
      final area = Size(c.maxWidth, c.maxHeight);
      return Stack(children: [
        for (final p in layout.controls)
          Positioned.fromRect(
            key: ValueKey(p.kind),
            rect: placementRect(p, area),
            child: editing ? _editable(p, area) : _control(p.kind),
          ),
      ]);
    });
  }

  Widget _editable(ControlPlacement p, Size area) {
    final isSelected = p.kind == selected;
    return Semantics(
      button: true,
      selected: isSelected,
      label: '${p.kind.label}. Drag to move, tap to select for resizing.',
      child: GestureDetector(
        behavior: HitTestBehavior.opaque,
        onTap: () => onSelect?.call(p.kind),
        onPanStart: (_) => onSelect?.call(p.kind),
        onPanUpdate: (d) => onMove?.call(p.copyWith(
          x: p.x + d.delta.dx / area.width,
          y: p.y + d.delta.dy / area.height,
        )),
        child: Stack(fit: StackFit.expand, children: [
          IgnorePointer(child: ExcludeSemantics(child: Opacity(opacity: 0.55, child: _control(p.kind)))),
          DecoratedBox(
            decoration: BoxDecoration(
              borderRadius: BorderRadius.circular(10),
              border: Border.all(
                color: isSelected ? PcColors.lime : PcColors.cyan.withValues(alpha: 0.6),
                width: isSelected ? 2 : 1,
              ),
              color: isSelected ? PcColors.lime.withValues(alpha: 0.08) : null,
            ),
          ),
        ]),
      ),
    );
  }
}
