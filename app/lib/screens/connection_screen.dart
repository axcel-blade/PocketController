import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import '../network/auto_connect.dart';
import '../network/bridge_connection.dart';
import '../network/discovery.dart';
import '../services/app_settings.dart';
import '../theme.dart';
import '../widgets/status_chip.dart';

/// Enter the PC bridge address, connect/disconnect, and see real status and errors.
class ConnectionScreen extends StatefulWidget {
  final AppSettings settings;
  final BridgeConnection bridge;
  final BridgeDiscovery discovery;
  final AutoConnector autoConnector;
  const ConnectionScreen({
    super.key,
    required this.settings,
    required this.bridge,
    required this.discovery,
    required this.autoConnector,
  });

  @override
  State<ConnectionScreen> createState() => _ConnectionScreenState();
}

class _ConnectionScreenState extends State<ConnectionScreen> {
  final _formKey = GlobalKey<FormState>();
  late final _hostCtrl = TextEditingController(text: widget.settings.host);
  late final _portCtrl = TextEditingController(text: '${widget.settings.port}');

  @override
  void initState() {
    super.initState();
    // Typing an address is easier in portrait; the controller re-locks landscape on return.
    SystemChrome.setPreferredOrientations([]);
    SystemChrome.setEnabledSystemUIMode(SystemUiMode.edgeToEdge);
  }

  @override
  void dispose() {
    _hostCtrl.dispose();
    _portCtrl.dispose();
    super.dispose();
  }

  Future<void> _connectTo(DiscoveredBridge b) async {
    FocusScope.of(context).unfocus();
    _hostCtrl.text = b.address;
    _portCtrl.text = '${b.port}';
    await widget.autoConnector.connectTo(b);
  }

  Future<void> _connect() async {
    if (!_formKey.currentState!.validate()) return;
    FocusScope.of(context).unfocus();
    final host = _hostCtrl.text.trim();
    final port = int.parse(_portCtrl.text.trim());
    await widget.settings.setBridge(host, port);
    widget.autoConnector.noteManualConnect();
    await widget.bridge.connect(host, port);
  }

  static String? _validateHost(String? v) {
    final s = v?.trim() ?? '';
    if (s.isEmpty) return 'Enter your PC’s IP address or hostname';
    if (s.contains(' ') || s.contains('://') || s.contains('/')) return 'Enter just the address, e.g. 192.168.1.20';
    return null;
  }

  static String? _validatePort(String? v) {
    final p = int.tryParse(v?.trim() ?? '');
    if (p == null || p < 1 || p > 65535) return 'Port must be 1–65535';
    return null;
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('PC connection')),
      body: ListenableBuilder(
        listenable: Listenable.merge([widget.bridge, widget.settings, widget.discovery]),
        builder: (context, _) {
          final b = widget.bridge;
          final busy = b.status == BridgeStatus.connecting;
          return SafeArea(
            child: Center(
              child: ConstrainedBox(
                constraints: const BoxConstraints(maxWidth: 560),
                child: ListView(padding: const EdgeInsets.fromLTRB(16, 8, 16, 24), children: [
                  _StatusCard(bridge: b),
                  const SizedBox(height: 20),
                  _DiscoveredList(
                    discovery: widget.discovery,
                    bridge: b,
                    enabled: !busy,
                    onSelect: _connectTo,
                  ),
                  const SizedBox(height: 20),
                  const _SectionLabel('Or enter the address manually'),
                  const SizedBox(height: 8),
                  Form(
                    key: _formKey,
                    child: Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
                      Expanded(
                        flex: 3,
                        child: TextFormField(
                          controller: _hostCtrl,
                          enabled: !busy,
                          keyboardType: TextInputType.url,
                          autocorrect: false,
                          textInputAction: TextInputAction.next,
                          validator: _validateHost,
                          decoration: const InputDecoration(
                            labelText: 'PC address',
                            hintText: '192.168.1.20',
                            prefixIcon: Icon(Icons.computer_rounded),
                          ),
                        ),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        flex: 1,
                        child: TextFormField(
                          controller: _portCtrl,
                          enabled: !busy,
                          keyboardType: TextInputType.number,
                          inputFormatters: [FilteringTextInputFormatter.digitsOnly],
                          validator: _validatePort,
                          onFieldSubmitted: (_) => _connect(),
                          decoration: const InputDecoration(labelText: 'UDP port'),
                        ),
                      ),
                    ]),
                  ),
                  const SizedBox(height: 16),
                  Row(children: [
                    Expanded(
                      child: switch (b.status) {
                        BridgeStatus.connected => OutlinedButton.icon(
                            onPressed: widget.autoConnector.disconnect,
                            icon: const Icon(Icons.link_off_rounded),
                            label: const Text('Disconnect'),
                            style: OutlinedButton.styleFrom(
                              foregroundColor: PcColors.danger,
                              side: const BorderSide(color: PcColors.danger),
                              minimumSize: const Size.fromHeight(48),
                            ),
                          ),
                        BridgeStatus.connecting => OutlinedButton.icon(
                            onPressed: widget.autoConnector.disconnect,
                            icon: const SizedBox.square(
                                dimension: 16, child: CircularProgressIndicator(strokeWidth: 2)),
                            label: const Text('Cancel'),
                            style: OutlinedButton.styleFrom(minimumSize: const Size.fromHeight(48)),
                          ),
                        _ => FilledButton.icon(
                            onPressed: _connect,
                            icon: const Icon(Icons.link_rounded),
                            label: Text(b.status == BridgeStatus.error ? 'Try again' : 'Connect'),
                            style: FilledButton.styleFrom(minimumSize: const Size.fromHeight(48)),
                          ),
                      },
                    ),
                  ]),
                  const SizedBox(height: 24),
                  SwitchListTile(
                    value: widget.settings.autoConnect,
                    onChanged: widget.settings.setAutoConnect,
                    title: const Text('Connect automatically'),
                    subtitle: const Text('Join your last PC whenever it’s found on the network, and reconnect if the connection drops',
                        style: TextStyle(color: PcColors.textDim)),
                    contentPadding: EdgeInsets.zero,
                  ),
                  SwitchListTile(
                    value: widget.settings.hapticsEnabled,
                    onChanged: widget.settings.setHaptics,
                    title: const Text('Haptic feedback'),
                    subtitle: const Text('Vibrate briefly on button presses', style: TextStyle(color: PcColors.textDim)),
                    contentPadding: EdgeInsets.zero,
                  ),
                  const SizedBox(height: 16),
                  const _CompatibilityCard(),
                ]),
              ),
            ),
          );
        },
      ),
    );
  }
}

class _SectionLabel extends StatelessWidget {
  final String text;
  const _SectionLabel(this.text);
  @override
  Widget build(BuildContext context) => Text(
        text.toUpperCase(),
        style: const TextStyle(color: PcColors.cyan, fontSize: 12, fontWeight: FontWeight.w700, letterSpacing: 1.2),
      );
}

/// Servers found by LAN discovery. Tapping one connects to it.
class _DiscoveredList extends StatelessWidget {
  final BridgeDiscovery discovery;
  final BridgeConnection bridge;
  final bool enabled;
  final ValueChanged<DiscoveredBridge> onSelect;

  const _DiscoveredList({
    required this.discovery,
    required this.bridge,
    required this.enabled,
    required this.onSelect,
  });

  @override
  Widget build(BuildContext context) {
    final found = discovery.bridges;
    final connectedKey = bridge.isConnected ? '${bridge.host}:${bridge.port}' : null;

    return Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
      Row(children: [
        const Expanded(child: _SectionLabel('On your network')),
        if (discovery.isScanning)
          const Padding(
            padding: EdgeInsets.only(right: 8),
            child: SizedBox.square(
              dimension: 14,
              child: CircularProgressIndicator(strokeWidth: 2, color: PcColors.cyan),
            ),
          ),
        TextButton.icon(
          onPressed: () => discovery.rescan(duration: bridge.isConnected ? const Duration(seconds: 10) : null),
          icon: const Icon(Icons.refresh_rounded, size: 18),
          label: Text(discovery.isScanning ? 'Searching' : 'Search'),
          style: TextButton.styleFrom(foregroundColor: PcColors.cyan),
        ),
      ]),
      const SizedBox(height: 4),
      if (found.isEmpty)
        Container(
          padding: const EdgeInsets.all(14),
          decoration: BoxDecoration(
            color: PcColors.surface,
            borderRadius: BorderRadius.circular(14),
            border: Border.all(color: PcColors.outline),
          ),
          child: Text(
            discovery.isScanning
                ? 'Looking for PocketController servers on this Wi‑Fi…'
                : bridge.isConnected
                    ? 'Search paused while connected. Tap Search to look again.'
                    : 'No servers found. Make sure the PC app is running and started, and that both '
                        'devices are on the same network — or enter the address below.',
            style: const TextStyle(color: PcColors.textDim, height: 1.35),
          ),
        ),
      for (final b in found)
        Padding(
          padding: const EdgeInsets.only(bottom: 8),
          child: _BridgeTile(
            bridge: b,
            connected: b.endpoint == connectedKey,
            onTap: enabled && b.endpoint != connectedKey && !b.isFull ? () => onSelect(b) : null,
          ),
        ),
    ]);
  }
}

class _BridgeTile extends StatelessWidget {
  final DiscoveredBridge bridge;
  final bool connected;
  final VoidCallback? onTap;
  const _BridgeTile({required this.bridge, required this.connected, required this.onTap});

  @override
  Widget build(BuildContext context) {
    final b = bridge;
    final accent = connected ? PcColors.lime : PcColors.cyan;
    final trailing = connected
        ? 'Connected'
        : b.isFull
            ? 'Full'
            : 'Connect';
    return Semantics(
      button: onTap != null,
      label: '${b.name}, ${b.address} port ${b.port}, ${b.clients} of ${b.max} controllers in use. $trailing',
      excludeSemantics: true,
      child: Material(
        color: PcColors.surface,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(14),
          side: BorderSide(color: connected ? PcColors.lime : PcColors.outline),
        ),
        child: InkWell(
          borderRadius: BorderRadius.circular(14),
          onTap: onTap,
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
            child: Row(children: [
              Icon(Icons.desktop_windows_rounded, color: accent),
              const SizedBox(width: 12),
              Expanded(
                child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                  Text(b.name, style: const TextStyle(fontWeight: FontWeight.w700), overflow: TextOverflow.ellipsis),
                  const SizedBox(height: 2),
                  Text('${b.address}:${b.port}  ·  ${b.clients}/${b.max} controllers',
                      style: const TextStyle(color: PcColors.textDim, fontSize: 12)),
                ]),
              ),
              Text(trailing,
                  style: TextStyle(
                    color: b.isFull && !connected ? PcColors.warning : accent,
                    fontWeight: FontWeight.w700,
                  )),
            ]),
          ),
        ),
      ),
    );
  }
}

class _StatusCard extends StatelessWidget {
  final BridgeConnection bridge;
  const _StatusCard({required this.bridge});

  @override
  Widget build(BuildContext context) {
    final s = describeStatus(bridge);
    final detail = switch (bridge.status) {
      BridgeStatus.connected =>
        'The bridge at ${bridge.host}:${bridge.port} confirmed this controller. Input is being sent.',
      BridgeStatus.connecting => 'Waiting for the bridge at ${bridge.host}:${bridge.port} to answer…',
      BridgeStatus.error => bridge.error ?? 'Something went wrong.',
      BridgeStatus.disconnected => 'Controls work on screen but nothing is sent until a bridge confirms the connection.',
    };
    return Semantics(
      liveRegion: true,
      label: '${s.label}. $detail',
      child: ExcludeSemantics(
        child: Container(
          padding: const EdgeInsets.all(16),
          decoration: BoxDecoration(
            color: PcColors.surface,
            borderRadius: BorderRadius.circular(16),
            border: Border.all(color: s.color.withValues(alpha: 0.45)),
          ),
          child: Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Icon(s.icon, color: s.color, size: 28),
            const SizedBox(width: 14),
            Expanded(
              child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Text(s.label, style: TextStyle(color: s.color, fontSize: 17, fontWeight: FontWeight.w700)),
                const SizedBox(height: 4),
                Text(detail, style: const TextStyle(color: PcColors.text, height: 1.35)),
              ]),
            ),
          ]),
        ),
      ),
    );
  }
}

class _CompatibilityCard extends StatelessWidget {
  const _CompatibilityCard();

  @override
  Widget build(BuildContext context) {
    const body = TextStyle(color: PcColors.textDim, height: 1.4);
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: PcColors.surface,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: PcColors.outline),
      ),
      child: const Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
        Row(children: [
          Icon(Icons.info_outline_rounded, color: PcColors.cyan, size: 20),
          SizedBox(width: 8),
          Text('What you need on the PC', style: TextStyle(fontWeight: FontWeight.w700)),
        ]),
        SizedBox(height: 10),
        Text(
          'This app needs the PocketController Server for Windows (in this project’s server/ folder) '
          'running on your PC. The server uses the ViGEmBus driver to create a virtual Xbox 360 '
          'controller, so games see a normal XInput gamepad.',
          style: body,
        ),
        SizedBox(height: 8),
        Text(
          '1. Install ViGEmBus.  2. Run PocketController Server and press Start.  '
          '3. Pick your PC under “On your network”, or enter the IP and port shown in the server '
          '(default 5555).  4. If Windows Firewall asks, allow the server on private networks '
          '(UDP 5555 for play, 5556–5557 for discovery).',
          style: body,
        ),
        SizedBox(height: 8),
        Text(
          'Not compatible with DroidJoy or other controller servers. The connection uses UDP '
          '(48-byte packets, documented in docs/PROTOCOL.md), not WebSockets.',
          style: body,
        ),
      ]),
    );
  }
}
