import 'package:flutter/material.dart';

/// Circular joystick widget that reports X/Y values in [-1.0, 1.0].
class AnalogStick extends StatefulWidget {
  final double size;
  final void Function(double x, double y) onChanged;

  const AnalogStick({super.key, required this.size, required this.onChanged});

  @override
  State<AnalogStick> createState() => _AnalogStickState();
}

class _AnalogStickState extends State<AnalogStick> {
  Offset _thumb = Offset.zero;

  double get _radius => widget.size / 2;
  double get _thumbRadius => widget.size * 0.18;

  void _update(Offset localPos) {
    final center = Offset(_radius, _radius);
    final delta = localPos - center;
    final dist = delta.distance;
    final clamped = dist > _radius ? delta / dist * _radius : delta;
    setState(() => _thumb = clamped);
    widget.onChanged(clamped.dx / _radius, -clamped.dy / _radius);
  }

  void _reset() {
    setState(() => _thumb = Offset.zero);
    widget.onChanged(0, 0);
  }

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onPanStart: (d) => _update(d.localPosition),
      onPanUpdate: (d) => _update(d.localPosition),
      onPanEnd: (_) => _reset(),
      onPanCancel: _reset,
      child: SizedBox(
        width: widget.size,
        height: widget.size,
        child: CustomPaint(painter: _StickPainter(_thumb, _radius, _thumbRadius)),
      ),
    );
  }
}

class _StickPainter extends CustomPainter {
  final Offset thumb;
  final double radius;
  final double thumbRadius;

  const _StickPainter(this.thumb, this.radius, this.thumbRadius);

  @override
  void paint(Canvas canvas, Size size) {
    final center = Offset(radius, radius);

    // Outer ring
    canvas.drawCircle(
      center,
      radius,
      Paint()
        ..color = Colors.white.withValues(alpha:0.12)
        ..style = PaintingStyle.fill,
    );
    canvas.drawCircle(
      center,
      radius,
      Paint()
        ..color = Colors.white.withValues(alpha:0.3)
        ..style = PaintingStyle.stroke
        ..strokeWidth = 2,
    );

    // Cross lines
    final linePaint = Paint()
      ..color = Colors.white.withValues(alpha:0.15)
      ..strokeWidth = 1;
    canvas.drawLine(Offset(center.dx, center.dy - radius), Offset(center.dx, center.dy + radius), linePaint);
    canvas.drawLine(Offset(center.dx - radius, center.dy), Offset(center.dx + radius, center.dy), linePaint);

    // Thumb
    final thumbCenter = center + thumb;
    canvas.drawCircle(
      thumbCenter,
      thumbRadius,
      Paint()
        ..shader = RadialGradient(colors: [Colors.grey.shade400, Colors.grey.shade700]).createShader(
          Rect.fromCircle(center: thumbCenter, radius: thumbRadius),
        ),
    );
    canvas.drawCircle(
      thumbCenter,
      thumbRadius,
      Paint()
        ..color = Colors.white.withValues(alpha:0.4)
        ..style = PaintingStyle.stroke
        ..strokeWidth = 1.5,
    );
  }

  @override
  bool shouldRepaint(_StickPainter old) => old.thumb != thumb;
}
