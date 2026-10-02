import 'dart:async';
import 'dart:convert';
import 'dart:io';
import 'package:flutter/foundation.dart';

/// A PocketController server found on the local network.
class DiscoveredBridge {
  /// Per-run server ID ('' for servers that don't send one).
  final String id;
  final String name;

  /// The address the app will connect to (see [chooseAddress]).
  final String address;

  /// Every address the server reported (its adapters, plus where its packets came from).
  final List<String> addresses;
  final int port;
  final int clients;
  final int max;
  final DateTime lastSeen;

  const DiscoveredBridge({
    this.id = '',
    required this.name,
    required this.address,
    this.addresses = const [],
    required this.port,
    required this.clients,
    required this.max,
    required this.lastSeen,
  });

  /// Identity of the server: its ID, so a PC heard via several adapters is listed once.
  String get key => id.isNotEmpty ? id : endpoint;

  /// Where the app connects.
  String get endpoint => '$address:$port';

  bool get isFull => clients >= max;

  DiscoveredBridge copyWith({String? address, List<String>? addresses}) => DiscoveredBridge(
        id: id,
        name: name,
        address: address ?? this.address,
        addresses: addresses ?? this.addresses,
        port: port,
        clients: clients,
        max: max,
        lastSeen: lastSeen,
      );
}

/// Picks the server address the phone can actually reach: one on the same /24 subnet as
/// one of the phone's own addresses. A PC with WSL/VMware/VirtualBox adapters also reports
/// addresses the phone can't route to. Falls back to [previous], then to [source]
/// (where the packet came from, which is reachable by definition).
String chooseAddress({
  required String source,
  required Iterable<String> reported,
  required Set<String> localPrefixes,
  String? previous,
}) {
  String prefix(String ip) {
    final i = ip.lastIndexOf('.');
    return i < 0 ? ip : ip.substring(0, i);
  }

  final candidates = [source, ...reported.where((a) => a != source)];
  for (final a in candidates) {
    if (localPrefixes.contains(prefix(a))) return a;
  }
  return previous ?? source;
}

/// Wire format for discovery packets. Must match server Protocol/DiscoveryMessage.cs.
class DiscoveryProtocol {
  static const discoveryPort = 5556; // server listens here for probes
  static const announcePort = 5557; // phones listen here for broadcasts
  static const probePrefix = 'PCTRL?1|';
  static const announcePrefix = 'PCTRL!1|';

  static List<int> probe(String deviceName) =>
      utf8.encode('$probePrefix${deviceName.replaceAll('|', ' ')}');

  /// Parses an announcement sent from [address]. Returns null for anything else.
  static DiscoveredBridge? parseAnnouncement(List<int> data, String address) {
    if (data.length > 1024) return null;
    final String text;
    try {
      text = utf8.decode(data);
    } on FormatException {
      return null;
    }
    if (!text.startsWith(announcePrefix)) return null;
    try {
      final j = jsonDecode(text.substring(announcePrefix.length));
      if (j is! Map<String, dynamic>) return null;
      final name = j['name'], port = j['port'], clients = j['clients'], max = j['max'];
      if (name is! String || port is! int || clients is! int || max is! int) return null;
      if (port < 1 || port > 65535) return null;
      final id = j['id'], ips = j['ips'];
      return DiscoveredBridge(
        id: id is String ? id : '',
        name: name.isEmpty ? address : name,
        address: address,
        addresses: [
          address,
          if (ips is List)
            for (final ip in ips)
              if (ip is String && ip != address && InternetAddress.tryParse(ip) != null) ip,
        ],
        port: port,
        clients: clients,
        max: max,
        lastSeen: DateTime.now(),
      );
    } on FormatException {
      return null;
    }
  }
}

/// Finds PocketController servers on the LAN.
///
/// Two complementary mechanisms, because broadcast support varies by phone and network:
/// * Probes: sent to the broadcast address and to every host in this phone's /24
///   subnet (unicast works even where broadcasts are blocked, e.g. iOS without the
///   multicast entitlement). Servers reply directly.
/// * Announcements: servers broadcast every 2 s; we listen on [DiscoveryProtocol.announcePort].
class BridgeDiscovery extends ChangeNotifier {
  static const probeInterval = Duration(seconds: 2);
  static const staleAfter = Duration(seconds: 7);

  final String deviceName;

  /// Overrides for tests: where to send probes, and which ports to use.
  final Future<List<InternetAddress>> Function()? targetsOverride;
  final int discoveryPort;
  final int? announcePort;

  final Map<String, DiscoveredBridge> _found = {};

  /// First three octets of this phone's IPv4 addresses, e.g. "192.168.0".
  final Set<String> _localPrefixes = {};
  RawDatagramSocket? _probeSocket;
  RawDatagramSocket? _announceSocket;
  Timer? _probeTimer;
  Timer? _stopTimer;
  int _round = 0;
  bool _scanning = false;

  BridgeDiscovery({
    String? deviceName,
    this.targetsOverride,
    this.discoveryPort = DiscoveryProtocol.discoveryPort,
    this.announcePort = DiscoveryProtocol.announcePort,
  }) : deviceName = deviceName ?? defaultDeviceName();

  bool get isScanning => _scanning;

  /// Servers seen recently, sorted by name.
  List<DiscoveredBridge> get bridges {
    final cutoff = DateTime.now().subtract(staleAfter);
    return _found.values.where((b) => b.lastSeen.isAfter(cutoff)).toList()
      ..sort((a, b) => a.name.toLowerCase().compareTo(b.name.toLowerCase()));
  }

  /// Starts scanning. Runs until [stop], or for [duration] if given.
  Future<void> start({Duration? duration}) async {
    _stopTimer?.cancel();
    if (duration != null) _stopTimer = Timer(duration, stop);
    if (_scanning) return;
    _scanning = true;
    notifyListeners();

    try {
      final probe = await RawDatagramSocket.bind(InternetAddress.anyIPv4, 0);
      probe.broadcastEnabled = true;
      probe.listen((e) => _onEvent(probe, e));
      _probeSocket = probe;
    } on SocketException {
      _scanning = false;
      notifyListeners();
      return;
    }

    final listenPort = announcePort;
    if (listenPort != null) {
      try {
        final announce = await RawDatagramSocket.bind(InternetAddress.anyIPv4, listenPort, reuseAddress: true);
        announce.listen((e) => _onEvent(announce, e));
        _announceSocket = announce;
      } on SocketException {
        // Port busy or broadcasts unavailable; probes alone still work.
      }
    }

    if (!_scanning) {
      _closeSockets();
      return;
    }
    _round = 0;
    await _sendProbes();
    _probeTimer = Timer.periodic(probeInterval, (_) => _sendProbes());
  }

  void stop() {
    _stopTimer?.cancel();
    _stopTimer = null;
    if (!_scanning) return;
    _scanning = false;
    _probeTimer?.cancel();
    _probeTimer = null;
    _closeSockets();
    notifyListeners();
  }

  /// Forgets previous results and scans again.
  Future<void> rescan({Duration? duration}) async {
    _found.clear();
    stop();
    await start(duration: duration);
  }

  void _closeSockets() {
    _probeSocket?.close();
    _announceSocket?.close();
    _probeSocket = _announceSocket = null;
  }

  Future<void> _sendProbes() async {
    final socket = _probeSocket;
    if (socket == null) return;
    final packet = DiscoveryProtocol.probe(deviceName);
    // Full subnet sweep on every other round keeps traffic low while still finding
    // servers on networks that drop broadcasts.
    final sweep = _round++ % 2 == 0;
    final targets =
        targetsOverride != null ? await targetsOverride!() : await _defaultTargets(sweep: sweep, prefixes: _localPrefixes);
    for (final t in targets) {
      try {
        socket.send(packet, t, discoveryPort);
      } on SocketException {
        // e.g. broadcast not permitted on this platform; other targets still go out.
      }
    }
    _pruneStale();
  }

  void _onEvent(RawDatagramSocket socket, RawSocketEvent e) {
    if (e != RawSocketEvent.read) return;
    for (var dg = socket.receive(); dg != null; dg = socket.receive()) {
      final parsed = DiscoveryProtocol.parseAnnouncement(dg.data, dg.address.address);
      if (parsed == null) continue;
      final previous = _found[parsed.key];
      final all = {...?previous?.addresses, ...parsed.addresses}.toList();
      final b = parsed.copyWith(
        addresses: all,
        address: chooseAddress(
          source: dg.address.address,
          reported: all,
          localPrefixes: _localPrefixes,
          previous: previous?.address,
        ),
      );
      _found[b.key] = b;
      if (previous == null ||
          previous.clients != b.clients ||
          previous.name != b.name ||
          previous.address != b.address ||
          DateTime.now().difference(previous.lastSeen) > staleAfter) {
        notifyListeners();
      }
    }
  }

  void _pruneStale() {
    final before = _found.length;
    final cutoff = DateTime.now().subtract(staleAfter * 3);
    _found.removeWhere((_, b) => b.lastSeen.isBefore(cutoff));
    if (_found.length != before) notifyListeners();
  }

  static Future<List<InternetAddress>> _defaultTargets({required bool sweep, required Set<String> prefixes}) async {
    final targets = <InternetAddress>[InternetAddress('255.255.255.255')];
    try {
      final interfaces = await NetworkInterface.list(type: InternetAddressType.IPv4, includeLoopback: false);
      for (final ni in interfaces) {
        for (final a in ni.addresses) {
          final p = a.rawAddress;
          if (p.length != 4 || p[0] == 169) continue; // skip link-local
          final prefix = '${p[0]}.${p[1]}.${p[2]}';
          prefixes.add(prefix);
          targets.add(InternetAddress('$prefix.255'));
          if (sweep) {
            for (var host = 1; host < 255; host++) {
              if (host != p[3]) targets.add(InternetAddress('$prefix.$host'));
            }
          }
        }
      }
    } on SocketException {
      // No interface info; broadcast only.
    }
    return targets;
  }

  static String defaultDeviceName() {
    try {
      final host = Platform.localHostname;
      if (host.isNotEmpty && host != 'localhost') return host;
    } catch (_) {}
    if (Platform.isIOS) return 'iPhone';
    if (Platform.isAndroid) return 'Android phone';
    return 'Phone';
  }

  @override
  void dispose() {
    stop();
    super.dispose();
  }
}
