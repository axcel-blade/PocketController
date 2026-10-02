import 'dart:async';
import 'dart:io';
import 'package:flutter/foundation.dart';
import '../models/gamepad_state.dart';
import 'protocol.dart';

enum BridgeStatus { disconnected, connecting, connected, error }

/// UDP link to the PocketController PC bridge (the `server/` project).
///
/// The app only reports [BridgeStatus.connected] after the bridge has answered the
/// Connect packet with a ConnectAck, and it drops to [BridgeStatus.error] if the
/// bridge stops answering pings. There is no optimistic "connected" state.
class BridgeConnection extends ChangeNotifier {
  static const int sendRateHz = 60;
  static const Duration pingInterval = Duration(seconds: 1);
  static const Duration handshakeTimeout = Duration(seconds: 3);
  static const Duration handshakeRetry = Duration(milliseconds: 600);
  static const Duration replyTimeout = Duration(seconds: 4);

  final GamepadState state;

  BridgeStatus _status = BridgeStatus.disconnected;
  String? _error;
  int? _latencyMs;
  String? _host;
  int? _port;

  RawDatagramSocket? _socket;
  InternetAddress? _addr;
  Timer? _inputTimer;
  Timer? _pingTimer;
  DateTime _lastReply = DateTime.fromMillisecondsSinceEpoch(0);
  Completer<MessageType>? _handshake;

  // Incremented whenever a connect attempt is superseded, so stale async work bails out.
  int _attempt = 0;

  BridgeConnection(this.state) {
    state.onDiscreteChange = sendNow;
  }

  BridgeStatus get status => _status;
  String? get error => _error;
  int? get latencyMs => _latencyMs;
  String? get host => _host;
  int? get port => _port;
  bool get isConnected => _status == BridgeStatus.connected;

  Future<void> connect(String host, int port) async {
    _teardown(sendDisconnect: true);
    final attempt = ++_attempt;
    _host = host;
    _port = port;
    _error = null;
    _latencyMs = null;
    _setStatus(BridgeStatus.connecting);

    try {
      final addresses = await InternetAddress.lookup(host).timeout(handshakeTimeout);
      if (attempt != _attempt) return;
      final addr = addresses.firstWhere(
        (a) => a.type == InternetAddressType.IPv4,
        orElse: () => addresses.first,
      );

      final socket = await RawDatagramSocket.bind(
        addr.type == InternetAddressType.IPv6 ? InternetAddress.anyIPv6 : InternetAddress.anyIPv4,
        0,
      );
      if (attempt != _attempt) {
        socket.close();
        return;
      }
      _addr = addr;
      _socket = socket;
      socket.listen(
        (e) => _onSocketEvent(socket, e),
        onError: (Object e) {
          if (identical(socket, _socket)) _fail(_describe(e));
        },
      );

      // UDP is lossy, so resend Connect until the bridge answers or we time out.
      final handshake = _handshake = Completer<MessageType>();
      final retry = Timer.periodic(handshakeRetry, (_) => _send(MessageType.connect));
      _send(MessageType.connect);
      final MessageType reply;
      try {
        reply = await handshake.future.timeout(handshakeTimeout);
      } finally {
        retry.cancel();
        _handshake = null;
      }
      if (attempt != _attempt) return;

      if (reply == MessageType.serverFull) {
        _fail('The bridge is full — all 4 controller slots are in use. '
            'Disconnect another device and try again.');
        return;
      }

      _lastReply = DateTime.now();
      _inputTimer = Timer.periodic(
        Duration(microseconds: (1000000 / sendRateHz).round()),
        (_) => _send(MessageType.input),
      );
      _pingTimer = Timer.periodic(pingInterval, (_) => _onPingTick());
      _setStatus(BridgeStatus.connected);
      _send(MessageType.ping);
    } on TimeoutException {
      if (attempt != _attempt) return;
      _fail('No response from a PocketController bridge at $host:$port. Make sure the PC app is '
          'running and started, both devices are on the same network, and Windows Firewall '
          'allows UDP port $port.');
    } catch (e) {
      if (attempt != _attempt) return;
      _fail(_describe(e));
    }
  }

  /// Gracefully leaves the bridge. Sends a neutral input first so nothing stays held on the PC.
  void disconnect() {
    _attempt++;
    _teardown(sendDisconnect: true);
    _error = null;
    _latencyMs = null;
    _setStatus(BridgeStatus.disconnected);
  }

  /// Sends the current state right away (used for press/release events).
  void sendNow() {
    if (isConnected) _send(MessageType.input);
  }

  void _onPingTick() {
    if (DateTime.now().difference(_lastReply) > replyTimeout) {
      _fail('Connection lost — the bridge stopped responding. '
          'Check Wi‑Fi and that the PC app is still running.');
      return;
    }
    _send(MessageType.ping);
  }

  void _onSocketEvent(RawDatagramSocket socket, RawSocketEvent event) {
    if (event != RawSocketEvent.read || !identical(socket, _socket)) return;
    for (var dg = socket.receive(); dg != null; dg = socket.receive()) {
      if (dg.address != _addr || dg.port != _port) continue;
      final reply = Protocol.parseReply(dg.data);
      if (reply != null) _onReply(reply);
    }
  }

  void _onReply(BridgeReply reply) {
    _lastReply = DateTime.now();
    switch (reply.type) {
      case MessageType.connectAck:
      case MessageType.serverFull:
        final h = _handshake;
        if (h != null && !h.isCompleted) h.complete(reply.type);
      case MessageType.pong:
        final rtt = DateTime.now().millisecondsSinceEpoch - reply.echoedTimestampMs;
        if (rtt >= 0 && rtt < 10000 && rtt != _latencyMs) {
          _latencyMs = rtt;
          notifyListeners();
        }
      case MessageType.notConnected:
        if (isConnected) {
          _fail('The bridge ended this session (it was restarted or timed out). Tap Connect to rejoin.');
        }
      default:
        break;
    }
  }

  void _send(MessageType type) {
    final socket = _socket;
    final addr = _addr;
    final port = _port;
    if (socket == null || addr == null || port == null) return;
    try {
      _sendReliably(socket, Protocol.serialize(type, state), addr, port);
    } on SocketException catch (e) {
      _fail(_describe(e));
    }
  }

  /// `send` returns 0 when the socket would block (seen on Windows under load), which
  /// silently drops the datagram. Retry briefly so press/release and Disconnect aren't lost.
  static void _sendReliably(RawDatagramSocket socket, List<int> bytes, InternetAddress addr, int port,
      [int retries = 10]) {
    if (socket.send(bytes, addr, port) > 0 || retries == 0) return;
    Timer(const Duration(milliseconds: 2), () {
      try {
        _sendReliably(socket, bytes, addr, port, retries - 1);
      } catch (_) {
        // Socket closed meanwhile.
      }
    });
  }

  void _fail(String message) {
    _attempt++;
    _teardown(sendDisconnect: _status == BridgeStatus.connected);
    _error = message;
    _latencyMs = null;
    _setStatus(BridgeStatus.error);
  }

  void _teardown({required bool sendDisconnect}) {
    _inputTimer?.cancel();
    _pingTimer?.cancel();
    _inputTimer = _pingTimer = null;
    final h = _handshake;
    if (h != null && !h.isCompleted) h.completeError(TimeoutException('cancelled'));
    _handshake = null;

    final socket = _socket, addr = _addr, port = _port;
    if (sendDisconnect && socket != null && addr != null && port != null) {
      // Neutral input, then Disconnect, so the PC never keeps a button held.
      final neutral = GamepadState();
      try {
        _sendReliably(socket, Protocol.serialize(MessageType.input, neutral), addr, port);
        _sendReliably(socket, Protocol.serialize(MessageType.disconnect, neutral), addr, port);
      } catch (_) {
        // Best effort — the bridge's heartbeat timeout cleans up otherwise.
      }
    }
    // Closing immediately can discard the datagrams just queued (seen on Windows),
    // so give the farewell packets a moment to leave first.
    if (socket != null) {
      if (sendDisconnect) {
        Timer(const Duration(milliseconds: 100), socket.close);
      } else {
        socket.close();
      }
    }
    _socket = null;
    _addr = null;
  }

  void _setStatus(BridgeStatus s) {
    _status = s;
    notifyListeners();
  }

  static String _describe(Object e) {
    if (e is SocketException) {
      final os = (e.osError?.message ?? '').toLowerCase();
      if (e.message.contains('host lookup') || os.contains('no address') || os.contains('host')) {
        return 'Couldn’t find that address. Enter the PC’s local IP, for example 192.168.1.20.';
      }
      if (os.contains('unreachable')) {
        return 'Network unreachable. Connect this phone to the same Wi‑Fi network as the PC.';
      }
      if (os.contains('refused')) {
        return 'The PC refused the packets. Is the PocketController bridge started on that port?';
      }
      return 'Network error: ${e.osError?.message ?? e.message}';
    }
    return 'Couldn’t connect: $e';
  }

  @override
  void dispose() {
    _attempt++;
    _teardown(sendDisconnect: true);
    state.onDiscreteChange = null;
    super.dispose();
  }
}
