# Feature Comparison: Castle DynamicProxy vs AspectCore Framework

This document provides a factual comparison of interception and proxy capabilities between
Castle DynamicProxy (v5.2.1) and AspectCore Framework.

## Core Interception

| Feature | Castle DynamicProxy | AspectCore |
|---------|:---:|:---:|
| Sync method interception | Yes | Yes |
| Async `Task<T>` interception | Partial (requires `IAsyncInterceptor` wrapper) | Yes (native) |
| Async `ValueTask<T>` interception | No | Yes |
| `IAsyncEnumerable<T>` interception | No | Yes |
| Unified interceptor model (sync + async) | No (separate `IInterceptor` / `IAsyncInterceptor`) | Yes |

## Modern C# Language Features

| Feature | Castle DynamicProxy | AspectCore |
|---------|:---:|:---:|
| `ref` / `ref readonly` return values | No | Yes |
| Record type proxy generation | No | Yes |
| Primary constructors (C# 12) | No | Yes |
| Partial properties (C# 13) | No | Yes |
| Default interface methods | Partial | Yes |

## Compilation and AOT

| Feature | Castle DynamicProxy | AspectCore |
|---------|:---:|:---:|
| NativeAOT support | No | Yes (Source Generator) |
| Compile-time proxy generation | No (planned v6, no release date) | Yes |
| Runtime IL emission (Reflection.Emit) | Yes (only option) | Yes (DynamicProxy engine) |
| Source Generator engine | No | Yes |
| Trimming-safe | No | Yes (Source Generator) |

## Dependency Injection Integration

| Feature | Castle DynamicProxy | AspectCore |
|---------|:---:|:---:|
| Microsoft.Extensions.DependencyInjection native | No (needs Autofac or Windsor bridge) | Yes |
| Keyed service interception (.NET 8+) | No | Yes |
| `IServiceProviderFactory<T>` integration | No (container-specific) | Yes |
| ASP.NET Core `IHost` integration | No (manual setup) | Yes (`AddDynamicProxy()`) |

## Interceptor Configuration

| Feature | Castle DynamicProxy | AspectCore |
|---------|:---:|:---:|
| Attribute-based interceptor selection | Limited (`InterceptorAttribute`) | Yes (`AbstractInterceptorAttribute`) |
| Predicate-based global configuration | No (manual `IInterceptorSelector`) | Yes (`AspectPredicate`) |
| Non-aspect exclusion (opt-out) | Limited | Yes (`[NonAspect]`) |
| Interceptor ordering | By registration order | Yes (explicit `Order` property) |
| Inherited interception control | Limited | Yes (`Inherited` property) |

## Proxy Generation

| Feature | Castle DynamicProxy | AspectCore |
|---------|:---:|:---:|
| Interface proxy (with target) | Yes | Yes |
| Class proxy (virtual methods) | Yes | Yes |
| Mixin support | Yes | No |
| `IChangeProxyTarget` | Yes | No |
| Multiple interface proxy | Yes | Yes |

## Performance Characteristics

| Aspect | Castle DynamicProxy | AspectCore |
|--------|:---:|:---:|
| First-call latency | Higher (runtime IL gen) | Lower (Source Generator pre-compiled) |
| Steady-state overhead | Moderate | Low |
| Per-invocation allocation | Higher | Lower |
| Startup time impact | Higher (assembly scanning + emit) | Minimal (Source Generator) |

> **Note:** See `benchmarks/AspectCore.Benchmarks.Competitive/` for reproducible numbers.

## Ecosystem

| Feature | Castle DynamicProxy | AspectCore |
|---------|:---:|:---:|
| Castle Windsor integration | Native | Via `AspectCore.Extensions.Windsor` |
| Autofac integration | Via `Autofac.Extras.DynamicProxy` | Via `AspectCore.Extensions.Autofac` |
| LightInject integration | No | Yes |
| NuGet downloads (monthly) | ~2M | ~200K |
| Active maintenance | Limited (v5.2.1, Feb 2024) | Active |
| .NET 9/10 support | Partial | Full |
| License | Apache 2.0 | MIT |

## Migration Compatibility

| Scenario | Support |
|----------|:---:|
| Run Castle interceptors in AspectCore pipeline | Yes (via `CastleInterceptorAdapter`) |
| Run AspectCore interceptors in Castle pipeline | Yes (via `AspectCoreInterceptorAdapter`) |
| Gradual migration (both frameworks coexist) | Yes |
| Zero-change migration | No (API differences require code changes) |

## Summary

AspectCore provides **broader language feature coverage**, **native async support**,
**NativeAOT compatibility**, and **tighter MSDI integration** compared to Castle DynamicProxy.

Castle DynamicProxy offers **mixin support** and **larger ecosystem adoption** but lacks
modern C# feature support and has no NativeAOT path.

For new projects or projects targeting .NET 8+, AspectCore is the recommended choice.
For existing Castle/Windsor codebases, the `AspectCore.Extensions.CastleCompat` package
provides a gradual migration path.
