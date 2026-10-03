# Contributing to PocketController

Thank you for your interest in contributing!

## Getting Started

1. Fork the repository and create your branch from `develop`.
2. Follow the [Git Flow](#git-flow) branching model.
3. Make sure the server and the mobile app build, analyze and test cleanly before opening a pull request.

## Git Flow

| Branch | Purpose |
|--------|---------|
| `main` | Stable releases only |
| `develop` | Integration branch for ongoing work |
| `feature/*` | New features branched from `develop` |
| `release/*` | Release preparation branched from `develop` |
| `hotfix/*` | Critical fixes branched from `main` |

## Prerequisites

- .NET 10 SDK (server)
- [ViGEmBus driver](https://github.com/nefarius/ViGEmBus/releases) for running the server locally
- Flutter 3.x / Dart 3.x (mobile app); Android SDK for Android builds, Xcode on macOS for iOS builds

## Building

### Server

```
cd server
dotnet build PocketControllerServer.slnx
dotnet test Tests
```

### Mobile app

```
cd app
flutter pub get
flutter analyze
flutter test
flutter run
```

## Protocol Changes

The app and server share a 48-byte UDP packet format documented in [docs/PROTOCOL.md](docs/PROTOCOL.md).
If you change it, update `server/Protocol/`, `app/lib/network/protocol.dart`, the tests on both
sides, and the protocol document in the same PR.

## Pull Request Guidelines

- Keep PRs focused — one feature or fix per PR.
- Update `CHANGELOG.md` under `[Unreleased]` with a summary of your change.
- The server and the mobile app are versioned **independently** — only bump the one you changed.
  They stay compatible as long as both follow the same wire format in `docs/PROTOCOL.md`, not by sharing a version number.
- When releasing the **server**, update:
  - `CHANGELOG.md` — move its `[Unreleased]` entries into a dated `## [Server X.Y.Z] - YYYY-MM-DD` section
  - `README.md` — the server version badge and the **Versions** section
  - `server/PocketControllerServer.csproj` — `Version`, `AssemblyVersion`, `FileVersion`
  - `server/MainForm.Designer.cs` — the `lblVersion` text in the window header
  - Tag the release `server-vX.Y.Z`
- When releasing the **mobile app**, update:
  - `CHANGELOG.md` — move its `[Unreleased]` entries into a dated `## [App X.Y.Z] - YYYY-MM-DD` section
  - `README.md` — the app version badge and the **Versions** section
  - `app/pubspec.yaml` — `version: X.Y.Z+N` (always increase the build number `N`, even for a patch)
  - Tag the release `app-vX.Y.Z`
- Do not include build outputs (`bin/`, `obj/`) or IDE files (`.vs/`) in your commit.

## Code Style

- C#: follow standard C# naming conventions and keep each class in its own file, named after the class.
- Dart: follow the rules in `app/analysis_options.yaml` (`flutter analyze` must report no issues).
- No commented-out code or `TODO` left in production files — open an issue instead.
