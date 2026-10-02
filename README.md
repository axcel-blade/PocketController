# PocketController

<!-- Dynamic badges (live data from GitHub) -->
[![CI - Server](https://img.shields.io/github/actions/workflow/status/axcel-blade/PocketController/ci-server.yml?branch=develop&label=CI%20server&logo=dotnet)](https://github.com/axcel-blade/PocketController/actions/workflows/ci-server.yml)
[![CI - App](https://img.shields.io/github/actions/workflow/status/axcel-blade/PocketController/ci-app.yml?branch=develop&label=CI%20app&logo=flutter)](https://github.com/axcel-blade/PocketController/actions/workflows/ci-app.yml)
[![Latest release](https://img.shields.io/github/v/release/axcel-blade/PocketController?include_prereleases&sort=semver)](https://github.com/axcel-blade/PocketController/releases)
[![Open issues](https://img.shields.io/github/issues/axcel-blade/PocketController)](https://github.com/axcel-blade/PocketController/issues)
[![Last commit](https://img.shields.io/github/last-commit/axcel-blade/PocketController/develop)](https://github.com/axcel-blade/PocketController/commits/develop)
[![Stars](https://img.shields.io/github/stars/axcel-blade/PocketController?style=flat)](https://github.com/axcel-blade/PocketController/stargazers)

<!-- Static badges -->
[![License: MIT](https://img.shields.io/badge/license-MIT-c4f82a)](LICENSE)
[![Version](https://img.shields.io/badge/version-1.1.0-c4f82a)](CHANGELOG.md)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Flutter](https://img.shields.io/badge/Flutter-3.x-02569B?logo=flutter&logoColor=white)](https://flutter.dev/)
[![Platforms](https://img.shields.io/badge/app-Android%20%7C%20iOS-5cd6e6)](app/README.md)
[![Server](https://img.shields.io/badge/server-Windows%2010%2F11-0078D4?logo=windows&logoColor=white)](server/)
[![Protocol](https://img.shields.io/badge/protocol-UDP%20%C2%B7%2048%20bytes-5cd6e6)](docs/PROTOCOL.md)
[![Driver](https://img.shields.io/badge/driver-ViGEmBus-555)](https://github.com/nefarius/ViGEmBus)

Turn your Android or iOS phone into a wireless Xbox 360 (XInput) controller for your PC.

The phone app only works together with PocketController Server on Windows; see [docs/PROTOCOL.md](docs/PROTOCOL.md) for the UDP protocol.

## Architecture

```
PocketControllerServer (WinForms host)        app/ (Flutter Android/iOS app)
├── Protocol          — Packet format         ├── screens/ (controller, connection)
├── GamepadDriver     — Virtual Xbox via ViGEm├── widgets/ (controls, layout surface/picker)
└── NetworkLayer      — UDP server            ├── models/ (gamepad state, layouts)
                                              ├── services/ (persisted settings)
                                              └── network/ (bridge connection, protocol)
```

## Features

- **Controls:** 8-way D-pad, A/B/X/Y, two analog sticks, LB/RB, analog LT/RT, Back / Home / Start — all multi-touch.
- **Layouts:** Classic, Racing and Compact, plus custom layouts you can drag, resize, save and delete.
- **Automatic discovery:** the app finds PocketController servers on your Wi‑Fi and reconnects to your PC on launch. Entering an IP manually still works.
- **Honest connection status:** the app only shows *Connected* after the server acknowledges it, and shows latency and specific errors otherwise.
- **Comfort & accessibility:** haptic feedback (toggleable), screen-reader labels, 44 px minimum touch targets, sizing for small phones.
- Settings and layouts are stored on the phone.

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for a detailed walkthrough.

## Requirements

### Server (PC)
- Windows 10/11
- [ViGEmBus driver](https://github.com/nefarius/ViGEmBus/releases) installed
- .NET 10 runtime

### Client (Android / iOS)
- Flutter 3.x / Dart 3.x
- Android or iOS phone on the same Wi-Fi network

## Getting Started

1. Install the ViGEmBus driver.
2. Build and run `PocketControllerServer`.
3. Click **Start** — the server listens on UDP port `5555` by default.
4. Build and install the Flutter app (`cd app && flutter run`).
5. Open the **PC connection** screen (status chip, top left). Your PC appears under **On your network**; tap it. Or enter the PC's IP address and port manually and tap **Connect**.
6. If Windows Firewall asks, allow PocketController Server on private networks (UDP `5555` for play, `5556`–`5557` for discovery).

Next time, the app reconnects to the same PC automatically when it finds it.

> PocketController only works with PocketController Server. It is not compatible with DroidJoy or
> other controller servers. The wire format is documented in [docs/PROTOCOL.md](docs/PROTOCOL.md).

## Default Settings

| Setting     | Value |
|-------------|-------|
| UDP Port    | 5555  |
| Discovery   | UDP 5556 (probes) / 5557 (announcements) |
| Max Clients | 4     |
| Timeout     | 5 s   |
| Send Rate   | 60 Hz |

## Development

```sh
cd server   && dotnet test Tests
cd app && flutter analyze && flutter test
```

See [CONTRIBUTING.md](CONTRIBUTING.md) for branching and PR guidelines.

## Version

**v1.1.0**
