import 'package:flutter/material.dart';
import 'screens/connect_screen.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  runApp(const PocketControllerApp());
}

class PocketControllerApp extends StatelessWidget {
  const PocketControllerApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'PocketController',
      debugShowCheckedModeBanner: false,
      theme: ThemeData.dark().copyWith(
        scaffoldBackgroundColor: const Color(0xFF0E0E0E),
        colorScheme: const ColorScheme.dark(primary: Color(0xFF107C10)),
      ),
      home: const ConnectScreen(),
    );
  }
}
