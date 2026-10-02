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

/// Keeps discovery running while disconnected and joins the user's known server
/// automatically whenever it appears: at launch, when the server is started after the
/// app, and after the connection drops (server restart, Wi‑Fi blip).
///
/// It stays out of the way when the user is in control: pressing Disconnect pauses it
/// until the user connects again or relaunches the app. Manual entry always works.
class AutoConnector {
  /// How long after launch an unknown-but-only server may be joined (rule 3).
  static const firstRunWindow = Duration(seconds: 8);

  /// Minimum time between automatic attempts, so a server that keeps failing
  /// isn't hammered and the user can still read the error.
  static const retryDelay = Duration(seconds: 5);

  final AppSettings settings;
  final BridgeConnection bridge;
  final BridgeDiscovery discovery;

  bool _pausedByUser = false;
  bool _windowOver = false;
  DateTime _lastAttempt = DateTime.fromMillisecondsSinceEpoch(0);
  Timer? _windowTimer;
  Timer? _retryTimer;

  AutoConnector({required this.settings, required this.bridge, required this.discovery});

  void start() {
    _windowTimer = Timer(firstRunWindow, () {
      _windowOver = true;
      _tryAutoConnect();
    });
    bridge.addListener(_onBridgeChanged);
    discovery.addListener(_tryAutoConnect);
    settings.addListener(_tryAutoConnect);
    _onBridgeChanged();
  }

  bool get _enabled => settings.autoConnect && !_pausedByUser;

  void _onBridgeChanged() {
    if (bridge.status == BridgeStatus.connected) {
      discovery.stop();
    } else if (bridge.status != BridgeStatus.connecting && !discovery.isScanning) {
      discovery.start();
    }
    _tryAutoConnect();
  }

  void _tryAutoConnect() {
    if (!_enabled) return;
    if (bridge.status == BridgeStatus.connected || bridge.status == BridgeStatus.connecting) return;

    final wait = retryDelay - DateTime.now().difference(_lastAttempt);
    if (wait > Duration.zero) {
      // Try again once the delay has passed, even if discovery stays quiet meanwhile.
      _retryTimer ??= Timer(wait, () {
        _retryTimer = null;
        _tryAutoConnect();
      });
      return;
    }

    final target = pickAutoConnectTarget(
      discovery.bridges,
      savedHost: settings.host,
      savedPort: settings.port,
      savedName: settings.bridgeName,
      // Joining a server we've never used is only done once, right after launch.
      scanWindowOver: _windowOver && _lastAttempt.millisecondsSinceEpoch == 0,
    );
    if (target == null) return;
    _lastAttempt = DateTime.now();
    connectTo(target, auto: true);
  }

  /// Connects to a discovered server and remembers it for next time.
  Future<void> connectTo(DiscoveredBridge b, {bool auto = false}) async {
    if (!auto) _pausedByUser = false;
    await settings.setBridge(b.address, b.port, name: b.name);
    await bridge.connect(b.address, b.port);
  }

  /// The user connected by typing an address: resume auto-reconnect for that server.
  void noteManualConnect() => _pausedByUser = false;

  /// The user pressed Disconnect/Cancel: don't reconnect behind their back.
  void disconnect() {
    _pausedByUser = true;
    _retryTimer?.cancel();
    _retryTimer = null;
    bridge.disconnect();
  }

  @visibleForTesting
  bool get isPaused => _pausedByUser;

  void dispose() {
    _windowTimer?.cancel();
    _retryTimer?.cancel();
    bridge.removeListener(_onBridgeChanged);
    discovery.removeListener(_tryAutoConnect);
    settings.removeListener(_tryAutoConnect);
  }
}
