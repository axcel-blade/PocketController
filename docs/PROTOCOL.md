# PocketController Wire Protocol

The mobile app talks to **PocketController Server** (`server/`, Windows) over **UDP**. The server turns each
connected phone into a virtual Xbox 360 controller through the [ViGEmBus](https://github.com/nefarius/ViGEmBus)
driver, so games see an ordinary XInput gamepad.

The app does **not** use WebSockets and is **not** compatible with DroidJoy or any other controller server.
To support a different PC bridge, implement this protocol on it.

## Transport

| Item          | Value                                   |
|---------------|-----------------------------------------|
| Transport     | UDP, unicast                            |
| Default port  | `5555` (configurable in the server)     |
| Packet size   | 48 bytes, every packet, both directions |
| Byte order    | Little-endian                           |
| Input rate    | 60 Hz, plus an immediate packet on every press/release |
| Keep-alive    | Ping every 1 s                          |
| Server timeout| Client dropped after 5 s with no packets |

## Packet layout

| Offset | Size | Type      | Field          | Notes |
|-------:|-----:|-----------|----------------|-------|
| 0      | 1    | `uint8`   | `type`         | See message types below |
| 1      | 2    | `uint16`  | `buttons`      | Bitmask, see below |
| 3      | 4    | `float32` | `leftStickX`   | -1.0 (left) … 1.0 (right) |
| 7      | 4    | `float32` | `leftStickY`   | -1.0 (down) … 1.0 (up) |
| 11     | 4    | `float32` | `rightStickX`  | -1.0 … 1.0 |
| 15     | 4    | `float32` | `rightStickY`  | -1.0 … 1.0 (up positive) |
| 19     | 4    | `float32` | `leftTrigger`  | 0.0 … 1.0 |
| 23     | 4    | `float32` | `rightTrigger` | 0.0 … 1.0 |
| 27     | 1    | `uint8`   | `dpad`         | Bitmask, see below |
| 28     | 12   | `float32`×3 | `gyroX/Y/Z`  | Reserved; the app sends 0 |
| 40     | 8    | `int64`   | `timestampMs`  | Sender clock, ms since Unix epoch |

### Buttons bitmask

| Bit | Button | Bit | Button |
|----:|--------|----:|--------|
| 0 | A  | 6  | Start |
| 1 | B  | 7  | Back |
| 2 | X  | 8  | Left stick click (not exposed in the app UI) |
| 3 | Y  | 9  | Right stick click (not exposed in the app UI) |
| 4 | LB | 10 | Home / Guide |
| 5 | RB |    |       |

### D-pad bitmask

`Up = 1`, `Down = 2`, `Left = 4`, `Right = 8`. Diagonals set two bits.

## Message types

Client → server:

| Value | Name         | Meaning |
|------:|--------------|---------|
| 0 | `Connect`    | Request a controller slot. The app resends every 600 ms for up to 3 s until answered. |
| 1 | `Disconnect` | Leave; the server removes the virtual controller. Preceded by an all-released `Input`. |
| 2 | `Input`      | Full controller state. Sent at 60 Hz and immediately on press/release/recentre. |
| 3 | `Ping`       | Keep-alive; `timestampMs` is echoed back in the `Pong`. |

Server → client (all fields zero except `type` and the echoed `timestampMs`):

| Value | Name           | Sent when |
|------:|----------------|-----------|
| 4 | `ConnectAck`   | A `Connect` created or refreshed a session. |
| 5 | `Pong`         | Reply to `Ping`. Round-trip latency = now − echoed timestamp. |
| 6 | `ServerFull`   | `Connect` refused: all 4 slots in use. |
| 7 | `NotConnected` | `Ping` from an endpoint with no session (server restarted or timed the client out). |

## LAN discovery

Discovery lets the app find servers without typing an IP. It uses short UTF-8 text packets on
**separate ports**, so it never mixes with the 48-byte controller packets above.

| Port | Direction | Purpose |
|-----:|-----------|---------|
| 5556 | phone → server | Probe: "is a server here?" The server replies to the sender. |
| 5557 | server → broadcast | Announcement, broadcast every 2 s while the server is running. |

| Packet | Format |
|--------|--------|
| Probe | `PCTRL?1\|<device name>` |
| Announcement | `PCTRL!1\|{"name":"GAMING-PC","port":5555,"clients":1,"max":4,"id":"3f9c0a1b2c4d","ips":["192.168.0.5","172.20.0.1"]}` |

* `name` is the PC's machine name, `port` is the controller port to connect to, and
  `clients`/`max` show how many controller slots are in use.
* `id` is a random ID chosen each time the server starts, and `ips` lists all of the PC's IPv4
  addresses. A PC with several adapters (Wi‑Fi, WSL, VMware, VirtualBox…) is heard from several
  source addresses, so the app groups announcements by `id` and connects through the address on
  the phone's own subnet. Both fields are optional; without them the app falls back to
  address + port.
* The app sends probes every 2 s to `255.255.255.255`, to its subnet broadcast address, and
  (every other round) to each host in its /24 subnet. The unicast sweep matters where broadcasts
  are blocked, such as on iOS without the multicast entitlement or on some routers.
* The app also listens on 5557 for announcements.
* A server is dropped from the list after 7 s without hearing from it.
* Discovery only finds servers. Connecting still uses the normal `Connect`/`ConnectAck` handshake.
* If 5556 is already in use on the PC, the server keeps running without discovery and phones can
  still connect by entering the IP.

### Auto-connect

When the app opens with **Connect automatically** on (the default), it joins:

1. the last-used server (same address and port), or
2. the last-used server by name, if its IP changed, or
3. if no server was ever saved, the only server found within 8 s, if exactly one has a free slot.

It never picks between several unknown servers, and it only auto-connects once per launch.

## Connection state in the app

* **Connecting** — after `Connect` is sent, until a reply arrives.
* **Connected** — only after `ConnectAck`. Never assumed.
* **Error** — no reply within 3 s, `ServerFull`, `NotConnected`, no reply to pings for 4 s, or a socket error.

Input is only transmitted while connected. When the app is backgrounded, every input is released.
