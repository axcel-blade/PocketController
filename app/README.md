# PocketController — Mobile App (`app/`)

Touch-first game controller for **Android and iOS**, built with Flutter. It sends input to
**PocketController Server** (`../server`, Windows), which exposes each phone as a virtual Xbox 360 /
XInput controller via the ViGEmBus driver.

> The app on its own cannot control PC games. It needs PocketController Server running on the PC.
> It is not compatible with DroidJoy or other controller servers. Protocol: [`docs/PROTOCOL.md`](../docs/PROTOCOL.md).

## Features

- D-pad (8-way), A/B/X/Y, two analog sticks (normalized axes, spring back to centre), LB/RB,
  analog LT/RT (press lower for a stronger pull), Back / Home / Start.
- Built-in **Classic**, **Racing** and **Compact** layouts; drag-and-resize editor to save custom
  layouts; select or delete them from the layout picker. Stored on the device.
- Connection screen with address/port entry, connect/disconnect, live status, latency and specific
  error messages. "Connected" is only shown after the server acknowledges the handshake.
- LAN discovery: servers on the same Wi‑Fi are listed under **On your network**; tap one to connect.
- Auto-connect (on by default): rejoins your last PC whenever it is found — at launch, when the
  server starts later, or after the connection drops. Pressing Disconnect pauses it.
- Haptic feedback (toggleable), screen-reader labels, minimum 44 px touch targets, multi-touch.
- Settings (address, port, server name, auto-connect, haptics, selected layout, custom layouts)
  persist via `shared_preferences`.

## Run

```sh
flutter pub get
flutter run
```

## Test

```sh
flutter analyze
flutter test
```

Tests include a fake UDP bridge on loopback that verifies the handshake, press/release delivery,
timeouts and the server-full response, plus discovery against a fake server (including one heard
from several addresses) and the auto-connect selection rules.
