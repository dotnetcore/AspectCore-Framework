# AspectCore Framework Benchmarks

Comprehensive benchmark suite comparing **Source Generator (SG) proxy** vs **DynamicProxy (runtime emit)** performance across multiple dimensions.

## Quick Start

```bash
# Run all benchmarks (full accuracy)
./benchmarks/run-benchmarks.sh

# Run with quick mode (CI-friendly, less accurate)
./benchmarks/run-benchmarks.sh --quick

# Run a specific category
./benchmarks/run-benchmarks.sh "*Sync*"

# List all available benchmarks
dotnet run --project benchmarks/AspectCore.Benchmarks/ -c Release -- --list flat
```

## Benchmark Categories

| Category | Class | Description |
|----------|-------|-------------|
| **Sync** | `SyncMethodBenchmarks` | Synchronous method calls: 2-arg, 3-arg, 8-arg |
| **Async** | `AsyncMethodBenchmarks` | `Task<T>` and `ValueTask<T>` async methods |
| **Property** | `PropertyBenchmarks` | Property getter and setter overhead |
| **Generic** | `GenericMethodBenchmarks` | Generic method calls (`Echo<T>`) |
| **Interface** | `InterfaceProxyBenchmarks` | Interface proxy with target implementation |
| **Creation** | `ProxyCreationBenchmarks` | Proxy type/instance creation cost |
| **MSDI** | `MsdiIntegrationBenchmarks` | Microsoft.Extensions.DI resolution + invocation |
| **MSDI/Scoped** | `MsdiScopedBenchmarks` | Scoped service resolution patterns |
| **Pipeline** | `PipelineBenchmarks` | Multi-interceptor chain (1, 2, 5, 10 interceptors) |
| **ColdStart** | `ColdStartBenchmarks` | First generator + proxy + call latency |
| **FirstCall** | `FirstCallBenchmarks` | First proxy creation + call (warm generator) |

## What's Measured

Each benchmark compares three approaches:

- **Direct call**: No proxy at all (baseline)
- **DynamicProxy (DP)**: Runtime IL-emit generated proxy
- **SourceGenerator (SG)**: Compile-time source-generated proxy

All benchmarks include `[MemoryDiagnoser]` for per-invocation allocation tracking (Gen0/Gen1 collections and bytes allocated).

## Running Benchmarks

### Full Accuracy Run

```bash
# All benchmarks
dotnet run --project benchmarks/AspectCore.Benchmarks/ -c Release

# Specific category
dotnet run --project benchmarks/AspectCore.Benchmarks/ -c Release -- --filter "*Pipeline*"

# Multiple categories
dotnet run --project benchmarks/AspectCore.Benchmarks/ -c Release -- --filter "*Sync*|*Async*"
```

### Quick Mode (CI / Iteration)

```bash
# Quick run (ShortRunJob - fewer iterations, less accurate)
dotnet run --project benchmarks/AspectCore.Benchmarks/ -c Release -- --quick

# Quick with filter
dotnet run --project benchmarks/AspectCore.Benchmarks/ -c Release -- --quick --filter "*MSDI*"
```

### Shell Script Runner

```bash
# Full run
./benchmarks/run-benchmarks.sh

# Quick + filter
./benchmarks/run-benchmarks.sh "*ColdStart*" --quick

# Show help
./benchmarks/run-benchmarks.sh --help
```

## Baseline Management

Track performance over time by exporting and comparing baselines.

### Export a Baseline

```bash
dotnet run --project benchmarks/AspectCore.Benchmarks/ -c Release -- --export-baseline
```

Saves results to `benchmarks/results/baseline.json`.

### Compare Against Baseline

```bash
dotnet run --project benchmarks/AspectCore.Benchmarks/ -c Release -- --compare-baseline
```

Reports regressions where current results exceed 2x the baseline mean.

## CI Integration

### Running in CI

```bash
./benchmarks/ci-benchmark.sh
```

The CI script:
1. Builds in Release mode
2. Runs all benchmarks with ShortRunJob (fast)
3. If `baseline.json` exists, compares against it and fails on regression (>2x threshold)
4. Outputs results to `benchmarks/results/ci-<timestamp>/`
5. Exit code: 0 = pass, 1 = regression or failure

### Setting Up CI Regression Detection

```bash
# 1. Create baseline (one-time or per-release)
dotnet run --project benchmarks/AspectCore.Benchmarks/ -c Release -- --export-baseline

# 2. Commit baseline.json to version control
git add benchmarks/results/baseline.json

# 3. In CI pipeline, run:
./benchmarks/ci-benchmark.sh
```

## Interpreting Results

### Key Columns

| Column | Meaning |
|--------|---------|
| **Mean** | Average execution time per operation |
| **StdDev** | Standard deviation (lower = more stable) |
| **Ratio** | Relative to baseline (1.00 = same, <1.00 = faster) |
| **Rank** | Performance ranking within the group |
| **Gen0** | Gen0 garbage collections per 1000 operations |
| **Allocated** | Bytes allocated per operation |

### What to Look For

- **SG vs DP ratio < 1.0**: SourceGenerator is faster (expected for steady-state)
- **SG allocation = 0 B**: SourceGenerator path has zero per-call allocations (ideal)
- **Pipeline scaling**: Linear growth in interceptor overhead
- **ColdStart**: SG should have lower cold-start since no IL emit is needed

### Expected Results

| Scenario | Expected Winner | Why |
|----------|----------------|-----|
| Steady-state calls | SG (slightly) | No reflection, direct dispatch |
| Cold start | SG (significantly) | No runtime type generation |
| Memory allocation | SG | No boxing, no reflection allocations |
| Pipeline (many interceptors) | Similar | Pipeline dispatch is the same infrastructure |
| MSDI resolution | Similar | Resolution path is mostly the same |

## Adding New Benchmarks

1. Create a new file in `benchmarks/AspectCore.Benchmarks/` (e.g., `MyNewBenchmarks.cs`)
2. Inherit from `ProxyBenchmarkBase` if you need DP/SG proxy generators, or set up your own
3. Add `[MemoryDiagnoser]` and `[BenchmarkCategory("MyCategory")]`
4. Add `[Benchmark(Baseline = true)]` to one method as the reference point
5. The benchmark will automatically appear in `--list flat` and be runnable via filter

### Template

```csharp
using BenchmarkDotNet.Attributes;

namespace AspectCore.Benchmarks;

[MemoryDiagnoser]
[BenchmarkCategory("MyCategory")]
public class MyNewBenchmarks : ProxyBenchmarkBase
{
    // Setup proxies in override Setup()

    [Benchmark(Baseline = true, Description = "Direct call")]
    public int Direct() => /* ... */;

    [Benchmark(Description = "DynamicProxy")]
    public int DynamicProxy() => /* ... */;

    [Benchmark(Description = "SourceGenerator")]
    public int SourceGen() => /* ... */;
}
```

## Project Structure

```
benchmarks/
  AspectCore.Benchmarks/
    Program.cs                      # Entry point + baseline export/compare
    BenchmarkConfig.cs              # Default + Quick configurations
    Services.cs                     # Test services and interceptors
    Benchmarks.cs                   # Core benchmarks (Sync, Async, Property, Generic, Interface, Creation)
    MsdiIntegrationBenchmarks.cs    # MSDI DI integration benchmarks
    PipelineBenchmarks.cs           # Multi-interceptor chain benchmarks
    ColdStartBenchmarks.cs          # Cold start / first call benchmarks
  run-benchmarks.sh                 # Full benchmark runner script
  ci-benchmark.sh                   # CI regression detection script
  results/                          # Output directory (gitignored)
    baseline.json                   # Saved baseline for comparison
    <timestamp>/                    # Full run results
    ci-<timestamp>/                 # CI run results
  README.md                         # This file
```

## Requirements

- .NET 10.0 SDK (or matching the project TFM)
- Release configuration (Debug mode disables optimizations and skews results)
- Sufficient system resources (avoid running other heavy workloads during benchmarks)
