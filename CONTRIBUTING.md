# Contributing to PocketConsole

Thank you for your interest in contributing!

## Getting Started

1. Fork the repository and create your branch from `develop`.
2. Follow the [Git Flow](#git-flow) branching model.
3. Make sure the solution builds with no errors before opening a pull request.

## Git Flow

| Branch | Purpose |
|--------|---------|
| `main` | Stable releases only |
| `develop` | Integration branch for ongoing work |
| `feature/*` | New features branched from `develop` |
| `release/*` | Release preparation branched from `develop` |
| `hotfix/*` | Critical fixes branched from `main` |

## Prerequisites

- .NET 10 SDK
- [ViGEmBus driver](https://github.com/nefarius/ViGEmBus/releases) for running locally

## Building

```
cd server
dotnet build PocketConsoleServer.slnx
```

## Pull Request Guidelines

- Keep PRs focused — one feature or fix per PR.
- Update `CHANGELOG.md` under `[Unreleased]` with a summary of your change.
- Update the version number in `README.md`, `CHANGELOG.md`, and the form title in `MainForm.Designer.cs` when bumping a release.
- Do not include build outputs (`bin/`, `obj/`) or IDE files (`.vs/`) in your commit.

## Code Style

- Follow standard C# naming conventions.
- No commented-out code or `TODO` left in production files — use `TODO.md` instead.
- Keep each class in its own file, named after the class.
