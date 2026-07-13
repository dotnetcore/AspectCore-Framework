# AspectCore Framework Benchmarks

Compares **Source Generator (SG) proxy** vs **DynamicProxy (runtime emit)** performance.

## Running

```bash
# Run all benchmarks
dotnet run -c Release --project benchmarks/AspectCore.Benchmarks/AspectCore.Benchmarks.csproj

# Run specific category
dotnet run -c Release --project benchmarks/AspectCore.Benchmarks/AspectCore.Benchmarks.csproj -- --filter *Sync*
dotnet run -c Release --project benchmarks/AspectCore.Benchmarks/AspectCore.Benchmarks.csproj -- --filter *Async*
dotnet run -c Release --project benchmarks/AspectCore.Benchmarks/AspectCore.Benchmarks.csproj -- --filter *Property*
dotnet run -c Release --project benchmarks/AspectCore.Benchmarks/AspectCore.Benchmarks.csproj -- --filter *Generic*
dotnet run -c Release --project benchmarks/AspectCore.Benchmarks/AspectCore.Benchmarks.csproj -- --filter *Interface*
dotnet run -c Release --project benchmarks/AspectCore.Benchmarks/AspectCore.Benchmarks.csproj -- --filter *Creation*

# List available benchmarks
dotnet run -c Release --project benchmarks/AspectCore.Benchmarks/AspectCore.Benchmarks.csproj -- --list flat
```

## Benchmark Categories

| Category | Description |
|----------|-------------|
| **Sync** | Synchronous method calls: 2-arg, 3-arg, 8-arg methods |
| **Async** | `Task<T>` and `ValueTask<T>` async methods |
| **Property** | Property getter and setter overhead |
| **Generic** | Generic method calls (`Echo<T>`) |
| **Interface** | Interface proxy with target implementation |
| **Creation** | Proxy type/instance creation cost |

## What's Measured

- **Direct call**: No proxy at all (baseline)
- **DynamicProxy**: Runtime IL-emit generated proxy
- **SourceGenerator**: Compile-time source-generated proxy

Each benchmark includes a `BenchmarkInterceptor` (pass-through interceptor) to measure
the full interceptor pipeline overhead.
