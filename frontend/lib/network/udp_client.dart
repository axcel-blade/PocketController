import 'dart:async';
import 'dart:io';
import '../models/gamepad_state.dart';
import 'protocol.dart';

/// Manages the UDP connection to the PocketController server.
/// Sends input packets at [sendRateHz] Hz and ping keep-alives every 2 seconds.
class UdpClient {
  static const int sendRateHz = 60;
  static const Duration pingInterval = Duration(seconds: 2);

  final String host;
  final int port;
  final GamepadState state;

  RawDatagramSocket? _socket;
  InternetAddress? _serverAddr;
  Timer? _inputTimer;
  Timer? _pingTimer;

  UdpClient({required this.host, required this.port, required this.state});

  Future<void> connect() async {
    _serverAddr = (await InternetAddress.lookup(host)).first;
    _socket = await RawDatagramSocket.bind(InternetAddress.anyIPv4, 0);

    _send(MessageType.connect);

    _inputTimer = Timer.periodic(
      Duration(microseconds: (1000000 / sendRateHz).round()),
      (_) => _send(MessageType.input),
    );

    _pingTimer = Timer.periodic(pingInterval, (_) => _send(MessageType.ping));
  }

  void _send(MessageType type) {
    final socket = _socket;
    final addr = _serverAddr;
    if (socket == null || addr == null) return;
    final bytes = Protocol.serialize(type, state);
    socket.send(bytes, addr, port);
  }

  void disconnect() {
    _send(MessageType.disconnect);
    _inputTimer?.cancel();
    _pingTimer?.cancel();
    _socket?.close();
    _socket = null;
  }
}
