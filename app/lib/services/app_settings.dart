import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../models/control_layout.dart';

/// Locally persisted preferences: bridge address, haptics, layouts.
class AppSettings extends ChangeNotifier {
  static const defaultPort = 5555; // matches server Protocol/Constants.DefaultPort

  static const _kHost = 'bridge.host';
  static const _kPort = 'bridge.port';
  static const _kName = 'bridge.name';
  static const _kAutoConnect = 'bridge.autoConnect';
  static const _kHaptics = 'haptics.enabled';
  static const _kSelected = 'layout.selected';
  static const _kCustom = 'layout.custom';

  final SharedPreferences _prefs;

  String _host;
  int _port;
  String _bridgeName;
  bool _autoConnect;
  bool _haptics;
  String _selectedId;
  final List<ControlLayout> _custom;

  AppSettings._(this._prefs, this._host, this._port, this._bridgeName, this._autoConnect, this._haptics,
      this._selectedId, this._custom);

  static Future<AppSettings> load() async {
    final prefs = await SharedPreferences.getInstance();
    return AppSettings._(
      prefs,
      prefs.getString(_kHost) ?? '',
      prefs.getInt(_kPort) ?? defaultPort,
      prefs.getString(_kName) ?? '',
      prefs.getBool(_kAutoConnect) ?? true,
      prefs.getBool(_kHaptics) ?? true,
      prefs.getString(_kSelected) ?? BuiltInLayouts.classic.id,
      _decodeLayouts(prefs.getString(_kCustom)),
    );
  }

  String get host => _host;
  int get port => _port;

  /// Name the server announced when we last connected through discovery ('' if entered manually).
  String get bridgeName => _bridgeName;

  /// Join the last-used server automatically when it is found on the network.
  bool get autoConnect => _autoConnect;
  bool get hapticsEnabled => _haptics;
  List<ControlLayout> get customLayouts => List.unmodifiable(_custom);
  List<ControlLayout> get allLayouts => [...BuiltInLayouts.all, ..._custom];

  ControlLayout get selectedLayout =>
      allLayouts.where((l) => l.id == _selectedId).firstOrNull ?? BuiltInLayouts.classic;

  /// Remembers the server to use. [name] comes from discovery; a manually typed
  /// address keeps the old name only if it points at the same server.
  Future<void> setBridge(String host, int port, {String? name}) async {
    _bridgeName = name ?? (host == _host && port == _port ? _bridgeName : '');
    _host = host;
    _port = port;
    notifyListeners();
    await _prefs.setString(_kHost, host);
    await _prefs.setInt(_kPort, port);
    await _prefs.setString(_kName, _bridgeName);
  }

  Future<void> setAutoConnect(bool enabled) async {
    _autoConnect = enabled;
    notifyListeners();
    await _prefs.setBool(_kAutoConnect, enabled);
  }

  Future<void> setHaptics(bool enabled) async {
    _haptics = enabled;
    notifyListeners();
    await _prefs.setBool(_kHaptics, enabled);
  }

  Future<void> selectLayout(String id) async {
    _selectedId = id;
    notifyListeners();
    await _prefs.setString(_kSelected, id);
  }

  /// Saves (or overwrites, when [layout.id] already exists) a custom layout and selects it.
  Future<void> saveCustomLayout(ControlLayout layout) async {
    assert(!layout.builtIn);
    final i = _custom.indexWhere((l) => l.id == layout.id);
    if (i >= 0) {
      _custom[i] = layout;
    } else {
      _custom.add(layout);
    }
    _selectedId = layout.id;
    notifyListeners();
    await _persistCustom();
    await _prefs.setString(_kSelected, layout.id);
  }

  Future<void> deleteCustomLayout(String id) async {
    _custom.removeWhere((l) => l.id == id);
    if (_selectedId == id) _selectedId = BuiltInLayouts.classic.id;
    notifyListeners();
    await _persistCustom();
    await _prefs.setString(_kSelected, _selectedId);
  }

  bool isNameTaken(String name, {String? exceptId}) => allLayouts
      .any((l) => l.id != exceptId && l.name.trim().toLowerCase() == name.trim().toLowerCase());

  static String newLayoutId() => 'custom.${DateTime.now().microsecondsSinceEpoch}';

  Future<void> _persistCustom() =>
      _prefs.setString(_kCustom, jsonEncode([for (final l in _custom) l.toJson()]));

  static List<ControlLayout> _decodeLayouts(String? raw) {
    if (raw == null) return [];
    try {
      final list = jsonDecode(raw);
      if (list is! List) return [];
      return [
        for (final item in list)
          if (item is Map<String, dynamic>) ?ControlLayout.fromJson(item),
      ];
    } catch (_) {
      return []; // Corrupt data shouldn't brick the app.
    }
  }
}
