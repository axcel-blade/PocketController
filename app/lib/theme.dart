import 'package:flutter/material.dart';

/// PocketController palette: dark graphite surfaces, a vivid lime accent for
/// primary/pressed states and restrained cyan for secondary detail.
class PcColors {
  static const background = Color(0xFF121417);
  static const surface = Color(0xFF1B1E23);
  static const surfaceRaised = Color(0xFF252930);
  static const outline = Color(0xFF363B44);
  static const lime = Color(0xFFC4F82A);
  static const onLime = Color(0xFF15190A);
  static const cyan = Color(0xFF5CD6E6);
  static const text = Color(0xFFE8EBEF);
  static const textDim = Color(0xFF8E96A3);
  static const danger = Color(0xFFFF6B6B);
  static const warning = Color(0xFFFFC857);
}

ThemeData buildTheme() {
  final base = ThemeData.dark(useMaterial3: true);
  return base.copyWith(
    scaffoldBackgroundColor: PcColors.background,
    colorScheme: const ColorScheme.dark(
      primary: PcColors.lime,
      onPrimary: PcColors.onLime,
      secondary: PcColors.cyan,
      surface: PcColors.surface,
      onSurface: PcColors.text,
      error: PcColors.danger,
      outline: PcColors.outline,
    ),
    textTheme: base.textTheme.apply(bodyColor: PcColors.text, displayColor: PcColors.text),
    appBarTheme: const AppBarTheme(backgroundColor: PcColors.background, elevation: 0, centerTitle: false),
    inputDecorationTheme: InputDecorationTheme(
      filled: true,
      fillColor: PcColors.surface,
      labelStyle: const TextStyle(color: PcColors.textDim),
      border: OutlineInputBorder(borderRadius: BorderRadius.circular(12), borderSide: const BorderSide(color: PcColors.outline)),
      enabledBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(12), borderSide: const BorderSide(color: PcColors.outline)),
      focusedBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(12), borderSide: const BorderSide(color: PcColors.lime, width: 1.5)),
    ),
    bottomSheetTheme: const BottomSheetThemeData(backgroundColor: PcColors.surface),
    dialogTheme: const DialogThemeData(backgroundColor: PcColors.surface),
    snackBarTheme: const SnackBarThemeData(backgroundColor: PcColors.surfaceRaised, contentTextStyle: TextStyle(color: PcColors.text)),
  );
}
