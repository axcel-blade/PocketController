import 'package:flutter/material.dart';

/// Vertical slider for LT/RT triggers. Reports 0.0 (released) to 1.0 (full press).
class TriggerSlider extends StatefulWidget {
  final String label;
  final double width;
  final double height;
  final void Function(double value) onChanged;

  const TriggerSlider({
    super.key,
    required this.label,
    required this.width,
    required this.height,
    required this.onChanged,
  });

  @override
  State<TriggerSlider> createState() => _TriggerSliderState();
}

class _TriggerSliderState extends State<TriggerSlider> {
  double _value = 0.0;

  void _update(Offset localPos) {
    final clamped = (localPos.dy / widget.height).clamp(0.0, 1.0);
    setState(() => _value = clamped);
    widget.onChanged(clamped);
  }

  void _reset() {
    setState(() => _value = 0.0);
    widget.onChanged(0.0);
  }

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onPanStart: (d) => _update(d.localPosition),
      onPanUpdate: (d) => _update(d.localPosition),
      onPanEnd: (_) => _reset(),
      onPanCancel: _reset,
      child: SizedBox(
        width: widget.width,
        height: widget.height,
        child: Column(
          children: [
            Text(widget.label, style: const TextStyle(color: Colors.white70, fontSize: 10)),
            const SizedBox(height: 2),
            Expanded(
              child: Container(
                decoration: BoxDecoration(
                  borderRadius: BorderRadius.circular(6),
                  color: Colors.white.withValues(alpha:0.08),
                  border: Border.all(color: Colors.white30),
                ),
                child: Stack(
                  alignment: Alignment.topCenter,
                  children: [
                    FractionallySizedBox(
                      heightFactor: _value,
                      child: Container(
                        decoration: BoxDecoration(
                          borderRadius: BorderRadius.circular(6),
                          gradient: LinearGradient(
                            begin: Alignment.topCenter,
                            end: Alignment.bottomCenter,
                            colors: [Colors.white60, Colors.grey.shade600],
                          ),
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
