# Running tests

This document gives the specific commands for running and filtering AspectCore tests locally and for collecting coverage. For test categories and coverage thresholds, see [Testing strategy](./testing-strategy.md).

## 1. Running the full test suite

Run against the whole solution from the repository root:

```bash
dotnet test AspectCore-Framework.sln
```

> CI actually iterates over every `*.csproj` under `./tests` and runs `dotnet test --configuration Release` one by one (see `.github/workflows/build-pr-ci.yml`). Solution-scope commands are more convenient locally; to get closer to CI, add `-c Release`.

## 2. Running a single test project only

```bash
# Core unit tests
dotnet test tests/AspectCore.Core.Tests/AspectCore.Core.Tests.csproj

# End-to-end tests
dotnet test tests/AspectCore.E2E.Tests/AspectCore.E2E.Tests.csproj

# A particular container integration test
dotnet test tests/AspectCore.Extensions.Autofac.Test/AspectCore.Extensions.Autofac.Test.csproj
```

## 3. Filtering cases by name (`--filter`)

Use `FullyQualifiedName~` for substring matching to run only the cases you care about:

```bash
# Run only the dual-engine parity (EngineParity) tests
dotnet test tests/AspectCore.Core.Tests/AspectCore.Core.Tests.csproj \
  --filter "FullyQualifiedName~EngineParity"

# Run only the ref return-value parity tests
dotnet test tests/AspectCore.Core.Tests/AspectCore.Core.Tests.csproj \
  --filter "FullyQualifiedName~RefReturnParityTests"

# Run only a particular E2E scenario
dotnet test tests/AspectCore.E2E.Tests/AspectCore.E2E.Tests.csproj \
  --filter "FullyQualifiedName~AsyncScenarios"
```

## 4. Specifying a target framework (`-f`)

The test projects multi-target and by default run once on every target framework. Add `-f` to run on a single framework and shorten the feedback loop:

```bash
# net8.0 only (Core.Tests targets net10.0;net9.0;net8.0;net6.0)
dotnet test tests/AspectCore.Core.Tests/AspectCore.Core.Tests.csproj -f net8.0

# net9.0 only
dotnet test tests/AspectCore.E2E.Tests/AspectCore.E2E.Tests.csproj -f net9.0
```

> Only a framework that is declared in the target-framework list and has its runtime installed locally can run. For each test project's target frameworks, see [Testing strategy](./testing-strategy.md).

## 5. Collecting coverage

The test projects already bring in `coverlet.msbuild` via `tests/Directory.Build.props`, so you can collect cobertura coverage directly with MSBuild properties:

```bash
dotnet test tests/AspectCore.Core.Tests/AspectCore.Core.Tests.csproj -f net9.0 \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=cobertura \
  /p:CoverletOutput=./TestResults/
```

CI uses `.github/scripts/check-coverage.sh` to collect and assert the thresholds on `net9.0` (unit 95% / E2E 80%). Locally you can reuse the script directly for a dry run:

```bash
# Collect unit-test coverage results
./.github/scripts/check-coverage.sh collect unit --output coverage-results/unit.env
# Assert whether the target is met
./.github/scripts/check-coverage.sh assert unit --input coverage-results/unit.env

# E2E works the same way
./.github/scripts/check-coverage.sh collect e2e --output coverage-results/e2e.env
./.github/scripts/check-coverage.sh assert e2e --input coverage-results/e2e.env
```

> The script generates `TestResults/*.cobertura.xml` under each test project directory and takes `line-rate`. It filters coverage by source assembly; E2E counts only `AspectCore.Core` and `AspectCore.Abstractions` (for details, see [Testing strategy](./testing-strategy.md)).

## Related docs

- [Testing strategy](./testing-strategy.md) — test categories and coverage thresholds
- [Local build](../development/building.md) — restore, compile, target frameworks
- [Contributing guide](../development/contributing.md) — pre-commit verification and PR required checks
- [Docs home](../README.md)
