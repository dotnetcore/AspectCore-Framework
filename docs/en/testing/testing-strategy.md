# Testing strategy

This document explains AspectCore's test categories, the purpose of each category, and the coverage thresholds and required checks in CI. All test projects are located in `tests/` and are based on xUnit (`xunit` 2.9.2).

## 1. Test categories

### Unit tests (`AspectCore.Core.Tests`)

Cover the unit behavior of the core runtime: dynamic proxy generation, the interceptor pipeline, dependency injection and the injector, configuration, and so on. The directories are divided by topic (`DynamicProxy/`, `DependencyInjection/`, `Injector/`, `Configuration/`, `Integrate/`, `Issues/`, `Extensions/`, `Utils/`, etc.). The target frameworks are `net10.0;net9.0;net8.0;net6.0`.

### Dual-engine parity tests (`AspectCore.Core.Tests/EngineParity/`)

AspectCore provides two equivalent proxy-generation engines: the runtime DynamicProxy and the compile-time Source Generator. The two share the same interception semantics, so they **must behave identically** — for the same types and methods, the interception result should be the same no matter which engine generated the proxy. `EngineParity/` exists precisely to guard this invariant, covering language features that tend to diverge between the two implementations, for example:

- `RefReturnParityTests` — `ref` / `ref readonly` return values
- `InitRequiredMembersParityTests` — `init` and `required` members
- `RefStructAndScopedParityTests` — `ref struct` and `scoped`
- `PrimaryConstructorAndParamsCollectionParityTests` — primary constructors and `params` collections
- `InterpolatedStringHandlerAndIndexRangeParityTests` — interpolated string handlers and `Index`/`Range`
- record type parity
- `SourceGeneratorDynamicProxyParityTests`, `SourceGeneratorEdgeCaseTests`, `SourceGeneratorDiagnosticTests`, etc.

For the differences between the two engines and how to choose, see [Comparing and choosing between the two engines](../architecture/engine-comparison.md).

### End-to-end tests (`AspectCore.E2E.Tests/Scenarios`)

Complete scenario tests organized from the user's perspective, with cases concentrated in `Scenarios/` (such as basic proxying, async `Task`/`ValueTask`, DI integration, configuration-driven, error handling, generic services, property injection, record types, `ref` returns, etc.), and shared hosts and services in `Fixtures/` (`TestHost.cs`, `TestServices.cs`). The target frameworks are `net10.0;net9.0;net8.0;net6.0`.

### Reflection tests (`AspectCore.Extensions.Reflection.Test`)

Cover the `AspectCore.Extensions.Reflection` high-performance reflection extensions. The target frameworks are `net9.0;net8.0;net6.0`.

### Per-container / host integration tests

Verify AspectCore's integration with third-party containers and hosts:

- `AspectCore.Extensions.Autofac.Test`, `AspectCore.Extensions.Windsor.Test`, `AspectCore.Extensions.LightInject.Test`, `AspectCore.Extensions.Hosting.Tests`, `AspectCore.Extensions.Configuration.Tests` — `net9.0;net8.0;net6.0`
- `AspectCore.Extensions.DependencyInjection.Test` — `net9.0;net6.0`

## 2. Coverage tooling

All test projects uniformly bring in `coverlet.msbuild` (`6.0.2`) via `tests/Directory.Build.props` to collect coverage. CI uses `.github/scripts/check-coverage.sh` to collect line coverage in cobertura format on `net9.0` and assert the thresholds.

Collection scope (see the `check-coverage.sh` script):

- Unit coverage: iterate over the `*.csproj` under `./tests`, excluding projects whose name contains `E2E`; each test project is filtered by its corresponding source assembly (e.g. `AspectCore.Extensions.Windsor.Test` → `[AspectCore.Extensions.Windsor]*`), then the arithmetic mean of each project's coverage is taken.
- E2E coverage: only `*E2E*.csproj`, and deliberately counts only `[AspectCore.Core]` and `[AspectCore.Abstractions]` (extension assemblies are measured separately by their own unit-test projects).

## 3. Coverage thresholds and required CI checks

PRs are validated by `.github/workflows/build-pr-ci.yml`, and the coverage thresholds are defined in `check-coverage.sh`:

- Unit-test coverage threshold: **95%** (`UT_THRESHOLD=95`)
- E2E-test coverage threshold: **80%** (`E2E_THRESHOLD=80`)

Branch protection requires all of the following status checks to pass:

- `lint`
- `build-and-test (ubuntu-latest)`
- `build-and-test (windows-latest)`
- `Unit Test Execution`
- `Unit Test Coverage Result`
- `E2E Test Execution`
- `E2E Test Coverage Result`

Coverage is split into two independent jobs, "execution" and "threshold assertion": `*-test-execution` runs the tests and collects the results, and `*-test-coverage-result` asserts whether the target is met. In addition, PRs also run CodeQL (C#) analysis. For the merge policy and review requirements, see [Contributing guide](../development/contributing.md).

## Related docs

- [Running tests](./running-tests.md) — how to run and filter tests, and collect coverage
- [Comparing and choosing between the two engines](../architecture/engine-comparison.md) — the background for dual-engine parity
- [Contributing guide](../development/contributing.md) — the PR process and required checks
- [Project structure](../development/project-structure.md) — where the test projects sit in the repository
- [Docs home](../README.md)
