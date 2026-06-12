# Setup Guide

## Requirements

| Requirement | Notes |
|-------------|-------|
| Windows 10 / 11 | 64-bit |
| [.NET 10 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0) | Desktop runtime |
| [ViGEmBus driver](https://github.com/nefarius/ViGEmBus/releases) | Required for virtual controllers |

## Installing ViGEmBus

1. Download the latest `ViGEmBus_Setup_*.exe` from the [releases page](https://github.com/nefarius/ViGEmBus/releases).
2. Run the installer and follow the prompts.
3. Reboot if prompted.

## Building from Source

```
git clone https://github.com/axcelblade/PocketConsole.git
cd PocketConsole/server
dotnet build PocketConsoleServer.slnx
```

The output binary is at `server/bin/Debug/net10.0-windows/PocketConsoleServer.exe`.

## Running the Server

1. Launch `PocketConsoleServer.exe`.
2. The server window shows your local IP address.
3. Set the port if needed (default: **5555**).
4. Click **Start**.
5. The app minimises to the system tray when you close the window — right-click the tray icon to exit fully.

## Firewall

Windows Firewall will prompt on first launch. Allow access on **private networks** so your phone can reach the server over Wi-Fi.
