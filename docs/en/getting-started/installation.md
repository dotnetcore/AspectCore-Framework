# Installation

AspectCore is split into multiple NuGet packages by responsibility: the core libraries handle AOP and the container, the integration packages handle wiring into various hosts and third-party containers, and the compile-time engine is published separately. This page explains what each package is for, the target frameworks it supports, and how to pick packages by scenario.

## Core libraries

| Package | Purpose | When you need it |
|----|------|----------|
| `AspectCore.Abstractions` | The contract layer: abstractions and attributes such as `AspectContext`, `IInterceptor`, `AbstractInterceptorAttribute`, `IAspectConfiguration`, and `IServiceContext`. | Usually pulled in indirectly by other packages; install it directly only when you just want to reference the contracts for an extension. |
| `AspectCore.Core` | The DynamicProxy runtime engine plus the built-in IoC container (`ServiceContext` / `IServiceResolver`). | When you need to use dynamic proxying or the built-in container on their own. |
| `AspectCore.Extensions.Reflection` | Standalone high-performance reflection extensions (`GetReflector()`), independent of AOP. | Can be used on its own when you only need fast reflection calls. |

## Integration packages

| Package | Purpose |
|----|------|
| `AspectCore.Extensions.DependencyInjection` | Integrates with `Microsoft.Extensions.DependencyInjection`: `ConfigureDynamicProxy(...)` configures interceptors, and `DynamicProxyServiceProviderFactory` takes over the container. |
| `AspectCore.Extensions.Autofac` | Integrates with Autofac: `ContainerBuilder.RegisterDynamicProxy(...)`. |
| `AspectCore.Extensions.Windsor` | Integrates with Castle Windsor: `container.AddAspectCoreFacility(...)`. |
| `AspectCore.Extensions.LightInject` | Integrates with LightInject: `container.RegisterDynamicProxy(...)`. |
| `AspectCore.Extensions.Hosting` | Integrates with the generic host: `IHostBuilder.UseServiceContext()` / `UseDynamicProxy()`. |
| `AspectCore.Extensions.AspNetCore` | Extension support for ASP.NET Core scenarios. |
| `AspectCore.Extensions.Configuration` | Injects configuration values from `IConfiguration`: `AddConfigurationInject()`. |
| `AspectCore.Extensions.DataValidation` | Data-validation infrastructure (abstractions and interceptors). |
| `AspectCore.Extensions.DataAnnotations` | A validation implementation based on `System.ComponentModel.DataAnnotations`: `AddDataAnnotations(...)`. |

## Compile-time engine

| Package | Purpose |
|----|------|
| `AspectCore.SourceGenerator` | The compile-time Source Generator AOP engine, referenced as an analyzer; it generates proxy types at compile time. This is **explicit opt-in** and does not change runtime behavior by default. |

The Source Generator is a separate engine from the runtime DynamicProxy. For the differences between the two and how to choose, see [Comparing and choosing between the two engines](../architecture/engine-comparison.md).

## Target frameworks

| Package category | Target frameworks |
|--------|----------|
| Core libraries and most integration packages (`AspectCore.Abstractions`, `AspectCore.Core`, `AspectCore.Extensions.Reflection`, `AspectCore.Extensions.DependencyInjection`, Autofac/Windsor/LightInject/Hosting, Configuration, DataAnnotations, etc.) | `net9.0`, `net8.0`, `net7.0`, `net6.0`, `netstandard2.1`, `netstandard2.0` |
| `AspectCore.Extensions.AspNetCore` | `net9.0`, `net8.0`, `net7.0`, `net6.0` (no netstandard) |
| `AspectCore.SourceGenerator` | `netstandard2.0` (analyzer convention) |

> The framework lists above come from the `TargetFrameworks` in each package's `.csproj`. The core libraries cover older runtimes through `netstandard2.0`/`netstandard2.1`; `AspectCore.Extensions.AspNetCore` targets only `net6.0` and above.

## Install commands

Using the .NET CLI as an example (in the Package Manager console, just replace `dotnet add package` with `Install-Package`):

```bash
# Integrate with Microsoft.Extensions.DependencyInjection (the most common entry point)
dotnet add package AspectCore.Extensions.DependencyInjection

# Use only dynamic proxying or the built-in container
dotnet add package AspectCore.Core

# Use only the high-performance reflection extensions
dotnet add package AspectCore.Extensions.Reflection
```

`AspectCore.Extensions.DependencyInjection` automatically pulls in `AspectCore.Core` and `AspectCore.Abstractions` through its dependencies, so you generally do not need to install them separately.

## How to pick packages

- **ASP.NET Core / generic host applications using the built-in container**: install `AspectCore.Extensions.DependencyInjection`. This is the starting point for the vast majority of scenarios — go straight to [Quick start](./quick-start.md).
- **Already using Autofac / Windsor / LightInject**: install the corresponding integration package; see [Third-party containers](../guide/third-party-containers.md).
- **No DI needed, just want to create proxy objects manually**: install `AspectCore.Core` and use `ProxyGeneratorBuilder` to generate proxies on your own.
- **Just want reflection calls faster than native reflection**: install `AspectCore.Extensions.Reflection` on its own; see [Reflection extensions](../guide/reflection-extensions.md).
- **Need compile-time proxies that reduce the dependency on dynamic code during proxy generation**: additionally reference `AspectCore.SourceGenerator`; see [Source Generator compile-time engine](../architecture/source-generator.md).

## Next steps

- [Quick start](./quick-start.md) — get your first interceptor running in five minutes.
- [Core concepts](./concepts.md) — understand terms such as interceptor, proxy, and aspect context.
