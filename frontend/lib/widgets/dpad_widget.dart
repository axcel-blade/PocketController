import 'package:flutter/material.dart';

/// D-pad widget. Reports bit flags: Up=0, Down=1, Left=2, Right=3.
class DpadWidget extends StatefulWidget {
  final double size;
  final void Function(int dpadBits) onChanged;

  const DpadWidget({super.key, required this.size, required this.onChanged});

  @override
  State<DpadWidget> createState() => _DpadWidgetState();
}

class _DpadWidgetState extends State<DpadWidget> {
  int _bits = 0;

  void _update(Offset localPos) {
    final center = widget.size / 2;
    final dx = localPos.dx - center;
    final dy = localPos.dy - center;
    final arm = widget.size / 3;

    int bits = 0;
    if (dy < -arm * 0.5) bits |= (1 << 0); // up
    if (dy > arm * 0.5) bits |= (1 << 1);  // down
    if (dx < -arm * 0.5) bits |= (1 << 2); // left
    if (dx > arm * 0.5) bits |= (1 << 3);  // right

    if (bits != _bits) {
      setState(() => _bits = bits);
      widget.onChanged(bits);
    }
  }

  void _reset() {
    if (_bits != 0) {
      setState(() => _bits = 0);
      widget.onChanged(0);
    }
  }

  bool _isPressed(int bit) => (_bits & (1 << bit)) != 0;

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
        child: CustomPaint(
          painter: _DpadPainter(
            upPressed: _isPressed(0),
            downPressed: _isPressed(1),
            leftPressed: _isPressed(2),
            rightPressed: _isPressed(3),
          ),
        ),
      ),
    );
  }
}

class _DpadPainter extends CustomPainter {
  final bool upPressed, downPressed, leftPressed, rightPressed;

  const _DpadPainter({
    required this.upPressed,
    required this.downPressed,
    required this.leftPressed,
    required this.rightPressed,
  });

  @override
  void paint(Canvas canvas, Size size) {
    final cx = size.width / 2;
    final cy = size.height / 2;
    final arm = size.width / 3;
    final thickness = arm * 0.8;

    _drawArm(canvas, cx - thickness / 2, 0, thickness, arm, upPressed);           // up
    _drawArm(canvas, cx - thickness / 2, cy + arm * 0.2, thickness, arm, downPressed); // down
    _drawArm(canvas, 0, cy - thickness / 2, arm, thickness, leftPressed);          // left
    _drawArm(canvas, cx + arm * 0.2, cy - thickness / 2, arm, thickness, rightPressed); // right

    // Center square
    canvas.drawRect(
      Rect.fromCenter(center: Offset(cx, cy), width: thickness, height: thickness),
      Paint()..color = const Color(0xFF2A2A2A),
    );
  }

  void _drawArm(Canvas canvas, double x, double y, double w, double h, bool pressed) {
    final paint = Paint()..color = pressed ? const Color(0xFF555555) : const Color(0xFF2A2A2A);
    final borderPaint = Paint()
      ..color = const Color(0xFF444444)
      ..style = PaintingStyle.stroke
      ..strokeWidth = 1.5;
    final rect = RRect.fromRectAndRadius(Rect.fromLTWH(x, y, w, h), const Radius.circular(4));
    canvas.drawRRect(rect, paint);
    canvas.drawRRect(rect, borderPaint);
  }

  @override
  bool shouldRepaint(_DpadPainter old) =>
      old.upPressed != upPressed ||
      old.downPressed != downPressed ||
      old.leftPressed != leftPressed ||
      old.rightPressed != rightPressed;
}
