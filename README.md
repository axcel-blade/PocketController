# PocketController

Turn your Android phone into a wireless Xbox 360 controller for your PC.

## Architecture

```
PocketControllerServer (WinForms host)
├── Protocol          — Packet format, serialization, constants
├── GamepadDriver     — Virtual Xbox 360 controller via ViGEmBus
└── NetworkLayer      — UDP server, client sessions, heartbeat
```

## Requirements

- Windows 10/11
- [ViGEmBus driver](https://github.com/nefarius/ViGEmBus/releases) installed
- .NET 10 runtime

## Getting Started

1. Install the ViGEmBus driver.
2. Build and run `PocketControllerServer`.
3. Click **Start** — the server listens on UDP port `5555` by default.
4. Connect your Android app to the displayed IP address.

## Default Settings

| Setting     | Value |
|-------------|-------|
| UDP Port    | 5555  |
| Max Clients | 4     |
| Timeout     | 5 s   |

## Version

**v1.0.0**
