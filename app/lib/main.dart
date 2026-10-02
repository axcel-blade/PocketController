import 'package:flutter/material.dart';
import 'models/gamepad_state.dart';
import 'network/auto_connect.dart';
import 'network/bridge_connection.dart';
import 'network/discovery.dart';
import 'screens/controller_screen.dart';
import 'services/app_settings.dart';
import 'theme.dart';
import 'widgets/controls.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();
  final settings = await AppSettings.load();
  final state = GamepadState();
  final bridge = BridgeConnection(state);
  final discovery = BridgeDiscovery();
  final autoConnector = AutoConnector(settings: settings, bridge: bridge, discovery: discovery)..start();
  runApp(PocketControllerApp(
    settings: settings,
    bridge: bridge,
    state: state,
    discovery: discovery,
    autoConnector: autoConnector,
  ));
}

class PocketControllerApp extends StatefulWidget {
  final AppSettings settings;
  final BridgeConnection bridge;
  final GamepadState state;
  final BridgeDiscovery discovery;
  final AutoConnector autoConnector;

  const PocketControllerApp({
    super.key,
    required this.settings,
    required this.bridge,
    required this.state,
    required this.discovery,
    required this.autoConnector,
  });

  @override
  State<PocketControllerApp> createState() => _PocketControllerAppState();
}

class _PocketControllerAppState extends State<PocketControllerApp> {
  @override
  void initState() {
    super.initState();
    _syncHaptics();
    widget.settings.addListener(_syncHaptics);
  }

  @override
  void dispose() {
    widget.settings.removeListener(_syncHaptics);
    super.dispose();
  }

  void _syncHaptics() => Haptics.enabled = widget.settings.hapticsEnabled;

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'PocketController',
      debugShowCheckedModeBanner: false,
      theme: buildTheme(),
      home: ControllerScreen(
        settings: widget.settings,
        bridge: widget.bridge,
        state: widget.state,
        discovery: widget.discovery,
        autoConnector: widget.autoConnector,
      ),
    );
  }
}
