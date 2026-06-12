# TODO

## In Progress

_Nothing currently in progress._

## Planned

- [ ] Android client app (sends `GamepadMessage` over UDP)
- [ ] Gyroscope / accelerometer support in Android client
- [ ] Per-client button remapping in settings UI
- [ ] Auto-start with Windows option
- [ ] Installer / setup wizard

## Completed

- [x] Protocol — packet format, serializer, constants
- [x] GamepadDriver — virtual Xbox 360 controllers via ViGEmBus
- [x] NetworkLayer — UDP server, client sessions, heartbeat monitor
- [x] WinForms server UI — Start/Stop, client list, event log, system tray
- [x] Settings persistence (JSON)
