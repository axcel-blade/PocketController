import 'dart:ui';

/// Every control a layout can place. All layouts include every control.
enum ControlKind {
  leftTrigger('LT', 'Left trigger'),
  leftBumper('LB', 'Left bumper'),
  rightTrigger('RT', 'Right trigger'),
  rightBumper('RB', 'Right bumper'),
  leftStick('L', 'Left stick'),
  rightStick('R', 'Right stick'),
  dpad('D-pad', 'Directional pad'),
  faceButtons('ABXY', 'Face buttons'),
  back('Back', 'Back'),
  home('Home', 'Home'),
  start('Start', 'Start');

  final String shortLabel;
  final String label;
  const ControlKind(this.shortLabel, this.label);

  /// Bounding box as a multiple of the placement's `size` (width, height).
  Size get aspect => switch (this) {
        ControlKind.leftTrigger || ControlKind.rightTrigger => const Size(1.5, 1.0),
        ControlKind.leftBumper || ControlKind.rightBumper => const Size(1.8, 0.62),
        ControlKind.back || ControlKind.start => const Size(1.5, 0.75),
        _ => const Size(1, 1),
      };
}

/// Where a control sits. [x]/[y] are the centre as a fraction of the control area;
/// [size] is a fraction of the area's height (the shorter side in landscape).
class ControlPlacement {
  final ControlKind kind;
  final double x;
  final double y;
  final double size;

  const ControlPlacement(this.kind, this.x, this.y, this.size);

  static const double minSize = 0.08;
  static const double maxSize = 0.6;

  ControlPlacement copyWith({double? x, double? y, double? size}) => ControlPlacement(
        kind,
        (x ?? this.x).clamp(0.0, 1.0),
        (y ?? this.y).clamp(0.0, 1.0),
        (size ?? this.size).clamp(minSize, maxSize),
      );

  Map<String, dynamic> toJson() => {'kind': kind.name, 'x': x, 'y': y, 'size': size};

  static ControlPlacement? fromJson(Map<String, dynamic> j) {
    final kind = ControlKind.values.where((k) => k.name == j['kind']).firstOrNull;
    final x = j['x'], y = j['y'], size = j['size'];
    if (kind == null || x is! num || y is! num || size is! num) return null;
    return ControlPlacement(kind, 0, 0, minSize).copyWith(x: x.toDouble(), y: y.toDouble(), size: size.toDouble());
  }
}

class ControlLayout {
  final String id;
  final String name;
  final bool builtIn;
  final List<ControlPlacement> controls;

  const ControlLayout({required this.id, required this.name, required this.controls, this.builtIn = false});

  ControlPlacement placementOf(ControlKind kind) => controls.firstWhere((c) => c.kind == kind);

  ControlLayout withPlacement(ControlPlacement p) => ControlLayout(
        id: id,
        name: name,
        builtIn: builtIn,
        controls: [for (final c in controls) c.kind == p.kind ? p : c],
      );

  ControlLayout asCustom({required String id, required String name}) =>
      ControlLayout(id: id, name: name, controls: controls);

  Map<String, dynamic> toJson() => {
        'id': id,
        'name': name,
        'controls': [for (final c in controls) c.toJson()],
      };

  /// Parses a saved custom layout. Missing controls are filled from Classic so
  /// every control stays usable even if the saved data is from an older version.
  static ControlLayout? fromJson(Map<String, dynamic> j) {
    final id = j['id'], name = j['name'], list = j['controls'];
    if (id is! String || name is! String || list is! List) return null;
    final parsed = <ControlKind, ControlPlacement>{};
    for (final item in list) {
      if (item is Map<String, dynamic>) {
        final p = ControlPlacement.fromJson(item);
        if (p != null) parsed[p.kind] = p;
      }
    }
    return ControlLayout(
      id: id,
      name: name,
      controls: [for (final c in BuiltInLayouts.classic.controls) parsed[c.kind] ?? c],
    );
  }
}

class BuiltInLayouts {
  static const classic = ControlLayout(id: 'builtin.classic', name: 'Classic', builtIn: true, controls: [
    ControlPlacement(ControlKind.leftTrigger, 0.08, 0.12, 0.18),
    ControlPlacement(ControlKind.leftBumper, 0.24, 0.10, 0.15),
    ControlPlacement(ControlKind.rightTrigger, 0.92, 0.12, 0.18),
    ControlPlacement(ControlKind.rightBumper, 0.76, 0.10, 0.15),
    ControlPlacement(ControlKind.leftStick, 0.16, 0.56, 0.40),
    ControlPlacement(ControlKind.dpad, 0.36, 0.80, 0.28),
    ControlPlacement(ControlKind.faceButtons, 0.84, 0.54, 0.40),
    ControlPlacement(ControlKind.rightStick, 0.64, 0.80, 0.30),
    ControlPlacement(ControlKind.back, 0.41, 0.40, 0.12),
    ControlPlacement(ControlKind.home, 0.50, 0.40, 0.14),
    ControlPlacement(ControlKind.start, 0.59, 0.40, 0.12),
  ]);

  /// Big analog triggers under the thumbs for throttle/brake, steering on the left stick.
  static const racing = ControlLayout(id: 'builtin.racing', name: 'Racing', builtIn: true, controls: [
    ControlPlacement(ControlKind.leftTrigger, 0.38, 0.78, 0.30),
    ControlPlacement(ControlKind.leftBumper, 0.12, 0.10, 0.14),
    ControlPlacement(ControlKind.rightTrigger, 0.87, 0.72, 0.34),
    ControlPlacement(ControlKind.rightBumper, 0.88, 0.10, 0.14),
    ControlPlacement(ControlKind.leftStick, 0.15, 0.58, 0.44),
    ControlPlacement(ControlKind.dpad, 0.36, 0.40, 0.22),
    ControlPlacement(ControlKind.faceButtons, 0.68, 0.40, 0.38),
    ControlPlacement(ControlKind.rightStick, 0.62, 0.81, 0.26),
    ControlPlacement(ControlKind.back, 0.40, 0.10, 0.12),
    ControlPlacement(ControlKind.home, 0.50, 0.10, 0.13),
    ControlPlacement(ControlKind.start, 0.60, 0.10, 0.12),
  ]);

  /// Smaller controls with more breathing room, for narrow or short phones.
  static const compact = ControlLayout(id: 'builtin.compact', name: 'Compact', builtIn: true, controls: [
    ControlPlacement(ControlKind.leftTrigger, 0.07, 0.10, 0.14),
    ControlPlacement(ControlKind.leftBumper, 0.21, 0.09, 0.13),
    ControlPlacement(ControlKind.rightTrigger, 0.93, 0.10, 0.14),
    ControlPlacement(ControlKind.rightBumper, 0.79, 0.09, 0.13),
    ControlPlacement(ControlKind.leftStick, 0.14, 0.48, 0.34),
    ControlPlacement(ControlKind.dpad, 0.31, 0.80, 0.26),
    ControlPlacement(ControlKind.faceButtons, 0.86, 0.48, 0.34),
    ControlPlacement(ControlKind.rightStick, 0.69, 0.80, 0.28),
    ControlPlacement(ControlKind.back, 0.42, 0.50, 0.11),
    ControlPlacement(ControlKind.home, 0.50, 0.50, 0.12),
    ControlPlacement(ControlKind.start, 0.58, 0.50, 0.11),
  ]);

  static const all = [classic, racing, compact];
}
