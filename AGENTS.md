# AGENTS.md — AspectCore-Framework

Project-level AI context for the AspectCore-Framework repository. Generated from the current code tree (commit `d7750bf`, version `2.7.0`). Keep this file lean; link to external docs instead of inlining them.

---

## 1. Project Overview

**AspectCore-Framework** is an Aspect-Oriented Programming (AOP) framework for .NET. It weaves interceptors into service methods through two equivalent proxy engines that share one contract (`AspectCore.Abstractions`):

- **DynamicProxy** (runtime, IL emit via `System.Reflection.Emit`) — lives in `AspectCore.Core`.
- **Source Generator** (compile-time, Roslyn `IIncrementalGenerator`) — lives in `AspectCore.SourceGenerator`.

**Tech stack (concrete):**
- **.NET target frameworks (libraries):** `net9.0;net8.0;net7.0;net6.0;netstandard2.1;netstandard2.0` (AspNetCore drops netstandard; SourceGenerator is `netstandard2.0` only).
- **.NET target frameworks (tests):** `net10.0;net9.0;net8.0;net6.0`.
- **C# language version:** `10.0` for `src/` (set in `build/common.props`); `13.0` for tests.
- **Test framework:** xUnit `2.9.2` + `Microsoft.NET.Test.Sdk 17.12.0`.
- **Coverage:** `coverlet.msbuild 6.0.2` (Cobertura), thresholds enforced in CI (unit 95%, E2E 80%).
- **DI integrations:** MsDI, Autofac `[7.0.0, 8.0.0)`, Castle.Windsor `6.0.0`, LightInject `6.6.4`, plus Generic Host and ASP.NET Core adapters.
- **Benchmarks:** BenchmarkDotNet `0.14.0`.
- **Version source of truth:** `build/version.props` (`VersionMajor=2`, `VersionMinor=7`, `VersionPatch=0`).

---

## 2. Project Structure Map

| Directory | Purpose | Local Documentation |
|-----------|---------|---------------------|
| `src/AspectCore.Abstractions/` | Pure contracts: interfaces, attributes, enums. No implementation. Namespaces `AspectCore.DynamicProxy`, `AspectCore.Configuration`, `AspectCore.DependencyInjection`. | – |
| `src/AspectCore.Core/` | Runtime DynamicProxy engine (IL emit), built-in IoC container (`ServiceContext`/`ServiceResolver`), interceptor pipeline, configuration. `AllowUnsafeBlocks=true`. | – |
| `src/AspectCore.Extensions.Reflection/` | Standalone high-performance reflection library. **No AspectCore project references**; consumed by Core. | – |
| `src/AspectCore.SourceGenerator/` | Roslyn compile-time proxy generator (`AspectCoreProxyGenerator`). `IsRoslynComponent=true`, `OutputItemType=Analyzer`. No project references. | – |
| `src/AspectCore.Extensions.DependencyInjection/` | Microsoft.Extensions.DependencyInjection (MsDI) adapter. | – |
| `src/AspectCore.Extensions.Autofac/` | Autofac adapter. | – |
| `src/AspectCore.Extensions.Windsor/` | Castle.Windsor adapter. | – |
| `src/AspectCore.Extensions.LightInject/` | LightInject adapter. | – |
| `src/AspectCore.Extensions.Hosting/` | Generic Host integration. | – |
| `src/AspectCore.Extensions.AspNetCore/` | ASP.NET Core web integration (`FrameworkReference Microsoft.AspNetCore.App`). | – |
| `src/AspectCore.Extensions.AspectScope/` | ScopedContext / aspect scope extension. | – |
| `src/AspectCore.Extensions.Configuration/` | Configuration injection via `Microsoft.Extensions.Configuration`. | – |
| `src/AspectCore.Extensions.DataAnnotations/` | DataAnnotations-based validation extension. | – |
| `src/AspectCore.Extensions.DataValidation/` | Data validation extension. | – |
| `tests/` | 9 xUnit test projects. `tests/Directory.Build.props` injects `coverlet.msbuild`. | – |
| `sample/` | 4 runnable sample projects (DI console, AspectScope, Autofac, DataAnnotations). | – |
| `benchmark/` `benchmarks/` | BenchmarkDotNet projects. | – |
| `docs/` | Architecture, guide, getting-started, development, testing docs (bilingual; `docs/en/` for English). | `docs/README.md` |
| `build/` | `common.props`, `version.props`, `sign.props`, `aspectcore.snk`. | – |
| `.github/workflows/` | `build-ci.yml`, `build-pr-ci.yml`, `release.yml`. | – |

**Dependency direction (acyclic, bottom-up):**
`Abstractions` + `Extensions.Reflection` ◄── `Core` ◄── all Extensions. `SourceGenerator` is independent (no project refs; generated code references Core/Abstractions at runtime).

---

## 3. Build & Development Commands

> No `global.json` exists — SDK is not pinned. CI installs `6.0.x / 8.0.x / 9.0.x / 10.0.x`. Locally you need an SDK that can build the target frameworks you care about.

```bash
# Whole solution
dotnet build AspectCore-Framework.sln --configuration Release

# Per-project (matches CI behavior)
for project in $(find ./src -name "*.csproj"); do
  dotnet build --configuration Release "$project"
done

# Build with explicit version (CI release flow)
dotnet build --configuration Release ./src/AspectCore.Core/AspectCore.Core.csproj -p:Version=2.7.0

# Format check (PR CI gate — currently warns, does not fail)
dotnet format AspectCore-Framework.sln --verify-no-changes

# Auto-format locally before pushing
dotnet format AspectCore-Framework.sln
```

**Pack / publish NuGet:**
```bash
# Pack all src projects (CI build flow, includes source/symbols)
for project in $(find ./src -name "*.csproj"); do
  dotnet pack --configuration Release --no-build "$project" \
    -p:PackageVersion=2.7.0 --include-source --output ./artifacts/packages
done

# Pack single project
dotnet pack --configuration Release --no-build \
  ./src/AspectCore.Core/AspectCore.Core.csproj \
  -p:PackageVersion=2.7.0 --output ./artifacts/packages
```

**Run a sample:**
```bash
dotnet run --configuration Release -f net9.0 \
  --project ./sample/AspectCore.Extensions.DependencyInjection.ConsoleSample/AspectCore.Extensions.DependencyInjection.ConsoleSample.csproj
```

**Run a benchmark:**
```bash
dotnet run --configuration Release -f net9.0 \
  --project ./benchmark/AspectCore.Core.Benchmark/AspectCore.Core.Benchmark.csproj
```

---

## 4. Testing Instructions

```bash
# All test projects (CI behavior)
for project in $(find ./tests -name "*.csproj"); do
  dotnet test --configuration Release "$project"
done

# Single test project
dotnet test --configuration Release ./tests/AspectCore.Core.Tests/AspectCore.Core.Tests.csproj

# Single target framework (tests target net10.0;net9.0;net8.0;net6.0)
dotnet test --configuration Release -f net9.0 ./tests/AspectCore.Core.Tests/AspectCore.Core.Tests.csproj

# Filter a single test (xUnit filter syntax)
dotnet test --configuration Release ./tests/AspectCore.Core.Tests/AspectCore.Core.Tests.csproj \
  --filter "FullyQualifiedName~AspectCore.Core.Tests.ProxyTests"

# With coverage (matches .github/scripts/check-coverage.sh)
dotnet test ./tests/AspectCore.Core.Tests/AspectCore.Core.Tests.csproj \
  --configuration Release -f net9.0 \
  /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura \
  /p:CoverletOutput=./TestResults/ \
  "/p:Include=[AspectCore.Core]*"
```

**Coverage thresholds (blocking CI gates):** unit tests **95%**, E2E tests **80%**. E2E coverage filters to `[AspectCore.Core]*` and `[AspectCore.Abstractions]*`. The coverage script sets `DOTNET_ROLL_FORWARD=Major` to run on newer runtimes.

**Engine parity:** `tests/AspectCore.Core.Tests/EngineParity/` enforces that DynamicProxy and the Source Generator behave identically. Any change to the core interceptor/proxy engine MUST keep both engines in sync and pass these tests.

---

## 5. Git Workflow

- **Default branch:** `master` (PR merge target). There is no `main` branch.
- **Branch naming:** `feat/<short-description>` from `origin/master`. Also used: `fix/`, `ci/`, `chore/`, `docs/`, `test/`, `feature/`.
- **Commit messages (CRITICAL):** Conventional Commits — `feat:`, `fix:`, `docs:`, `test:`, `ci:`, `chore:`. Example: `fix: implement keyed service resolution in IServiceResolver (#387)`.
- **PR merge:** squash-merge; the merge commit title includes the PR number `(#number)`.
- **Committer identity (CRITICAL):** must be `Haoyang Liu` / `liuhaoyang1221@hotmail.com`.
- **Avoid `Co-Authored-By` trailers:** recently adopted policy (most recent commits follow this; older commits may still contain them). Prefer commits without these trailers.
- **Release flow:** tag `v*` → `release.yml` builds, tests, packs, publishes to NuGet.org + MyGet, creates a GitHub Release, then auto-bumps `build/version.props` to the next minor via an auto-PR. Patch bumps are manual.

---

## 6. Code Style Guidelines

Formatting is enforced by `dotnet format` (no `.editorconfig`; uses .NET SDK defaults). `src/Directory.Build.props` enables .NET analyzers at informational level (non-blocking). `LangVersion=10.0` for `src/`.

**Namespaces — block-scoped, not file-scoped:**
```csharp
// ✅
namespace AspectCore.DynamicProxy
{
    public interface IInterceptor { ... }
}

// ❌
namespace AspectCore.DynamicProxy;
public interface IInterceptor { ... }
```

**Private fields prefixed with `_`:**
```csharp
// ✅
private readonly IInterceptorSelector[] _interceptorSelectors;
```

**Public API contract/implementation split (CRITICAL):**
- Interfaces and attributes go in `AspectCore.Abstractions` (no implementation dependencies).
- Implementations go in `AspectCore.Core` or the relevant extension package.
- Dependency direction is `Abstractions ◄── Core ◄── Extensions` — never reverse it, never add horizontal coupling between extension packages.

**Naming conventions:**
- Interfaces: `I`-prefixed — `IInterceptor`, `IServiceResolver`, `IAspectBuilder`, `IProxyGenerator`.
- Attribute-based interceptors: derive from `AbstractInterceptorAttribute`; name ends with `InterceptorAttribute` (e.g. `ServiceInterceptorAttribute`, `DataValidationInterceptorAttribute`).
- Non-attribute interceptors: derive from `AbstractInterceptor`; name ends with `Interceptor`.
- Other attributes: end with `Attribute` (e.g. `NonAspectAttribute`, `FromServiceContextAttribute`, `AspectCoreGenerateProxyAttribute`).
- Namespaces match the package/feature.

**XML doc comments (`/// <summary>`) are required on public APIs.**

**Constructor null guards use `ArgumentNullException(nameof(param))`.**

**Async interceptor pattern:**
```csharp
public async Task Invoke(AspectContext context, AspectDelegate next)
{
    // before
    await next(context);
    // after
}
```

---

## 7. Boundaries & Guardrails

- ✅ **Always do (MANDATORY, before any work)** — read the [development guidelines](docs/development/development-guidelines.md) in full **before** starting any code change, and walk through the [code review guidelines](docs/development/code-review-guidelines.md) self-check checklist **before** opening a PR. The `aspectcore-dev-review` skill (`.agents/skills/aspectcore-dev-review/SKILL.md`) is a quick reference only and does **not** replace reading the full guidelines.
- ✅ **Always do** — keep DynamicProxy and the Source Generator behaviorally in sync; run the `EngineParity/` tests after any core engine change.
- ✅ **Always do** — run `dotnet format` locally before pushing to avoid the CI lint gate.
- ✅ **Always do** — put new public interfaces/attributes in `AspectCore.Abstractions`; implementations in `Core` or the relevant extension.
- ✅ **Always do** — use Conventional Commits and the fixed committer identity (`Haoyang Liu`).
- ✅ **Always do** — follow the [development guidelines](docs/development/development-guidelines.md) and [code review guidelines](docs/development/code-review-guidelines.md) for every change; consult the `aspectcore-dev-review` skill (`.agents/skills/aspectcore-dev-review/SKILL.md`) as a quick reference.
- ⚠️ **Ask first** — bumping `build/version.props` (release flow auto-bumps minor only; patch bumps need explicit approval).
- ⚠️ **Ask first** — changing target frameworks or `LangVersion` in `build/common.props` (affects all packages and CI matrix).
- ⚠️ **Ask first** — adding a new DI container integration or a new top-level package.
- ⚠️ **Ask first** — adding `Co-Authored-By` trailers to commit messages (recently adopted policy; check with maintainer before including).
- 🚫 **Never do** — commit generated proxy source from `AspectCore.SourceGenerator` (it is emitted at compile time into `obj/`, which is gitignored).
- 🚫 **Never do** — reverse the `Abstractions ◄── Core ◄── Extensions` dependency direction, or add horizontal references between extension packages.
- 🚫 **Never do** — commit secrets, NuGet API keys, or `artifacts/` output.
- 🚫 **Never do** — skip the 95% unit / 80% E2E coverage thresholds; they are blocking CI gates.

---

## 8. Related Documentation

- `README.md` — project overview, NuGet install table, quick start.
- `ROADMAP.md` — current roadmap and planned work.
- `docs/README.md` — documentation index.
- `docs/architecture/overview.md` — architecture and module design (Chinese).
- `docs/architecture/module-design.md` — contract/implementation split and dependency rules.
- `docs/development/contributing.md` — contribution rules, commit conventions, CI gates (Chinese).
- `docs/development/development-guidelines.md` — development standards: project structure recognition, command granularity, testing, performance, design principles.
- `docs/development/code-review-guidelines.md` — code review standards: review dimensions, blocking issues, self-check checklist.
- `.agents/skills/aspectcore-dev-review/SKILL.md` — skill for AI agents: quick reference for development and review guidelines.
- `docs/guide/interceptor.md` — how to write interceptors.
- `docs/testing/` — testing guidance.
- `docs/en/` — English documentation mirror.
- `.github/workflows/build-ci.yml`, `build-pr-ci.yml`, `release.yml` — CI/CD definitions.

No top-level `deepwiki/` directory exists in this repository.
