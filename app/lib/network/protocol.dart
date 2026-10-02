import 'dart:typed_data';
import '../models/gamepad_state.dart';

/// Packet type byte — must match the server-side MessageType enum.
enum MessageType {
  connect(0),
  disconnect(1),
  input(2),
  ping(3),
  connectAck(4),
  pong(5),
  serverFull(6),
  notConnected(7);

  final int value;
  const MessageType(this.value);

  static MessageType? fromByte(int b) {
    for (final t in values) {
      if (t.value == b) return t;
    }
    return null;
  }
}

/// A control reply sent by the bridge (ConnectAck, Pong, ServerFull, NotConnected).
class BridgeReply {
  final MessageType type;

  /// The client timestamp the bridge echoed back, used for round-trip latency.
  final int echoedTimestampMs;

  const BridgeReply(this.type, this.echoedTimestampMs);
}

/// Serializes controller state to the 48-byte little-endian UDP packet the bridge expects.
/// Field order must stay in sync with server MessageSerializer.cs. See docs/PROTOCOL.md.
class Protocol {
  static const int packetSize = 48;
  static const int timestampOffset = 40;

  static Uint8List serialize(MessageType type, GamepadState state, {int? timestampMs}) {
    final data = ByteData(packetSize);
    int o = 0;

    data.setUint8(o, type.value); o += 1;
    data.setUint16(o, state.buttons, Endian.little); o += 2;
    data.setFloat32(o, state.leftStickX, Endian.little); o += 4;
    data.setFloat32(o, state.leftStickY, Endian.little); o += 4;
    data.setFloat32(o, state.rightStickX, Endian.little); o += 4;
    data.setFloat32(o, state.rightStickY, Endian.little); o += 4;
    data.setFloat32(o, state.leftTrigger, Endian.little); o += 4;
    data.setFloat32(o, state.rightTrigger, Endian.little); o += 4;
    data.setUint8(o, state.dpad); o += 1;
    // Gyro X/Y/Z: reserved, always zero from this client.
    o += 12;
    data.setInt64(o, timestampMs ?? DateTime.now().millisecondsSinceEpoch, Endian.little);

    return data.buffer.asUint8List();
  }

  /// Parses a server → client reply. Returns null for anything that isn't a known reply.
  static BridgeReply? parseReply(Uint8List bytes) {
    if (bytes.length < packetSize) return null;
    final type = MessageType.fromByte(bytes[0]);
    if (type == null || type.value < MessageType.connectAck.value) return null;
    final data = ByteData.sublistView(bytes);
    return BridgeReply(type, data.getInt64(timestampOffset, Endian.little));
  }
}
