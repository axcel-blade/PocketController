# Protocol Reference

## Transport

- **UDP**, default port **5555**
- Max payload: 256 bytes
- Little-endian byte order (BinaryWriter default)

## Packet Layout

All packets share the same `GamepadMessage` struct. Fields are written in this order:

| Offset | Size | Type | Field |
|--------|------|------|-------|
| 0 | 1 | `byte` | `MessageType` |
| 1 | 2 | `ushort` | `Buttons` bitmask |
| 3 | 4 | `float` | `LeftStickX` |
| 7 | 4 | `float` | `LeftStickY` |
| 11 | 4 | `float` | `RightStickX` |
| 15 | 4 | `float` | `RightStickY` |
| 19 | 4 | `float` | `LeftTrigger` |
| 23 | 4 | `float` | `RightTrigger` |
| 27 | 1 | `byte` | `DPad` bitmask |
| 28 | 4 | `float` | `GyroX` |
| 32 | 4 | `float` | `GyroY` |
| 36 | 4 | `float` | `GyroZ` |
| 40 | 8 | `long` | `TimestampMs` |

**Total: 48 bytes**

## MessageType Values

| Value | Name | Description |
|-------|------|-------------|
| 0 | Connect | First packet from client; server registers session |
| 1 | Disconnect | Client is leaving gracefully |
| 2 | Input | Controller state update |
| 3 | Ping | Heartbeat; keeps session alive |

## Buttons Bitmask

| Bit | Button |
|-----|--------|
| 0 | A |
| 1 | B |
| 2 | X |
| 3 | Y |
| 4 | Left Bumper (LB) |
| 5 | Right Bumper (RB) |
| 6 | Start |
| 7 | Back |
| 8 | Left Stick click |
| 9 | Right Stick click |

## D-Pad Bitmask

| Bit | Direction |
|-----|-----------|
| 0 | Up |
| 1 | Down |
| 2 | Left |
| 3 | Right |

## Value Ranges

| Field | Range |
|-------|-------|
| Sticks (X/Y) | `-1.0` to `1.0` |
| Triggers | `0.0` to `1.0` |
| Gyro (°/s) | unbounded float |
