import 'package:flutter/material.dart';
import '../network/bridge_connection.dart';
import '../network/discovery.dart';
import '../theme.dart';

({Color color, String label, IconData icon}) describeStatus(BridgeConnection b) => switch (b.status) {
      BridgeStatus.connected => (
          color: PcColors.lime,
          label: b.latencyMs == null ? 'Connected' : 'Connected · ${b.latencyMs} ms',
          icon: Icons.link_rounded,
        ),
      BridgeStatus.connecting => (color: PcColors.warning, label: 'Connecting…', icon: Icons.sync_rounded),
      BridgeStatus.error => (color: PcColors.danger, label: 'Connection problem', icon: Icons.link_off_rounded),
      BridgeStatus.disconnected => (color: PcColors.textDim, label: 'Not connected', icon: Icons.link_off_rounded),
    };

/// Compact, always-visible connection indicator. Tapping opens the connection screen.
class StatusChip extends StatelessWidget {
  final BridgeConnection bridge;
  final BridgeDiscovery? discovery;
  final VoidCallback onTap;
  const StatusChip({super.key, required this.bridge, required this.onTap, this.discovery});

  @override
  Widget build(BuildContext context) {
    var s = describeStatus(bridge);
    final found = discovery?.bridges.length ?? 0;
    // When idle but a server is visible, say so: one tap away from playing.
    if (found > 0 &&
        (bridge.status == BridgeStatus.disconnected || bridge.status == BridgeStatus.error)) {
      s = (
        color: PcColors.cyan,
        label: found == 1 ? '1 PC found · tap to connect' : '$found PCs found · tap to connect',
        icon: Icons.wifi_find_rounded,
      );
    }
    return Semantics(
      button: true,
      label: 'Connection status: ${s.label}. Open connection settings.',
      child: ExcludeSemantics(
        child: Material(
          color: PcColors.surface,
          shape: StadiumBorder(side: BorderSide(color: s.color.withValues(alpha: 0.5))),
          child: InkWell(
            customBorder: const StadiumBorder(),
            onTap: onTap,
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 7),
              child: Row(mainAxisSize: MainAxisSize.min, children: [
                Container(
                  width: 8,
                  height: 8,
                  decoration: BoxDecoration(color: s.color, shape: BoxShape.circle),
                ),
                const SizedBox(width: 8),
                Text(s.label, style: TextStyle(color: s.color, fontWeight: FontWeight.w600, fontSize: 13)),
              ]),
            ),
          ),
        ),
      ),
    );
  }
}
