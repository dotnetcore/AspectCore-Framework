# Local build

This document explains how to restore, compile, and test AspectCore locally, and describes how multi-targeting and build properties (build props) are organized. All commands and configuration are based on the actual files in the repository root's `AspectCore-Framework.sln` and under `build/`, `src/`, and `tests/`.

## 1. Environment setup

The repository root has no `global.json`, so it does not pin a specific SDK version, but the target frameworks determine which SDKs you need to install:

- The test projects `AspectCore.Core.Tests` and `AspectCore.E2E.Tests` target `net10.0;net9.0;net8.0;net6.0`. To compile and run the full test suite, you need the **.NET 10 SDK** installed.
- The source packages target `net6.0` at minimum (some packages also include `netstandard2.0`/`netstandard2.1`); running a multi-target build requires the corresponding .NET 6/7/8/9 runtimes.
- CI explicitly installs the following SDKs via `actions/setup-dotnet`: `6.0.x`, `8.0.x`, `9.0.x`, `10.0.x` (see `.github/workflows/build-ci.yml` and `.github/workflows/build-pr-ci.yml`). Aligning locally with these versions covers all target frameworks.

Verify your local SDKs:

```bash
dotnet --list-sdks
dotnet --version
```

## 2. Restore, compile, test (solution scope)

Operate on the whole solution from the repository root:

```bash
# Restore NuGet dependencies
dotnet restore AspectCore-Framework.sln

# Compile (Release, consistent with CI)
dotnet build AspectCore-Framework.sln -c Release

# Run all tests
dotnet test AspectCore-Framework.sln
```

> CI does not call `dotnet build` on the solution directly; instead it iterates over every `*.csproj` under `./src` and `./tests` and runs `build`/`test` one by one (see the `Build`/`Run Tests` steps in `build-ci.yml`). Solution-scope commands are more convenient locally; if you need to reproduce CI exactly, run at per-project granularity.

## 3. Running per project / by condition (a narrower scope)

When debugging a single module, point directly at the project file to avoid a full build:

```bash
# Build only the core package
dotnet build src/AspectCore.Core/AspectCore.Core.csproj -c Release

# Test only the core unit-test project
dotnet test tests/AspectCore.Core.Tests/AspectCore.Core.Tests.csproj

# Run only the dual-engine parity (EngineParity) cases
dotnet test tests/AspectCore.Core.Tests/AspectCore.Core.Tests.csproj \
  --filter "FullyQualifiedName~EngineParity"

# Test on a single target framework to shorten the feedback loop
dotnet test tests/AspectCore.Core.Tests/AspectCore.Core.Tests.csproj -f net8.0
```

For more filtering and coverage-collection examples, see [Running tests](../testing/running-tests.md).

## 4. Notes on target frameworks

Different projects choose target frameworks by purpose; the authoritative source is each `*.csproj`:

| Project | Target frameworks | Notes |
|------|----------|------|
| `AspectCore.Abstractions`, `AspectCore.Core`, `AspectCore.Extensions.Reflection` | `net9.0;net8.0;net7.0;net6.0;netstandard2.1;netstandard2.0` | The core packages multi-target, compatible with .NET Framework (via netstandard2.0) |
| `AspectCore.SourceGenerator` | `netstandard2.0` | The compile-time engine must target `netstandard2.0` for Roslyn to load; `LangVersion=latest` |
| `AspectCore.Extensions.AspNetCore` | `net9.0;net8.0;net7.0;net6.0` | `net6.0` and above only |
| Container/host and other extension packages | Per each `*.csproj` (mostly `net6.0` and above, including netstandard targets) | See [Project structure](./project-structure.md) and [Module and package structure design](../architecture/module-design.md) |
| Test projects | Mostly `net10.0;net9.0;net8.0;net6.0` or `net9.0;net8.0;net6.0` | For the specific differences, see [Testing strategy](../testing/testing-strategy.md) |

A multi-target build produces one assembly per target framework; therefore, missing a runtime locally will cause the compile or test step for that target framework to fail.

## 5. Build properties (build props) layout

Build configuration is centralized in the `build/` directory and two `Directory.Build.props`, imported by each `*.csproj` via `Import`:

- `build/version.props` — the product version. Currently `VersionMajor=2`, `VersionMinor=7`, `VersionPatch=0`, with `VersionQuality` empty, so `VersionPrefix=2.7.0`. When there is no Git tag, CI appends `-preview-<timestamp>`.
- `build/common.props` — common package metadata (`Authors=Lemon`, `Product=AspectCore Framework`, repository URL, etc.), and it `Import`s `sign.props` and `version.props`. It sets `LangVersion=10.0`; the comment explains: 10.0 is the latest stable C# version supported by the lowest target framework `net6.0`. The core package source is written under this constraint.
- `build/sign.props` and `build/aspectcore.snk` — strong-name signing configuration and key.
- `src/Directory.Build.props` — enables .NET analyzers for all projects under `src/` (`EnableNETAnalyzers=true`, `AnalysisLevel=latest`, `AnalysisMode=Default`, `EnforceExtendedAnalyzerRules=true`). These are advisory diagnostics, not a hard failure gate.
- `tests/Directory.Build.props` — uniformly brings in `coverlet.msbuild` (version `6.0.2`, `PrivateAssets=all`) for all test projects, used for coverage collection.

> The repository root has no `Directory.Build.props`; the `src/` and `tests/` props apply only to their respective subdirectories. `AspectCore.SourceGenerator` separately overrides `LangVersion` to `latest`.

## Related docs

- [Project structure](./project-structure.md) — the layout of the source, test, sample, and benchmark directories
- [Contributing guide](./contributing.md) — branch, commit, and PR process
- [Running tests](../testing/running-tests.md) — test filtering and coverage collection
- [Module and package structure design](../architecture/module-design.md) — the responsibilities and dependency directions of the 14 packages
- [Docs home](../README.md)
