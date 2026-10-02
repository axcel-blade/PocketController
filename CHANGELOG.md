# Changelog

## [Unreleased]

### Added
- LAN discovery: the server answers probes (UDP 5556) and broadcasts announcements (UDP 5557); the app lists servers found on the network, connects with one tap, and auto-connects to the last-used PC at launch (can be turned off). Manual IP entry is unchanged
- Server logs phones that are searching for a server
- Discovery lists a PC with several network adapters once (announcements carry a server ID and its addresses; the app connects via the address on its own subnet)
- Server window redesign matching the app theme (graphite, lime, cyan): rounded cards, lime Start / red Stop buttons, status pill, large PC address with Copy button, client cards, dark title bar, app and tray icon
- Mobile app redesign (Android + iOS): graphite/lime/cyan theme, Classic/Racing/Compact layouts, custom layout editor with save/select/delete, persisted settings, haptics toggle, accessibility labels, small-screen sizing
- Connection screen with verified status, round-trip latency and actionable error messages
- Server replies `ConnectAck`, `Pong`, `ServerFull` and `NotConnected` so the app can confirm the connection
- Home/Guide button (bit 10) mapped to the Xbox 360 Guide button
- `docs/PROTOCOL.md` describing the UDP wire format
- Flutter unit and widget tests

### Changed
- Triggers are now analog pads; D-pad supports diagonals and sliding; all controls are multi-touch

### Changed
- Renamed the `frontend/` folder to `app/` and the `CI - Frontend` workflow to `CI - App` (`ci-app.yml`)

### Removed
- Gyroscope streaming (`sensors_plus`); the gyro fields are sent as zero

### Fixed
- Release builds lacked the Android `INTERNET` permission
- Farewell packets could be dropped when the socket was closed or would block
- Server: session and controller lists were shared between the network thread, the heartbeat timer and the UI without locking, which could crash the client list or corrupt sessions
- Server: a phone that went away could flood the log with Windows UDP connection-reset errors
- Server: stray or short packets were logged as errors; they are now ignored
- Server: a session is dropped (and the phone told the server is full) if its virtual controller cannot be created
- Server: restarting no longer leaks a heartbeat timer, and log/UI updates no longer block worker threads

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
