import 'dart:typed_data';
import '../models/gamepad_state.dart';

/// Packet type byte — must match server-side MessageType enum.
enum MessageType {
  connect(0),
  disconnect(1),
  input(2),
  ping(3);

  final int value;
  const MessageType(this.value);
}

/// Serializes controller state to the 48-byte little-endian UDP packet the server expects.
/// Field order must stay in sync with server MessageSerializer.cs.
class Protocol {
  static const int packetSize = 48;

  static Uint8List serialize(MessageType type, GamepadState state) {
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
    data.setFloat32(o, state.gyroX, Endian.little); o += 4;
    data.setFloat32(o, state.gyroY, Endian.little); o += 4;
    data.setFloat32(o, state.gyroZ, Endian.little); o += 4;
    data.setInt64(o, DateTime.now().millisecondsSinceEpoch, Endian.little);

    return data.buffer.asUint8List();
  }
}
