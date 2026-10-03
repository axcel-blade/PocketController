# Support

## Getting Help

- **Bug reports & feature requests** — open an [issue](../../issues) on GitHub.
- **Questions** — open a [discussion](../../discussions) on GitHub.
- **Security issues** — see [SECURITY.md](SECURITY.md).

## Before Opening an Issue

1. Check the [README](README.md) for setup and usage instructions.
2. Make sure the [ViGEmBus driver](https://github.com/nefarius/ViGEmBus/releases) is installed — most connection issues are caused by a missing or outdated driver.
3. Read the error shown on the app's **PC connection** screen — it says what to check. Common causes:
   - The server isn't running, or **Start** wasn't clicked.
   - The phone and PC are on different networks (or the Wi-Fi isolates clients).
   - Windows Firewall is blocking the server's UDP port (default `5555`).
   - The PC doesn't show up under **On your network**: discovery uses UDP `5556`/`5557`, which the firewall
     or a "client isolation" Wi‑Fi setting may block. Entering the IP manually still works.
   - All 4 controller slots are already in use.
4. Search [existing issues](../../issues) to see if your problem has already been reported.

## Providing Good Bug Reports

Please include:
- OS version (e.g. Windows 11 22H2)
- .NET runtime version (`dotnet --version`)
- ViGEmBus driver version
- Phone model and Android / iOS version
- PocketController server version and app version and the error shown on the connection screen (if any)
- Steps to reproduce
- Expected vs actual behaviour
- Any relevant log output from the event log panel
