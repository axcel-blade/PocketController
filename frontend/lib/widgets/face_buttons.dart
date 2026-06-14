import 'package:flutter/material.dart';

/// Single face button (A/B/X/Y) with press state.
class FaceButton extends StatelessWidget {
  final String label;
  final Color color;
  final double size;
  final bool pressed;
  final void Function(bool) onChanged;

  const FaceButton({
    super.key,
    required this.label,
    required this.color,
    required this.size,
    required this.pressed,
    required this.onChanged,
  });

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTapDown: (_) => onChanged(true),
      onTapUp: (_) => onChanged(false),
      onTapCancel: () => onChanged(false),
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 60),
        width: size,
        height: size,
        decoration: BoxDecoration(
          shape: BoxShape.circle,
          color: pressed ? color.withValues(alpha:0.9) : color.withValues(alpha:0.3),
          border: Border.all(color: color, width: 2),
          boxShadow: pressed
              ? [BoxShadow(color: color.withValues(alpha:0.6), blurRadius: 10, spreadRadius: 2)]
              : [],
        ),
        child: Center(
          child: Text(
            label,
            style: TextStyle(
              color: Colors.white,
              fontWeight: FontWeight.bold,
              fontSize: size * 0.38,
            ),
          ),
        ),
      ),
    );
  }
}

/// ABXY cluster in Xbox layout (Y top, X left, B right, A bottom).
class FaceButtonCluster extends StatelessWidget {
  final double buttonSize;
  final Set<String> pressed;
  final void Function(String button, bool isPressed) onChanged;

  const FaceButtonCluster({
    super.key,
    required this.buttonSize,
    required this.pressed,
    required this.onChanged,
  });

  @override
  Widget build(BuildContext context) {
    final gap = buttonSize * 0.2;
    final total = buttonSize * 3 + gap * 2;

    return SizedBox(
      width: total,
      height: total,
      child: Stack(
        alignment: Alignment.center,
        children: [
          // Y — top center
          Positioned(
            top: 0,
            left: total / 2 - buttonSize / 2,
            child: FaceButton(
              label: 'Y', color: const Color(0xFFFFD700),
              size: buttonSize, pressed: pressed.contains('Y'),
              onChanged: (p) => onChanged('Y', p),
            ),
          ),
          // X — middle left
          Positioned(
            top: total / 2 - buttonSize / 2,
            left: 0,
            child: FaceButton(
              label: 'X', color: const Color(0xFF4A9EFF),
              size: buttonSize, pressed: pressed.contains('X'),
              onChanged: (p) => onChanged('X', p),
            ),
          ),
          // B — middle right
          Positioned(
            top: total / 2 - buttonSize / 2,
            right: 0,
            child: FaceButton(
              label: 'B', color: const Color(0xFFFF4444),
              size: buttonSize, pressed: pressed.contains('B'),
              onChanged: (p) => onChanged('B', p),
            ),
          ),
          // A — bottom center
          Positioned(
            bottom: 0,
            left: total / 2 - buttonSize / 2,
            child: FaceButton(
              label: 'A', color: const Color(0xFF4CAF50),
              size: buttonSize, pressed: pressed.contains('A'),
              onChanged: (p) => onChanged('A', p),
            ),
          ),
        ],
      ),
    );
  }
}
