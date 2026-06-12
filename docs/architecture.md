# Architecture

## Project Structure

```
PocketConsoleServer.slnx
├── Protocol/               # Shared packet format (no dependencies)
├── GamepadDriver/          # Virtual controller via ViGEmBus
├── NetworkLayer/           # UDP transport + session management
└── PocketConsoleServer/    # WinForms host (entry point)
```

## Dependency Graph

```
PocketConsoleServer
    ├── Protocol
    ├── GamepadDriver → Protocol
    └── NetworkLayer  → Protocol
```

## Protocol Layer

Defines the wire format shared by server and Android client.

| Class | Responsibility |
|-------|---------------|
| `GamepadMessage` | All controller state in one struct (buttons bitmask, sticks, triggers, D-pad, gyro, timestamp) |
| `MessageSerializer` | `BinaryWriter`/`BinaryReader` ↔ `byte[]` |
| `MessageType` | Enum: Connect, Disconnect, Input, Ping |
| `Constants` | Port 5555, max 4 clients, buffer 256 B, timeouts |

## GamepadDriver Layer

Wraps [ViGEmBus](https://github.com/nefarius/ViGEmBus) to create virtual Xbox 360 controllers.

| Class | Responsibility |
|-------|---------------|
| `VirtualGamepadManager` | Dictionary of `clientId → VirtualGamepad`; create/destroy on connect/disconnect |
| `VirtualGamepad` | Owns one `IXbox360Controller`; calls `GamepadMapper.Apply` on each input frame |
| `GamepadMapper` | Translates `GamepadMessage` floats and bitmasks to ViGEm property setters |

## NetworkLayer

| Class | Responsibility |
|-------|---------------|
| `UdpServer` | Async receive loop; deserializes bytes; fires `OnMessageReceived` |
| `ClientSession` | Endpoint, assigned ID, `LastSeen` timestamp |
| `ClientManager` | GetOrAdd sessions; fires `OnClientConnected` / `OnClientDisconnected` |
| `HeartbeatMonitor` | Timer every 3 s; drops clients silent > 5 s |

## WinForms Host

```
MainForm
  └── ServerController
        ├── UdpServer          (NetworkLayer)
        ├── ClientManager      (NetworkLayer)
        ├── HeartbeatMonitor   (NetworkLayer)
        └── VirtualGamepadManager (GamepadDriver)
```

`ServerController` is the integration hub — it wires all events and drives the gamepad updates. `MainForm` only calls `Start()` / `Stop()` and renders the log and client list.
