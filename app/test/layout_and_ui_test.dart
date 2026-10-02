import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:pocket_controller/models/control_layout.dart';
import 'package:pocket_controller/models/gamepad_state.dart';
import 'package:pocket_controller/services/app_settings.dart';
import 'package:pocket_controller/theme.dart';
import 'package:pocket_controller/widgets/control_surface.dart';
import 'package:shared_preferences/shared_preferences.dart';

Widget _surface(GamepadState state, {ControlLayout layout = BuiltInLayouts.classic}) => MaterialApp(
      theme: buildTheme(),
      home: Scaffold(body: ControlSurface(layout: layout, state: state)),
    );

void main() {
  group('Layouts', () {
    test('every built-in layout places every control exactly once', () {
      for (final l in BuiltInLayouts.all) {
        expect(l.controls.map((c) => c.kind).toSet(), ControlKind.values.toSet(), reason: l.name);
        expect(l.controls.length, ControlKind.values.length, reason: l.name);
      }
    });

    test('placements stay on screen with minimum touch targets on a small phone', () {
      const area = Size(568 - 24, 320 - 48 - 14); // iPhone SE (1st gen) landscape minus chrome
      for (final l in BuiltInLayouts.all) {
        for (final p in l.controls) {
          final r = placementRect(p, area);
          expect(r.left >= 0 && r.top >= 0 && r.right <= area.width && r.bottom <= area.height, isTrue,
              reason: '${l.name} ${p.kind}');
          expect(r.shortestSide, greaterThanOrEqualTo(kMinTarget), reason: '${l.name} ${p.kind}');
        }
      }
    });

    test('custom layout JSON round-trips and fills missing controls', () {
      final custom = BuiltInLayouts.racing
          .asCustom(id: 'custom.1', name: 'Mine')
          .withPlacement(const ControlPlacement(ControlKind.dpad, 0.2, 0.3, 0.25));
      final back = ControlLayout.fromJson(custom.toJson())!;
      expect(back.name, 'Mine');
      expect(back.placementOf(ControlKind.dpad).x, 0.2);

      final partial = ControlLayout.fromJson({'id': 'x', 'name': 'Partial', 'controls': []})!;
      expect(partial.controls.length, ControlKind.values.length);
    });
  });

  group('AppSettings', () {
    test('persists bridge address, haptics and custom layouts', () async {
      SharedPreferences.setMockInitialValues({});
      final s = await AppSettings.load();
      expect(s.selectedLayout.id, BuiltInLayouts.classic.id);
      await s.setBridge('192.168.1.20', 6000);
      await s.setHaptics(false);
      await s.saveCustomLayout(BuiltInLayouts.compact.asCustom(id: 'custom.a', name: 'Tiny'));

      final reloaded = await AppSettings.load();
      expect(reloaded.host, '192.168.1.20');
      expect(reloaded.port, 6000);
      expect(reloaded.hapticsEnabled, isFalse);
      expect(reloaded.selectedLayout.name, 'Tiny');
      expect(reloaded.isNameTaken('tiny'), isTrue);

      await reloaded.deleteCustomLayout('custom.a');
      final again = await AppSettings.load();
      expect(again.customLayouts, isEmpty);
      expect(again.selectedLayout.id, BuiltInLayouts.classic.id);
    });
  });

  group('Controls', () {
    setUp(() {
      final binding = TestWidgetsFlutterBinding.ensureInitialized();
      binding.platformDispatcher.views.first.physicalSize = const Size(844, 390) * 3;
      binding.platformDispatcher.views.first.devicePixelRatio = 3;
    });
    tearDown(() => TestWidgetsFlutterBinding.instance.platformDispatcher.views.first.reset());

    testWidgets('A button sends press then release', (tester) async {
      final state = GamepadState();
      await tester.pumpWidget(_surface(state));
      final g = await tester.startGesture(tester.getCenter(find.bySemanticsLabel('A button')));
      await tester.pump();
      expect(state.isPressed(Btn.a), isTrue);
      await g.up();
      await tester.pump();
      expect(state.isPressed(Btn.a), isFalse);
    });

    testWidgets('home, start, back and bumpers map to their bits', (tester) async {
      final state = GamepadState();
      await tester.pumpWidget(_surface(state));
      for (final (label, bit) in [
        ('Home button', Btn.guide),
        ('Start button', Btn.start),
        ('Back button', Btn.back),
        ('Left bumper', Btn.lb),
        ('Right bumper', Btn.rb),
      ]) {
        final g = await tester.startGesture(tester.getCenter(find.bySemanticsLabel(label)));
        await tester.pump();
        expect(state.isPressed(bit), isTrue, reason: label);
        await g.up();
        await tester.pump();
        expect(state.buttons, 0, reason: label);
      }
    });

    testWidgets('multi-touch: holding A and dragging the left stick together', (tester) async {
      final state = GamepadState();
      await tester.pumpWidget(_surface(state));
      final a = await tester.startGesture(tester.getCenter(find.bySemanticsLabel('A button')), pointer: 1);
      final stickCenter = tester.getCenter(find.bySemanticsLabel('Left stick'));
      final stick = await tester.startGesture(stickCenter, pointer: 2);
      await stick.moveBy(const Offset(200, -200));
      await tester.pump();
      expect(state.isPressed(Btn.a), isTrue);
      expect(state.leftStickX, greaterThan(0.6));
      expect(state.leftStickY, greaterThan(0.6)); // up is positive
      expect(state.leftStickX * state.leftStickX + state.leftStickY * state.leftStickY, lessThanOrEqualTo(1.0001));

      await stick.up();
      await tester.pump();
      expect([state.leftStickX, state.leftStickY], [0, 0]); // returns to centre
      await a.up();
    });

    testWidgets('D-pad diagonals and triggers', (tester) async {
      final state = GamepadState();
      await tester.pumpWidget(_surface(state));
      final dpad = tester.getRect(find.bySemanticsLabel('Directional pad'));
      final g = await tester.startGesture(dpad.center + Offset(dpad.width * 0.3, -dpad.height * 0.3));
      await tester.pump();
      expect(state.dpad, (1 << Dpad.up) | (1 << Dpad.right));
      await g.up();
      expect(state.dpad, 0);

      final rt = tester.getRect(find.bySemanticsLabel('Right trigger'));
      final t = await tester.startGesture(rt.bottomCenter - const Offset(0, 2));
      await tester.pump();
      expect(state.rightTrigger, greaterThan(0.95));
      await t.up();
      await tester.pump();
      expect(state.rightTrigger, 0);
    });

    testWidgets('every layout renders on a small phone without overflow', (tester) async {
      tester.view.physicalSize = const Size(568, 320) * 2;
      tester.view.devicePixelRatio = 2;
      for (final l in BuiltInLayouts.all) {
        await tester.pumpWidget(_surface(GamepadState(), layout: l));
        expect(tester.takeException(), isNull, reason: l.name);
      }
    });
  });
}
