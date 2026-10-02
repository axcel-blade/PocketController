import 'package:flutter/material.dart';
import '../models/control_layout.dart';
import '../services/app_settings.dart';
import '../theme.dart';

enum LayoutSheetAction { createCustom }

/// Bottom sheet for picking, creating and deleting layouts.
Future<LayoutSheetAction?> showLayoutSheet(BuildContext context, AppSettings settings) {
  return showModalBottomSheet<LayoutSheetAction>(
    context: context,
    isScrollControlled: true,
    showDragHandle: true,
    constraints: const BoxConstraints(maxWidth: 560),
    builder: (context) => ListenableBuilder(
      listenable: settings,
      builder: (context, _) {
        final selectedId = settings.selectedLayout.id;
        Widget tile(ControlLayout l) => ListTile(
              leading: Icon(
                l.id == selectedId ? Icons.radio_button_checked_rounded : Icons.radio_button_off_rounded,
                color: l.id == selectedId ? PcColors.lime : PcColors.textDim,
              ),
              title: Text(l.name),
              subtitle: l.builtIn ? const Text('Built-in', style: TextStyle(color: PcColors.textDim)) : null,
              selected: l.id == selectedId,
              selectedColor: PcColors.text,
              onTap: () {
                settings.selectLayout(l.id);
                Navigator.pop(context);
              },
              trailing: l.builtIn
                  ? null
                  : IconButton(
                      tooltip: 'Delete ${l.name}',
                      icon: const Icon(Icons.delete_outline_rounded, color: PcColors.danger),
                      onPressed: () async {
                        final ok = await showDialog<bool>(
                          context: context,
                          builder: (c) => AlertDialog(
                            title: const Text('Delete layout?'),
                            content: Text('“${l.name}” will be removed from this device.'),
                            actions: [
                              TextButton(onPressed: () => Navigator.pop(c, false), child: const Text('Cancel')),
                              FilledButton(
                                style: FilledButton.styleFrom(backgroundColor: PcColors.danger),
                                onPressed: () => Navigator.pop(c, true),
                                child: const Text('Delete'),
                              ),
                            ],
                          ),
                        );
                        if (ok == true) await settings.deleteCustomLayout(l.id);
                      },
                    ),
            );

        return SafeArea(
          child: ListView(shrinkWrap: true, padding: const EdgeInsets.only(bottom: 12), children: [
            const _Header('Layouts'),
            for (final l in BuiltInLayouts.all) tile(l),
            const _Header('Your layouts'),
            if (settings.customLayouts.isEmpty)
              const Padding(
                padding: EdgeInsets.symmetric(horizontal: 16, vertical: 4),
                child: Text('No custom layouts yet.', style: TextStyle(color: PcColors.textDim)),
              ),
            for (final l in settings.customLayouts) tile(l),
            Padding(
              padding: const EdgeInsets.fromLTRB(16, 8, 16, 0),
              child: OutlinedButton.icon(
                onPressed: () => Navigator.pop(context, LayoutSheetAction.createCustom),
                icon: const Icon(Icons.add_rounded),
                label: Text('Customize “${settings.selectedLayout.name}”'),
              ),
            ),
          ]),
        );
      },
    ),
  );
}

class _Header extends StatelessWidget {
  final String text;
  const _Header(this.text);
  @override
  Widget build(BuildContext context) => Padding(
        padding: const EdgeInsets.fromLTRB(16, 8, 16, 4),
        child: Text(text.toUpperCase(),
            style: const TextStyle(color: PcColors.cyan, fontSize: 12, fontWeight: FontWeight.w700, letterSpacing: 1.2)),
      );
}

/// Asks for a name and returns the layout to save, or null if cancelled.
/// Built-in layouts are always saved as a new copy; custom ones can be overwritten.
Future<ControlLayout?> showSaveLayoutDialog(BuildContext context, AppSettings settings, ControlLayout draft) {
  final ctrl = TextEditingController(text: draft.builtIn ? '${draft.name} (custom)' : draft.name);
  String? error;
  return showDialog<ControlLayout>(
    context: context,
    builder: (context) => StatefulBuilder(builder: (context, setState) {
      ControlLayout? build({required bool asNew}) {
        final name = ctrl.text.trim();
        if (name.isEmpty) {
          setState(() => error = 'Enter a name');
          return null;
        }
        if (settings.isNameTaken(name, exceptId: asNew ? null : draft.id)) {
          setState(() => error = 'A layout with that name already exists');
          return null;
        }
        return draft.asCustom(id: asNew ? AppSettings.newLayoutId() : draft.id, name: name);
      }

      return AlertDialog(
        title: const Text('Save layout'),
        content: TextField(
          controller: ctrl,
          autofocus: true,
          maxLength: 32,
          decoration: InputDecoration(labelText: 'Layout name', errorText: error),
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(context), child: const Text('Cancel')),
          if (!draft.builtIn)
            TextButton(
              onPressed: () {
                final l = build(asNew: true);
                if (l != null) Navigator.pop(context, l);
              },
              child: const Text('Save as new'),
            ),
          FilledButton(
            onPressed: () {
              final l = build(asNew: draft.builtIn);
              if (l != null) Navigator.pop(context, l);
            },
            child: const Text('Save'),
          ),
        ],
      );
    }),
  );
}
