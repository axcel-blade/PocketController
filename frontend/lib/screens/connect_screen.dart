import 'package:flutter/material.dart';
import '../models/gamepad_state.dart';
import '../network/udp_client.dart';
import 'controller_screen.dart';

/// Initial screen for entering server IP and port before connecting.
class ConnectScreen extends StatefulWidget {
  const ConnectScreen({super.key});

  @override
  State<ConnectScreen> createState() => _ConnectScreenState();
}

class _ConnectScreenState extends State<ConnectScreen> {
  final _hostCtrl = TextEditingController(text: '192.168.1.100');
  final _portCtrl = TextEditingController(text: '9000');
  bool _connecting = false;
  String? _error;

  Future<void> _connect() async {
    setState(() { _connecting = true; _error = null; });

    final host = _hostCtrl.text.trim();
    final port = int.tryParse(_portCtrl.text.trim());

    if (host.isEmpty || port == null) {
      setState(() { _connecting = false; _error = 'Enter a valid host and port.'; });
      return;
    }

    try {
      final state = GamepadState();
      final client = UdpClient(host: host, port: port, state: state);
      await client.connect();

      if (!mounted) return;
      Navigator.of(context).pushReplacement(
        MaterialPageRoute(builder: (_) => ControllerScreen(client: client, state: state)),
      );
    } catch (e) {
      setState(() { _connecting = false; _error = e.toString(); });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFF0E0E0E),
      body: Center(
        child: SizedBox(
          width: 320,
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              // Xbox-style logo area
              Container(
                width: 72,
                height: 72,
                decoration: const BoxDecoration(
                  color: Color(0xFF107C10),
                  shape: BoxShape.circle,
                ),
                child: const Center(
                  child: Text('X', style: TextStyle(color: Colors.white, fontSize: 36, fontWeight: FontWeight.bold)),
                ),
              ),
              const SizedBox(height: 20),
              const Text(
                'PocketController',
                style: TextStyle(color: Colors.white, fontSize: 22, fontWeight: FontWeight.w600),
              ),
              const SizedBox(height: 8),
              const Text(
                'Enter server address to connect',
                style: TextStyle(color: Colors.white54, fontSize: 13),
              ),
              const SizedBox(height: 32),
              _field(_hostCtrl, 'Server IP Address', TextInputType.url),
              const SizedBox(height: 12),
              _field(_portCtrl, 'Port', TextInputType.number),
              if (_error != null) ...[
                const SizedBox(height: 12),
                Text(_error!, style: const TextStyle(color: Colors.redAccent, fontSize: 13)),
              ],
              const SizedBox(height: 24),
              SizedBox(
                width: double.infinity,
                height: 48,
                child: ElevatedButton(
                  onPressed: _connecting ? null : _connect,
                  style: ElevatedButton.styleFrom(
                    backgroundColor: const Color(0xFF107C10),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
                  ),
                  child: _connecting
                      ? const SizedBox(width: 20, height: 20, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white))
                      : const Text('Connect', style: TextStyle(color: Colors.white, fontSize: 16)),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _field(TextEditingController ctrl, String hint, TextInputType type) {
    return TextField(
      controller: ctrl,
      keyboardType: type,
      style: const TextStyle(color: Colors.white),
      decoration: InputDecoration(
        hintText: hint,
        hintStyle: const TextStyle(color: Colors.white38),
        filled: true,
        fillColor: Colors.white.withValues(alpha:0.07),
        border: OutlineInputBorder(borderRadius: BorderRadius.circular(8), borderSide: BorderSide.none),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(8),
          borderSide: const BorderSide(color: Color(0xFF107C10)),
        ),
      ),
    );
  }

  @override
  void dispose() {
    _hostCtrl.dispose();
    _portCtrl.dispose();
    super.dispose();
  }
}
