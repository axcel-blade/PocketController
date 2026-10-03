# Changelog

## [Unreleased]

## [1.2.2] - 2026-10-03

### Fixed
- App icon no longer has transparent corners: the iOS app icon set is a full lime square (corners were rendering black), and Android 8+ uses an adaptive icon (lime background + controller foreground) instead of a circle on a white plate
- Launch screen uses the app background colour (`#121417`) on Android and iOS instead of white
- Android 12+ launch screen shows the logo without the white circle behind it, using a dedicated transparent launch icon

## [1.2.1] - 2026-10-03

### Changed
- New controller logo for the Android app, iOS app and Windows server: launcher icons, iOS app icon set, server `.exe`, window and tray icon (`server/Assets/app.ico`)
- Source logos (1024×1024) live in `docs/assets/logos/`

## [1.2.0] - 2026-10-03

### Added
- **Mobile app redesign (Android + iOS):** graphite/lime/cyan theme; Classic, Racing and Compact layouts; custom layout editor (drag, resize, save, select, delete); haptics toggle; screen-reader labels; minimum touch targets and small-screen sizing; settings and layouts saved on the device
- **Connection screen** with verified status, round-trip latency and actionable error messages; "Connected" is only shown after the server acknowledges the handshake
- **LAN discovery:** the server answers probes (UDP 5556) and broadcasts announcements (UDP 5557); the app lists servers on the network and connects with one tap. Manual IP entry is unchanged
- **Auto-connect:** the app joins the last-used PC whenever it is found (at launch, when the server starts after the app, and after the connection drops); pressing Disconnect pauses it; can be turned off
- **Server window redesign** matching the app theme: rounded cards, lime Start / red Stop, status pill, large PC address with Copy button, dark title bar, app and tray icon
- Server shows each connected phone's name (learned from discovery) in the Phones list, and logs phones that are searching for a server
- Server replies `ConnectAck`, `Pong`, `ServerFull` and `NotConnected` so the app can confirm the connection
- Home/Guide button (bit 10) mapped to the Xbox 360 Guide button
- `docs/PROTOCOL.md` (wire format and discovery) and `docs/ARCHITECTURE.md`
- Flutter unit and widget tests; server tests for discovery, thread safety and reply types

### Changed
- Triggers are analog pads; the D-pad supports diagonals and sliding; all controls are multi-touch
- Renamed the `frontend/` folder to `app/` and the `CI - Frontend` workflow to `CI - App` (`ci-app.yml`)
- Closing the server window exits instead of hiding to the tray when running under the Visual Studio debugger, so a hidden instance can't lock the build output
- Server window layout scales with Windows display settings; text, buttons and cards share one alignment grid

### Removed
- Gyroscope streaming (`sensors_plus`); the gyro fields are sent as zero

### Fixed
- Release builds lacked the Android `INTERNET` permission
- Farewell packets could be dropped when the socket was closed or would block
- Discovery listed a PC with several network adapters (Wi‑Fi, WSL, VMware…) more than once; announcements now carry a server ID and its addresses, and the app connects via the address on its own subnet
- Server: session and controller lists were shared between the network thread, the heartbeat timer and the UI without locking, which could crash the client list or corrupt sessions
- Server: a phone that went away could flood the log with Windows UDP connection-reset errors
- Server: stray or short packets were logged as errors; they are now ignored
- Server: a session is dropped (and the phone told the server is full) if its virtual controller cannot be created
- Server: restarting no longer leaks a heartbeat timer, and log/UI updates no longer block worker threads
- Server: a phone's name and IP overlapped in the Phones list at higher display scaling
- Server window designer failed to load (`InitializeComponent` is now in standard designer format)

## [1.1.0] - 2026-06-14

### Added
- `frontend/` Flutter Android app: Xbox-style controller UI with dual analog sticks, D-pad, ABXY face buttons, LB/RB bumpers, LT/RT trigger sliders, Back/Start/Guide buttons
- `frontend/lib/network/protocol.dart`: 48-byte little-endian UDP packet serialization matching server `MessageSerializer`
- `frontend/lib/network/udp_client.dart`: UDP client with 60 Hz input send loop and 2-second ping keep-alive
- `frontend/lib/screens/connect_screen.dart`: Server IP/port entry screen
- `frontend/lib/screens/controller_screen.dart`: Landscape-locked full-screen controller UI with gyroscope support via `sensors_plus`

## [1.0.0] - 2026-06-12

### Added
- `Protocol` class library: `GamepadMessage`, `MessageSerializer`, `MessageType`, `Constants`
- `GamepadDriver` class library: virtual Xbox 360 controllers via ViGEmBus (`VirtualGamepad`, `VirtualGamepadManager`, `GamepadMapper`)
- `NetworkLayer` class library: UDP server, client session tracking, heartbeat monitor
- `PocketControllerServer` WinForms app: Start/Stop UI, client list, event log, system tray icon, settings persistence
