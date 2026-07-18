# Contributing guide

This document explains the process and conventions for submitting code to AspectCore. All rules are based on this repository's actual configuration (branch protection, CI workflows, commit history, and Git identity).

## 1. Branches and workflow

- The default branch is `master`, which is also the merge target for PRs.
- Create a feature branch from the latest `origin/master` to develop:

```bash
git fetch origin
git switch -c feat/<short-description> origin/master
```

- Unless you explicitly need a stacked PR, do not develop on top of another feature branch.

## 2. Commit conventions (Conventional Commits)

The commit history follows [Conventional Commits](https://www.conventionalcommits.org/), with common prefixes: `feat:`, `fix:`, `docs:`, `test:`, `ci:`. For reference, some recent commits:

```text
feat: support ref and ref readonly return proxy generation (#385)
feat: support C# 9 record type proxy generation (#384)
ci: split coverage checks by test type (#380)
docs(architecture): complete architecture section
```

- PRs are merged via squash, and the merge commit title includes the PR number `(#number)`.
- Use a single-line, concise description of the change's intent in the commit message, adding context in the body when necessary.

## 3. Git identity

This repository uses a fixed commit identity; confirm your local `git config` before committing:

```bash
git config user.name    # Haoyang Liu
git config user.email   # liuhaoyang1221@hotmail.com
```

- The commit identity must match the repository configuration; do not override it with a command-line author argument or environment variables.
- **Do not** add any `Co-Authored-By` trailer.

## 4. Local verification before committing

Before opening a PR, at least get compilation and tests passing on the project the change belongs to; changes that span the core engine, public contracts, or multiple packages should broaden the verification scope:

```bash
# Solution scope
dotnet build AspectCore-Framework.sln -c Release
dotnet test AspectCore-Framework.sln

# Or narrow it to the scope of the change
dotnet test tests/AspectCore.Core.Tests/AspectCore.Core.Tests.csproj
```

For command details and filtering options, see [Local build](./building.md) and [Running tests](../testing/running-tests.md).

> For changes involving the two proxy engines, be sure to get the parity tests under `EngineParity/` passing, ensuring that DynamicProxy and Source Generator behave consistently (for background, see [Comparing and choosing between the two engines](../architecture/engine-comparison.md)).

## 5. PR process and required checks

PRs target `master`, and merging requires satisfying the branch protection rules:

- squash-only merge;
- at least 1 approving review;
- all review discussion threads resolved;
- all of the following required status checks passing (produced by `.github/workflows/build-pr-ci.yml`):
  - `lint` (`dotnet format --verify-no-changes`)
  - `build-and-test (ubuntu-latest)`
  - `build-and-test (windows-latest)`
  - `Unit Test Execution`
  - `Unit Test Coverage Result` (unit coverage threshold 95%)
  - `E2E Test Execution`
  - `E2E Test Coverage Result` (E2E coverage threshold 80%)

For the meaning of the coverage thresholds and checks, see [Testing strategy](../testing/testing-strategy.md). Lint uses `dotnet format`; you can self-check locally first:

```bash
dotnet format AspectCore-Framework.sln --verify-no-changes
# If there are formatting issues, run dotnet format to fix them automatically
dotnet format AspectCore-Framework.sln
```

## Related docs

- [Local build](./building.md) — restore, compile, target frameworks
- [Project structure](./project-structure.md) — the source, test, sample, and benchmark layout
- [Testing strategy](../testing/testing-strategy.md) — test categories and coverage thresholds
- [Running tests](../testing/running-tests.md) — test filtering and coverage collection
- [Docs home](../README.md)
