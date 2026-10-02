import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import '../models/control_layout.dart';
import '../models/gamepad_state.dart';
import '../network/auto_connect.dart';
import '../network/bridge_connection.dart';
import '../network/discovery.dart';
import '../services/app_settings.dart';
import '../theme.dart';
import '../widgets/control_surface.dart';
import '../widgets/layout_sheet.dart';
import '../widgets/status_chip.dart';
import 'connection_screen.dart';

/// Main screen: the controller itself, plus a slim toolbar for connection,
/// layout selection and the layout editor.
class ControllerScreen extends StatefulWidget {
  final AppSettings settings;
  final BridgeConnection bridge;
  final GamepadState state;
  final BridgeDiscovery discovery;
  final AutoConnector autoConnector;

  const ControllerScreen({
    super.key,
    required this.settings,
    required this.bridge,
    required this.state,
    required this.discovery,
    required this.autoConnector,
  });

  @override
  State<ControllerScreen> createState() => _ControllerScreenState();
}

class _ControllerScreenState extends State<ControllerScreen> with WidgetsBindingObserver {
  ControlLayout? _draft; // non-null while editing
  ControlKind? _selected;

  AppSettings get settings => widget.settings;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
    _lockLandscape();
  }

  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(this);
    super.dispose();
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState s) {
    // Never leave a button held on the PC when the app loses focus.
    if (s != AppLifecycleState.resumed) widget.state.reset();
  }

  void _lockLandscape() {
    SystemChrome.setPreferredOrientations([DeviceOrientation.landscapeLeft, DeviceOrientation.landscapeRight]);
    SystemChrome.setEnabledSystemUIMode(SystemUiMode.immersiveSticky);
  }

  Future<void> _openConnection() async {
    widget.state.reset();
    await Navigator.of(context).push(MaterialPageRoute(
      builder: (_) => ConnectionScreen(
        settings: settings,
        bridge: widget.bridge,
        discovery: widget.discovery,
        autoConnector: widget.autoConnector,
      ),
    ));
    _lockLandscape();
  }

  Future<void> _openLayouts() async {
    final action = await showLayoutSheet(context, settings);
    if (action == LayoutSheetAction.createCustom) _startEditing();
  }

  void _startEditing() {
    widget.state.reset();
    setState(() {
      _draft = settings.selectedLayout;
      _selected = null;
    });
  }

  void _cancelEditing() => setState(() {
        _draft = null;
        _selected = null;
      });

  Future<void> _saveDraft() async {
    final draft = _draft!;
    final result = await showSaveLayoutDialog(context, settings, draft);
    if (result == null || !mounted) return;
    await settings.saveCustomLayout(result);
    if (!mounted) return;
    _cancelEditing();
    ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Saved layout “${result.name}”')));
  }

  @override
  Widget build(BuildContext context) {
    return ListenableBuilder(
      listenable: Listenable.merge([settings, widget.bridge, widget.discovery]),
      builder: (context, _) {
        final editing = _draft != null;
        final layout = _draft ?? settings.selectedLayout;
        return Scaffold(
          body: SafeArea(
            child: Column(children: [
              SizedBox(height: 48, child: editing ? _editTopBar(layout) : _topBar(layout)),
              Expanded(
                child: Padding(
                  padding: const EdgeInsets.fromLTRB(12, 4, 12, 10),
                  child: ControlSurface(
                    layout: layout,
                    state: widget.state,
                    editing: editing,
                    selected: _selected,
                    onSelect: (k) => setState(() => _selected = k),
                    onMove: (p) => setState(() => _draft = _draft!.withPlacement(p)),
                  ),
                ),
              ),
              if (editing) _editBottomBar(layout),
            ]),
          ),
        );
      },
    );
  }

  Widget _topBar(ControlLayout layout) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 12),
      child: Row(children: [
        StatusChip(bridge: widget.bridge, discovery: widget.discovery, onTap: _openConnection),
        const Spacer(),
        TextButton.icon(
          onPressed: _openLayouts,
          icon: const Icon(Icons.grid_view_rounded, size: 18),
          label: Text(layout.name, overflow: TextOverflow.ellipsis),
          style: TextButton.styleFrom(foregroundColor: PcColors.text),
        ),
        IconButton(
          tooltip: 'Edit layout',
          onPressed: _startEditing,
          icon: const Icon(Icons.tune_rounded),
          color: PcColors.textDim,
        ),
        IconButton(
          tooltip: settings.hapticsEnabled ? 'Turn haptics off' : 'Turn haptics on',
          onPressed: () => settings.setHaptics(!settings.hapticsEnabled),
          icon: Icon(settings.hapticsEnabled ? Icons.vibration_rounded : Icons.mobile_off_rounded),
          color: settings.hapticsEnabled ? PcColors.cyan : PcColors.textDim,
        ),
        IconButton(
          tooltip: 'Connection',
          onPressed: _openConnection,
          icon: const Icon(Icons.settings_ethernet_rounded),
          color: PcColors.textDim,
        ),
      ]),
    );
  }

  Widget _editTopBar(ControlLayout layout) {
    return Container(
      color: PcColors.surface,
      padding: const EdgeInsets.symmetric(horizontal: 12),
      child: Row(children: [
        const Icon(Icons.tune_rounded, color: PcColors.lime, size: 20),
        const SizedBox(width: 8),
        Expanded(
          child: Text(
            'Editing “${layout.name}” — drag controls to move, tap one to resize',
            overflow: TextOverflow.ellipsis,
            style: const TextStyle(color: PcColors.text, fontWeight: FontWeight.w600),
          ),
        ),
        TextButton(onPressed: _cancelEditing, child: const Text('Cancel')),
        const SizedBox(width: 4),
        FilledButton.icon(
          onPressed: _saveDraft,
          icon: const Icon(Icons.save_rounded, size: 18),
          label: const Text('Save'),
        ),
      ]),
    );
  }

  Widget _editBottomBar(ControlLayout layout) {
    final kind = _selected;
    final p = kind == null ? null : layout.placementOf(kind);
    return Container(
      height: 52,
      color: PcColors.surface,
      padding: const EdgeInsets.symmetric(horizontal: 16),
      child: p == null
          ? const Align(
              alignment: Alignment.centerLeft,
              child: Text('Select a control to change its size.', style: TextStyle(color: PcColors.textDim)),
            )
          : Row(children: [
              SizedBox(
                width: 130,
                child: Text(p.kind.label, style: const TextStyle(fontWeight: FontWeight.w600)),
              ),
              const Icon(Icons.photo_size_select_small_rounded, size: 18, color: PcColors.textDim),
              Expanded(
                child: Slider(
                  value: p.size,
                  min: ControlPlacement.minSize,
                  max: ControlPlacement.maxSize,
                  label: 'Size',
                  semanticFormatterCallback: (v) => '${p.kind.label} size ${(v * 100).round()}',
                  onChanged: (v) => setState(() => _draft = _draft!.withPlacement(p.copyWith(size: v))),
                ),
              ),
              const Icon(Icons.photo_size_select_large_rounded, size: 18, color: PcColors.textDim),
            ]),
    );
  }
}
