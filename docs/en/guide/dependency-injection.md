# Dependency Injection Integration

Alongside providing AOP, AspectCore ships a lightweight, high-performance IoC container that integrates seamlessly with dynamic proxying. You can use this built-in container `IServiceContext` directly, or let it take over `Microsoft.Extensions.DependencyInjection` and replace the default container in ASP.NET Core. This page covers both usages: registration, resolution, lifetime, constructor and property injection, AOP integration, and a performance comparison against Autofac.

## The Built-in Container IServiceContext

The interface of the built-in container is `IServiceContext` (namespace `AspectCore.DependencyInjection`), with the default implementation `ServiceContext`. It comes with `AspectCore.Core`.

### Service Registration

`IServiceContext` provides three registration approaches—by type, by instance, and by delegate—through extension methods:

```csharp
using AspectCore.DependencyInjection;

IServiceContext services = new ServiceContext();

// register by type
services.AddType<ILogger, Logger>();

// register by instance (the lifetime is singleton)
services.AddInstance<ITaskService>(new TaskService());

// register by delegate factory (the delegate parameter is IServiceResolver)
services.AddDelegate<ITaskService, TaskService>(resolver => new TaskService());
```

Both `AddType` and `AddDelegate` take an optional `Lifetime` parameter (default `Transient`); services registered with `AddInstance` are always singletons.

### Service Resolution

First use `Build()` to build the `IServiceContext` into an `IServiceResolver`, then resolve services:

```csharp
// build the resolver
IServiceResolver resolver = services.Build();

// resolve a single service
ISampleService sampleService = resolver.Resolve<ISampleService>();

// resolve a single service, throwing if not registered
ISampleService required = resolver.ResolveRequired<ISampleService>();

// resolve a collection of services, returning an empty collection if not registered
IEnumerable<ISampleService> many = resolver.ResolveMany<ISampleService>();
```

### Lifetime

The `Lifetime` enum provides three lifetimes:

- **Transient**: a new instance is created on every request, suitable for lightweight, stateless services.
- **Scoped**: created once per scope.
- **Singleton**: created on first resolution, after which all resolutions reuse the same instance.

```csharp
services.AddType<ILogger, Logger>(Lifetime.Singleton);
services.AddType<ITaskService, TaskService>(Lifetime.Scoped);
services.AddType<ISampleService, SampleService>(Lifetime.Transient);
```

### Constructor Injection and Property Injection

The built-in container supports both constructor injection and property injection:

```csharp
using AspectCore.DependencyInjection;

public class SampleService : ISampleService
{
    private readonly ISampleRepository _repository;
    private readonly ILogger _logger;

    // constructor injection
    public SampleService(ISampleRepository repository, ILogger logger)
    {
        _repository = repository;
        _logger = logger;
    }
}

public class SampleRepository : ISampleRepository
{
    // property injection: a settable property annotated with [FromServiceContext] is injected automatically
    [FromServiceContext]
    public ILogger Logger { get; set; }
}
```

The conditions for property injection are: the property is annotated with `[FromServiceContext]` (namespace `AspectCore.DependencyInjection`) and is writable.

### AOP Integration

The built-in container enables AOP integration with DynamicProxy by default, configuring interceptors through the `Configure` method—using the same `IAspectConfiguration` as in other hosts:

```csharp
services.Configure(config =>
{
    config.Interceptors.AddTyped<SampleInterceptorAttribute>();
});
```

Therefore, an instance resolved from the container is already a proxy with aspects woven in, if it matches the interceptor configuration. For the full explanation of interceptor configuration, see [Interceptor Configuration](./interceptor-configuration.md).

## Integration with Microsoft.Extensions.DependencyInjection

Most applications do not use `ServiceContext` directly, but instead let AspectCore take over `IServiceCollection`. Install `AspectCore.Extensions.DependencyInjection`:

```bash
dotnet add package AspectCore.Extensions.DependencyInjection
```

### Configuring Interceptors: ConfigureDynamicProxy

Call `ConfigureDynamicProxy(...)` once on `IServiceCollection` to enable dynamic proxying and register global interceptors:

```csharp
using AspectCore.Extensions.DependencyInjection;

services.AddTransient<ICustomService, CustomService>();
services.ConfigureDynamicProxy(config =>
{
    config.Interceptors.AddTyped<CustomInterceptorAttribute>();
});
```

### Two Ways to Build the Container

**Approach 1: `DynamicProxyServiceProviderFactory` (recommended for ASP.NET Core / Generic Host)**

Take over container construction at the host layer with `IServiceProviderFactory`, transparent to business code:

```csharp
using AspectCore.Extensions.DependencyInjection;

Host.CreateDefaultBuilder(args)
    .UseServiceProviderFactory(new DynamicProxyServiceProviderFactory())
    .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
```

**Approach 2: `BuildDynamicProxyProvider` / `BuildServiceContextProvider` (manual construction)**

Build the `IServiceProvider` directly in console or test scenarios:

```csharp
// directly obtain a ServiceProvider with dynamic proxying enabled
var provider = services.BuildDynamicProxyProvider();

// or bridge IServiceCollection to the built-in ServiceContext and then build
var provider2 = services.BuildServiceContextProvider();
```

### Using It in ASP.NET Core

In `Program`/`Startup`, hook up `DynamicProxyServiceProviderFactory`, and in `ConfigureServices` register services as usual and call `ConfigureDynamicProxy()` once, to replace the default DI with AspectCore's container. For the full flow, see [Quick Start](../getting-started/quick-start.md).

## Performance

The built-in container is designed for low overhead. Officially, using Autofac (4.6.2) as a comparison, benchmarks were run across three aspects: resolving a simple object, property injection, and constructor injection. The data below comes from a historical benchmark (.NET Core 2.0, BenchmarkDotNet v0.10.8), and is only for a sense of magnitude; actual numbers vary with the runtime environment:

| Method | Mean | Allocated |
|--------|------|-----------|
| Autofac_Sample_Resolve | 494.83 ns | 752 B |
| AspectCore_Sample_Resolve | 88.52 ns | 88 B |
| Autofac_PropertyInjection | 2,014.46 ns | 1856 B |
| AspectCore_PropertyInjection | 307.55 ns | 336 B |
| Autofac_ConstructorInjection | 1,465.71 ns | 1920 B |
| AspectCore_ConstructorInjection | 284.94 ns | 312 B |

> This set of data is an old benchmark from a historical environment, used to illustrate that the built-in container is relatively lightweight; it does not represent the exact behavior of the current version or your runtime environment. For the latest data, re-run the benchmark in your own environment.

## Next Steps

- [Interceptor Configuration](./interceptor-configuration.md) — the three registration approaches for global interceptors.
- [Third-Party Containers](./third-party-containers.md) — how to integrate AOP when using Autofac / Windsor / LightInject.
- [Configuration Injection](./configuration-injection.md) — injecting field values from `IConfiguration`.
- [Overall Architecture](../architecture/overview.md) — how the container and proxy generation collaborate.
