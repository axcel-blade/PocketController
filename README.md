# PocketController

Turn your Android phone into a wireless Xbox 360 controller for your PC.

## Architecture

```
PocketControllerServer (WinForms host)        frontend/ (Flutter Android app)
├── Protocol          — Packet format         ├── screens/connect_screen.dart
├── GamepadDriver     — Virtual Xbox via ViGEm├── screens/controller_screen.dart
└── NetworkLayer      — UDP server            ├── widgets/ (sticks, dpad, face, triggers)
                                              └── network/ (UDP client, protocol)
```

## Requirements

### Server (PC)
- Windows 10/11
- [ViGEmBus driver](https://github.com/nefarius/ViGEmBus/releases) installed
- .NET 10 runtime

### Client (Android)
- Flutter 3.x / Dart 3.x
- Android phone on the same Wi-Fi network

## Getting Started

1. Install the ViGEmBus driver.
2. Build and run `PocketControllerServer`.
3. Click **Start** — the server listens on UDP port `9000` by default.
4. Build and install the Flutter app (`cd frontend && flutter run`).
5. Enter your PC's IP address and port, then tap **Connect**.

## Default Settings

| Setting     | Value |
|-------------|-------|
| UDP Port    | 9000  |
| Max Clients | 4     |
| Timeout     | 5 s   |
| Send Rate   | 60 Hz |

## Version

**v1.1.1**
