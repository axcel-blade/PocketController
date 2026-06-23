# Changelog

## [1.1.1] - 2026-06-23

### Changed
- `frontend/lib/screens/controller_screen.dart`: repositioned the on-screen controls to better match the Xbox controller reference layout and removed the decorative oval background
- `frontend/lib/widgets/face_buttons.dart`: tightened ABXY diamond spacing to better match the Xbox button cluster
- Updated project version references across the Flutter app, README, roadmap, and server UI label

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
