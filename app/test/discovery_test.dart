import 'dart:async';
import 'dart:convert';
import 'dart:io';
import 'package:flutter_test/flutter_test.dart';
import 'package:pocket_controller/network/auto_connect.dart';
import 'package:pocket_controller/network/discovery.dart';

DiscoveredBridge _b(String name, String addr, {int port = 5555, int clients = 0, int max = 4}) =>
    DiscoveredBridge(name: name, address: addr, port: port, clients: clients, max: max, lastSeen: DateTime.now());

void main() {
  group('DiscoveryProtocol', () {
    test('builds probes and parses announcements like the server', () {
      expect(utf8.decode(DiscoveryProtocol.probe('My|Phone')), 'PCTRL?1|My Phone');

      final b = DiscoveryProtocol.parseAnnouncement(
        utf8.encode('PCTRL!1|{"name":"GAMING-PC","port":5555,"clients":1,"max":4}'),
        '192.168.1.20',
      )!;
      expect([b.name, b.address, b.port, b.clients, b.max], ['GAMING-PC', '192.168.1.20', 5555, 1, 4]);
      expect(b.isFull, isFalse);
    });

    test('rejects garbage, wrong prefixes and bad ports', () {
      for (final t in [
        '',
        'hello',
        'PCTRL?1|probe',
        'PCTRL!1|{bad json',
        'PCTRL!1|{"name":"x","port":0,"clients":0,"max":4}',
        'PCTRL!1|{"name":"x","port":"5555","clients":0,"max":4}',
      ]) {
        expect(DiscoveryProtocol.parseAnnouncement(utf8.encode(t), '1.2.3.4'), isNull, reason: t);
      }
    });
  });

  group('Multiple network adapters', () {
    test('announcement carries the server id and all of its addresses', () {
      final b = DiscoveryProtocol.parseAnnouncement(
        utf8.encode('PCTRL!1|{"name":"PC","port":5555,"clients":0,"max":4,"id":"abc",'
            '"ips":["192.168.0.5","172.20.0.1","192.168.56.1"]}'),
        '192.168.56.1',
      )!;
      expect(b.key, 'abc');
      expect(b.addresses, ['192.168.56.1', '192.168.0.5', '172.20.0.1']);
    });

    test('picks the address on the phone\'s own subnet', () {
      expect(
        chooseAddress(
          source: '192.168.56.1', // heard via a VirtualBox/VMware adapter
          reported: ['192.168.0.5', '172.20.0.1'],
          localPrefixes: {'192.168.0'},
        ),
        '192.168.0.5',
      );
    });

    test('falls back to the previous choice, then the packet source', () {
      expect(chooseAddress(source: '10.1.1.1', reported: [], localPrefixes: {}, previous: '10.2.2.2'), '10.2.2.2');
      expect(chooseAddress(source: '10.1.1.1', reported: [], localPrefixes: {}), '10.1.1.1');
    });
  });

  group('BridgeDiscovery', () {
    test('lists a server once even when heard from several addresses', () async {
      // One server (same id) answering from two source addresses, like a PC whose
      // reply also goes out through a WSL/VMware adapter.
      final a = await RawDatagramSocket.bind(InternetAddress.loopbackIPv4, 0);
      final b = await RawDatagramSocket.bind(InternetAddress('127.0.0.2'), 0);
      const reply = 'PCTRL!1|{"name":"PC","port":5555,"clients":0,"max":4,"id":"same-server"}';
      a.listen((e) {
        if (e != RawSocketEvent.read) return;
        for (var dg = a.receive(); dg != null; dg = a.receive()) {
          a.send(utf8.encode(reply), dg.address, dg.port);
          b.send(utf8.encode(reply), dg.address, dg.port);
        }
      });
      final discovery = BridgeDiscovery(
        deviceName: 'Test',
        targetsOverride: () async => [InternetAddress.loopbackIPv4],
        discoveryPort: a.port,
        announcePort: null,
      );
      await discovery.start();
      await Future<void>.delayed(const Duration(milliseconds: 400));

      expect(discovery.bridges.length, 1);
      expect(discovery.bridges.single.addresses.toSet(), {'127.0.0.1', '127.0.0.2'});
      discovery.stop();
      a.close();
      b.close();
    });

    test('finds a server that answers probes', () async {
      final server = await RawDatagramSocket.bind(InternetAddress.loopbackIPv4, 0);
      final probes = <String>[];
      server.listen((e) {
        if (e != RawSocketEvent.read) return;
        for (var dg = server.receive(); dg != null; dg = server.receive()) {
          probes.add(utf8.decode(dg.data));
          server.send(
            utf8.encode('PCTRL!1|{"name":"TEST-PC","port":6000,"clients":0,"max":4}'),
            dg.address,
            dg.port,
          );
        }
      });

      final discovery = BridgeDiscovery(
        deviceName: 'Test Phone',
        targetsOverride: () async => [InternetAddress.loopbackIPv4],
        discoveryPort: server.port,
        announcePort: null,
      );
      final found = Completer<void>();
      discovery.addListener(() {
        if (discovery.bridges.isNotEmpty && !found.isCompleted) found.complete();
      });
      await discovery.start();
      await found.future.timeout(const Duration(seconds: 3));

      final b = discovery.bridges.single;
      expect([b.name, b.address, b.port], ['TEST-PC', '127.0.0.1', 6000]);
      expect(probes.first, 'PCTRL?1|Test Phone');

      discovery.stop();
      expect(discovery.isScanning, isFalse);
      server.close();
    });
  });

  group('pickAutoConnectTarget', () {
    DiscoveredBridge? pick(List<DiscoveredBridge> found,
            {String host = '', int port = 5555, String name = '', bool over = false}) =>
        pickAutoConnectTarget(found, savedHost: host, savedPort: port, savedName: name, scanWindowOver: over);

    test('prefers the last-used address and port', () {
      final found = [_b('A', '192.168.1.5'), _b('B', '192.168.1.9')];
      expect(pick(found, host: '192.168.1.9')?.name, 'B');
    });

    test('follows the last server by name when its IP changed', () {
      final found = [_b('GAMING-PC', '192.168.1.44')];
      expect(pick(found, host: '192.168.1.20', name: 'GAMING-PC')?.address, '192.168.1.44');
    });

    test('joins the only server once the scan window ends, if nothing was saved', () {
      final found = [_b('ONLY', '10.0.0.2')];
      expect(pick(found), isNull);
      expect(pick(found, over: true)?.name, 'ONLY');
    });

    test('never guesses between several servers or joins a full one', () {
      expect(pick([_b('A', '10.0.0.2'), _b('B', '10.0.0.3')], over: true), isNull);
      expect(pick([_b('A', '10.0.0.2', clients: 4)], host: '10.0.0.2', over: true), isNull);
    });

    test('does not jump to an unknown server when a different one was saved', () {
      expect(pick([_b('OTHER', '10.0.0.9')], host: '10.0.0.2', name: 'MINE', over: true), isNull);
    });
  });
}
