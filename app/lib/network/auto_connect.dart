import 'dart:async';
import 'package:flutter/foundation.dart';
import '../services/app_settings.dart';
import 'bridge_connection.dart';
import 'discovery.dart';

/// Picks the server to join automatically, or null if the choice isn't obvious.
///
/// 1. The last server used (same address and port).
/// 2. The last server by name, if its address changed (e.g. a new DHCP lease).
/// 3. Only once the scan window has ended and nothing was ever saved: the single
///    server found, if it is the only one and has a free slot.
DiscoveredBridge? pickAutoConnectTarget(
  List<DiscoveredBridge> found, {
  required String savedHost,
  required int savedPort,
  required String savedName,
  required bool scanWindowOver,
}) {
  final open = found.where((b) => !b.isFull).toList();
  for (final b in open) {
    if (b.address == savedHost && b.port == savedPort) return b;
  }
  if (savedName.isNotEmpty) {
    for (final b in open) {
      if (b.name == savedName) return b;
    }
  }
  if (scanWindowOver && savedHost.isEmpty && found.length == 1 && open.length == 1) return open.single;
  return null;
}

/// Keeps discovery running while disconnected and, once per app launch, joins a
/// known server automatically when it appears. Manual entry always remains possible.
class AutoConnector {
  static const autoWindow = Duration(seconds: 8);

  final AppSettings settings;
  final BridgeConnection bridge;
  final BridgeDiscovery discovery;

  bool _armed = false;
  bool _windowOver = false;
  Timer? _windowTimer;

  AutoConnector({required this.settings, required this.bridge, required this.discovery});

  void start() {
    _armed = settings.autoConnect;
    _windowTimer = Timer(autoWindow, () {
      _windowOver = true;
      _tryAutoConnect();
      _armed = false; // only auto-join at launch; after that the user decides
    });
    bridge.addListener(_onBridgeChanged);
    discovery.addListener(_tryAutoConnect);
    _onBridgeChanged();
  }

  void _onBridgeChanged() {
    if (bridge.status == BridgeStatus.connected) {
      _armed = false;
      discovery.stop();
    } else if (bridge.status != BridgeStatus.connecting && !discovery.isScanning) {
      discovery.start();
    }
  }

  void _tryAutoConnect() {
    if (!_armed || bridge.status != BridgeStatus.disconnected) return;
    final target = pickAutoConnectTarget(
      discovery.bridges,
      savedHost: settings.host,
      savedPort: settings.port,
      savedName: settings.bridgeName,
      scanWindowOver: _windowOver,
    );
    if (target == null) return;
    _armed = false;
    connectTo(target);
  }

  /// Connects to a discovered server and remembers it for next time.
  Future<void> connectTo(DiscoveredBridge b) async {
    await settings.setBridge(b.address, b.port, name: b.name);
    await bridge.connect(b.address, b.port);
  }

  @visibleForTesting
  bool get isArmed => _armed;

  void dispose() {
    _windowTimer?.cancel();
    bridge.removeListener(_onBridgeChanged);
    discovery.removeListener(_tryAutoConnect);
  }
}
