# PocketController Architecture

PocketController has two parts that talk over the local network:

```
┌──────────────────────────────┐   UDP, 48-byte packets    ┌──────────────────────────────┐   ViGEmBus   ┌──────────┐
│ Mobile app (app/)            │ ───── Connect/Input/Ping ─▶│ PocketController Server      │ ───────────▶ │ Game     │
│ Flutter · Android + iOS      │ ◀─ ConnectAck/Pong/Errors ─│ (server/) · .NET 10 · Windows│  virtual X360│ (XInput) │
└──────────────────────────────┘                           └──────────────────────────────┘              └──────────┘
```

- The **mobile app** draws the controller, turns touches into a gamepad state and sends it.
- The **server** keeps one session per phone, and gives each one a virtual Xbox 360 controller
  through the [ViGEmBus](https://github.com/nefarius/ViGEmBus) driver. Games see a normal XInput pad.

The wire format is specified in [PROTOCOL.md](PROTOCOL.md).

## Mobile app (`app/lib/`)

| Path | Responsibility |
|------|----------------|
| `main.dart` | Loads settings, creates the shared `GamepadState` and `BridgeConnection`, starts the app. |
| `theme.dart` | Colour palette (graphite, lime accent, cyan detail) and Material theme. |
| `models/gamepad_state.dart` | Live input state: button and D-pad bitmasks, stick axes (-1…1), triggers (0…1). Fires `onDiscreteChange` on press/release/recentre. |
| `models/control_layout.dart` | `ControlKind`, `ControlPlacement` (normalized position + size) and `ControlLayout`; the Classic, Racing and Compact built-ins; JSON (de)serialization. |
| `services/app_settings.dart` | Persists bridge address/port, haptics, the selected layout and custom layouts with `shared_preferences`. |
| `network/protocol.dart` | Serializes packets and parses server replies. |
| `network/discovery.dart` | LAN discovery: sends probes, listens for announcements, keeps the list of servers found. |
| `network/auto_connect.dart` | Runs discovery while disconnected and joins a known server automatically at launch (`pickAutoConnectTarget`). |
| `network/bridge_connection.dart` | UDP socket, handshake, 60 Hz send loop, pings, latency, timeouts and error messages. Exposes `BridgeStatus`. |
| `screens/controller_screen.dart` | Main screen: toolbar, control surface and layout editor. Releases all inputs when the app loses focus. |
| `screens/connection_screen.dart` | Address/port entry, connect/disconnect, status, errors, haptics toggle, compatibility notes. |
| `widgets/controls.dart` | Individual controls: `PadButton`, `FaceButtons`, `DpadControl`, `AnalogStick`, `TriggerPad`; haptics helper. |
| `widgets/control_surface.dart` | Positions each control of a layout (`placementRect`), enforces minimum touch targets, handles drag/select in edit mode. |
| `widgets/layout_sheet.dart` | Layout picker (select/delete) and save-layout dialog. |
| `widgets/status_chip.dart` | Always-visible connection indicator. |

### Input flow

```
touch ─▶ control widget (raw pointer events) ─▶ GamepadState ─┬─▶ onDiscreteChange ─▶ BridgeConnection.sendNow()
                                                              └─▶ 60 Hz timer ──────▶ BridgeConnection Input packet
```

- Controls use `Listener` pointer events rather than gesture detectors, so several controls can be
  held at the same time.
- The protocol sends the whole state every time, so a lost packet is corrected by the next one
  (at most ~16 ms later). Press and release events are also sent right away.
- Important packets are retried for a short time if the socket is busy (`send` returns 0).

### Connection lifecycle

```
disconnected ──connect()──▶ connecting ──ConnectAck──▶ connected
      ▲                         │                          │
      │                    timeout / ServerFull       no Pong for 4 s / NotConnected / socket error
      │                         ▼                          ▼
      └────disconnect()──── error ◀─────────────────────────┘
```

The app never assumes it is connected. It only sends input while `connected`. On disconnect it
sends an all-released input and then `Disconnect`, so no button stays held on the PC.

### Layouts

A layout is a list of placements. Each placement has a centre (`x`, `y` as fractions of the
control area) and a `size` (a fraction of the area height). `placementRect` turns this into
pixels, applies minimum sizes and keeps every control on screen, so one layout works on phones of
different sizes. Every layout contains every control. Custom layouts are stored as JSON. If a
saved layout is missing a control, it is filled in from Classic.

## Server (`server/`)

| Project / file | Responsibility |
|----------------|----------------|
| `Protocol/` | `GamepadMessage`, `MessageType`, `MessageSerializer` (48-byte little-endian format) and `Constants`. |
| `NetworkLayer/UdpServer.cs` | Receive loop on a background task, plus `Send` for replies. |
| `NetworkLayer/ClientManager.cs` / `ClientSession.cs` | One session per endpoint, up to 4 clients. |
| `NetworkLayer/DiscoveryService.cs` | Answers discovery probes (UDP 5556) and broadcasts announcements (UDP 5557) while running. |
| `Protocol/DiscoveryMessage.cs` | Builds and parses the discovery text packets. |
| `NetworkLayer/HeartbeatMonitor.cs` | Removes sessions that have been quiet for 5 s. |
| `GamepadDriver/VirtualGamepadManager.cs` | One ViGEm Xbox 360 controller per session. |
| `GamepadDriver/GamepadMapper.cs` | Maps packet fields to Xbox buttons, axes and triggers. |
| `ServerController.cs` | Connects the network layer to the gamepad driver; handles messages and sends replies. |
| `MainForm.cs`, `TrayManager.cs`, `SettingsManager.cs` | WinForms UI, tray icon and saved settings (such as the port). |
| `Tests/` | xUnit tests for the protocol and network layer. |

### Discovery

```
phone ── probe "PCTRL?1|Pixel 8" ──▶ UDP 5556 (server)      server ── "PCTRL!1|{…}" ──▶ phone
server ── broadcast every 2 s ─────▶ UDP 5557 (phones listening)
```

The server starts discovery after the controller port is open, and logs each phone that is
searching (at most once every 30 s per phone). On the phone, `AutoConnector` keeps discovery
running whenever it isn't connected, stops it once connected, and auto-joins a known server
once per launch. Manual IP entry always stays available. See [PROTOCOL.md](PROTOCOL.md#lan-discovery).

### Message handling (`ServerController.HandleMessage`)

| Incoming | Has a session? | Action |
|----------|----------------|--------|
| `Connect` | creates one if a slot is free | reply `ConnectAck`, or `ServerFull` |
| `Input` | yes | update the virtual controller |
| `Ping` | yes | reply `Pong` (echoes the timestamp) |
| `Ping` | no | reply `NotConnected` |
| `Disconnect` | yes | remove the session and its controller |
| anything else | no | ignore |

## Keeping the two sides in sync

The packet layout and the button bit positions are defined in two places:
`server/Protocol/` and `app/lib/network/protocol.dart` + `models/gamepad_state.dart`.
Change both in the same PR, update [PROTOCOL.md](PROTOCOL.md), and run both test suites.

## Testing

- `server/Tests` — serializer round-trips, reply type values, client/session management,
  discovery packets and a probe/reply round trip over loopback.
- `app/test/protocol_test.dart` — packet layout, reply parsing, and `BridgeConnection` against
  a fake UDP server on loopback (handshake, press/release delivery, timeout, server full).
- `app/test/discovery_test.dart` — discovery packet format, finding a fake server on loopback,
  and the auto-connect selection rules.
- `app/test/layout_and_ui_test.dart` — layout completeness, small-screen fit, persistence, and
  widget tests for buttons, multi-touch sticks, the D-pad and triggers.
