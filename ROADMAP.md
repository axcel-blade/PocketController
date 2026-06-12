# Roadmap

## v1.0.0 — Server Foundation ✅
- Protocol layer (UDP packet format + serialization)
- Virtual Xbox 360 controllers via ViGEmBus
- UDP server with client session management and heartbeat
- WinForms host with system tray, event log, and settings persistence

## v1.1.0 — Android Client
- Android app that reads touch/gyro input and sends `GamepadMessage` packets
- On-screen virtual gamepad layout
- Connection screen (IP + port entry)

## v1.2.0 — Polish
- Per-client button remapping
- Auto-start with Windows
- Configurable dead zones for joysticks
- Installer / MSIX package

## v2.0.0 — DualShock / DS4 Support
- Add `GamepadDriver` support for virtual DualShock 4 controllers
- Protocol extension for touchpad and light-bar colour
- Client app profile switcher (Xbox / DS4)
