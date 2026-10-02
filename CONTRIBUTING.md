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
- When bumping a release, update the version in all of these:
  - `CHANGELOG.md` — move `[Unreleased]` into a dated release section
  - `README.md` — the version badge and the **Version** section
  - `server/PocketControllerServer.csproj` — `Version`, `AssemblyVersion`, `FileVersion`
  - `server/MainForm.Designer.cs` — the `lblVersion` text in the window header
  - `app/pubspec.yaml` — `version: X.Y.Z+N` (increase the build number `N` too)
- Do not include build outputs (`bin/`, `obj/`) or IDE files (`.vs/`) in your commit.

## Code Style

- C#: follow standard C# naming conventions and keep each class in its own file, named after the class.
- Dart: follow the rules in `app/analysis_options.yaml` (`flutter analyze` must report no issues).
- No commented-out code or `TODO` left in production files — open an issue instead.
