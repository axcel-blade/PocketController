import 'dart:io';
import 'dart:typed_data';
import 'package:flutter_test/flutter_test.dart';
import 'package:pocket_controller/models/gamepad_state.dart';
import 'package:pocket_controller/network/bridge_connection.dart';
import 'package:pocket_controller/network/protocol.dart';

/// Minimal stand-in for the Windows bridge: replies like server/ServerController.cs.
class FakeBridge {
  final RawDatagramSocket socket;
  final List<Uint8List> received = [];
  MessageType connectReply;

  FakeBridge._(this.socket, this.connectReply) {
    socket.listen((e) {
      if (e != RawSocketEvent.read) return;
      for (var dg = socket.receive(); dg != null; dg = socket.receive()) {
        _handle(dg);
      }
    });
  }

  void _handle(Datagram dg) {
    {
      received.add(dg.data);
      final type = MessageType.fromByte(dg.data[0]);
      final reply = switch (type) {
        MessageType.connect => connectReply,
        MessageType.ping => MessageType.pong,
        _ => null,
      };
      if (reply != null) {
        final echo = ByteData.sublistView(dg.data).getInt64(Protocol.timestampOffset, Endian.little);
        socket.send(Protocol.serialize(reply, GamepadState(), timestampMs: echo), dg.address, dg.port);
      }
    }
  }

  static Future<FakeBridge> start({MessageType connectReply = MessageType.connectAck}) async =>
      FakeBridge._(await RawDatagramSocket.bind(InternetAddress.loopbackIPv4, 0), connectReply);

  int get port => socket.port;
  Iterable<Uint8List> ofType(MessageType t) => received.where((d) => d[0] == t.value);
  void close() => socket.close();
}

void main() {
  group('Protocol', () {
    test('serializes 48 little-endian bytes in server field order', () {
      final s = GamepadState()
        ..buttons = 0x0401
        ..leftStickX = 0.5
        ..leftStickY = -1
        ..rightTrigger = 0.25
        ..dpad = 0x5;
      final bytes = Protocol.serialize(MessageType.input, s, timestampMs: 1234);
      final d = ByteData.sublistView(bytes);
      expect(bytes.length, 48);
      expect(bytes[0], 2);
      expect(d.getUint16(1, Endian.little), 0x0401);
      expect(d.getFloat32(3, Endian.little), 0.5);
      expect(d.getFloat32(7, Endian.little), -1);
      expect(d.getFloat32(23, Endian.little), 0.25);
      expect(bytes[27], 0x5);
      expect(d.getInt64(40, Endian.little), 1234);
    });

    test('parses replies and ignores client message types', () {
      final ack = Protocol.serialize(MessageType.pong, GamepadState(), timestampMs: 99);
      final r = Protocol.parseReply(ack)!;
      expect(r.type, MessageType.pong);
      expect(r.echoedTimestampMs, 99);
      expect(Protocol.parseReply(Protocol.serialize(MessageType.input, GamepadState())), isNull);
      expect(Protocol.parseReply(Uint8List(10)), isNull);
    });
  });

  group('GamepadState', () {
    test('button press and release toggle bits and notify', () {
      var events = 0;
      final s = GamepadState()..onDiscreteChange = () => events++;
      s.setButton(Btn.guide, true);
      expect(s.isPressed(Btn.guide), isTrue);
      s.setButton(Btn.guide, true); // no duplicate event
      s.setButton(Btn.guide, false);
      expect(s.buttons, 0);
      expect(events, 2);
    });

    test('sticks clamp to [-1, 1] and notify on return to centre', () {
      var events = 0;
      final s = GamepadState()..onDiscreteChange = () => events++;
      s.setLeftStick(2, -3);
      expect([s.leftStickX, s.leftStickY], [1, -1]);
      expect(events, 0);
      s.setLeftStick(0, 0);
      expect(events, 1);
    });
  });

  group('BridgeConnection', () {
    test('connects only after the bridge acknowledges, then sends input and pings', () async {
      final fake = await FakeBridge.start();
      final state = GamepadState();
      final bridge = BridgeConnection(state);
      final f = bridge.connect('127.0.0.1', fake.port);
      expect(bridge.status, BridgeStatus.connecting);
      await f;
      expect(bridge.status, BridgeStatus.connected);

      state.setButton(Btn.a, true);
      state.setButton(Btn.a, false);
      await Future<void>.delayed(const Duration(milliseconds: 150));
      final inputs = fake.ofType(MessageType.input).map((d) => ByteData.sublistView(d).getUint16(1, Endian.little));
      expect(inputs, contains(1)); // press
      expect(inputs.last, 0); // release
      expect(bridge.latencyMs, isNotNull);

      bridge.disconnect();
      await Future<void>.delayed(const Duration(milliseconds: 200));
      expect(bridge.status, BridgeStatus.disconnected);
      expect(fake.ofType(MessageType.disconnect), isNotEmpty);
      fake.close();
    });

    test('reports an error, not a connection, when nothing answers', () async {
      final silent = await RawDatagramSocket.bind(InternetAddress.loopbackIPv4, 0);
      final bridge = BridgeConnection(GamepadState());
      await bridge.connect('127.0.0.1', silent.port);
      expect(bridge.status, BridgeStatus.error);
      expect(bridge.error, contains('No response'));
      silent.close();
    });

    test('reports a full bridge', () async {
      final fake = await FakeBridge.start(connectReply: MessageType.serverFull);
      final bridge = BridgeConnection(GamepadState());
      await bridge.connect('127.0.0.1', fake.port);
      expect(bridge.status, BridgeStatus.error);
      expect(bridge.error, contains('full'));
      fake.close();
    });
  });
}
