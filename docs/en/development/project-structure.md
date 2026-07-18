# Project structure

This document explains the top-level directory layout of the AspectCore repository and the responsibilities of each directory. Package-level responsibility boundaries, public entry points, and dependency directions are not repeated here; for those, see [Module and package structure design](../architecture/module-design.md).

## 1. Top-level directory overview

| Directory | Purpose |
|------|------|
| `src/` | The 14 publishable source packages (core + extensions + compile-time engine) |
| `tests/` | xUnit test projects: unit, dual-engine parity, E2E, reflection, and per-container integration |
| `sample/` | Runnable sample projects |
| `benchmark/` | The early BenchmarkDotNet benchmark projects (Core, Reflection) |
| `benchmarks/` | The new unified benchmark project `AspectCore.Benchmarks` |
| `docs/` | This documentation (primarily Chinese; English is under `docs/en/`) |
| `build/` | Version, signing, common package properties, and Cake build scripts |
| `.github/` | CI workflows and the coverage script |

The root also contains the solution and workspace files: `AspectCore-Framework.sln`, `NuGet.config`, `LICENSE`, `README.md`, `build.cake`/`build.ps1`.

## 2. `src/` — source packages

There are 14 packages under `src/` (excluding `Directory.Build.props`). They fall into three roles; the responsibilities and dependency directions of each package are in [Module and package structure design](../architecture/module-design.md).

- Core: `AspectCore.Abstractions`, `AspectCore.Core`, `AspectCore.Extensions.Reflection`
- Compile-time engine: `AspectCore.SourceGenerator` (`netstandard2.0`, loaded by Roslyn)
- Extensions and integrations: `AspectCore.Extensions.DependencyInjection`, `AspectCore.Extensions.Autofac`, `AspectCore.Extensions.Windsor`, `AspectCore.Extensions.LightInject`, `AspectCore.Extensions.Hosting`, `AspectCore.Extensions.AspNetCore`, `AspectCore.Extensions.Configuration`, `AspectCore.Extensions.DataValidation`, `AspectCore.Extensions.DataAnnotations`, `AspectCore.Extensions.AspectScope`

`src/Directory.Build.props` enables .NET analyzers for all source projects (advisory, not blocking the build).

## 3. `tests/` — test projects

Each project under `tests/` corresponds to a category of test target; for test categories and coverage thresholds, see [Testing strategy](../testing/testing-strategy.md).

| Test project | What it covers |
|----------|----------|
| `AspectCore.Core.Tests` | Core unit tests. Includes the `EngineParity/` subdirectory, which verifies that the DynamicProxy and Source Generator engines behave consistently (e.g. `RefReturnParityTests`, `InitRequiredMembersParityTests`, record types, etc.); it also has subdirectories such as `DynamicProxy/`, `DependencyInjection/`, `Injector/`, `Configuration/`, `Integrate/`, `Issues/`, `Extensions/`, `Utils/` |
| `AspectCore.E2E.Tests` | End-to-end scenario tests, with cases concentrated in `Scenarios/` and shared support in `Fixtures/` (`TestHost.cs`, `TestServices.cs`) |
| `AspectCore.Extensions.Reflection.Test` | Reflection-extension tests |
| `AspectCore.Extensions.Autofac.Test`, `AspectCore.Extensions.Windsor.Test`, `AspectCore.Extensions.LightInject.Test`, `AspectCore.Extensions.Hosting.Tests`, `AspectCore.Extensions.DependencyInjection.Test`, `AspectCore.Extensions.Configuration.Tests` | Integration tests for each container / host / configuration |

`tests/Directory.Build.props` uniformly brings in `coverlet.msbuild` coverage collection for all test projects.

## 4. `sample/` — samples

Runnable demo projects that showcase typical usage:

- `AspectCore.Extensions.DependencyInjection.ConsoleSample`
- `AspectCore.Extensions.Autofac.Sample`
- `AspectCore.Extensions.DataAnnotations.Sample`
- `AspectCore.Extensions.AspectScope.Sample`

## 5. `benchmark/` and `benchmarks/`

The repository has two benchmark directories:

- `benchmark/` — the early benchmarks: `AspectCore.Core.Benchmark`, `AspectCore.Extensions.Reflection.Benchmark`
- `benchmarks/` — the new unified benchmark project `AspectCore.Benchmarks`

## 6. `build/` — build configuration

Centralized management of version, signing, and common package properties: `version.props` (product version 2.7.0), `common.props` (package metadata + `LangVersion=10.0`), `sign.props` + `aspectcore.snk` (strong-name signing), and the Cake scripts (`index.cake`, `util.cake`, `version.cake`). For details, see [Local build](./building.md).

## 7. `.github/` — CI

- `workflows/build-ci.yml` — build, packaging, and MyGet publishing on push to `master`
- `workflows/build-pr-ci.yml` — PR validation: lint, `build-and-test` (ubuntu/windows), unit and E2E execution and coverage gates, CodeQL
- `workflows/release.yml` — the release process
- `scripts/check-coverage.sh` — the coverage collection and threshold-assertion script

For CI details, see [Testing strategy](../testing/testing-strategy.md) and [Contributing guide](./contributing.md).

## Related docs

- [Module and package structure design](../architecture/module-design.md) — the responsibility boundaries and dependency directions of the 14 packages
- [Local build](./building.md) — restore, compile, target frameworks, and build properties
- [Testing strategy](../testing/testing-strategy.md) — test categories and coverage thresholds
- [Docs home](../README.md)
