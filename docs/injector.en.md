# IoC Container and Dependency Injection in AspectCore

The IoC pattern and dependency injection have been a very popular pattern in recent years, and everyone should be familiar with it. ASP.NET Core provides dependency injection as built-in infrastructure. For readers still unfamiliar with dependency injection, you can refer to the relevant chapter on dependency injection in the ASP.NET Core documentation we translated: [ASP.NET Core Chinese Documentation Chapter 3 Principles (10) Dependency Injection](http://www.cnblogs.com/dotNETCoreSG/p/aspnetcore-3_10-dependency-injection.html).

Due to the importance of IoC, AspectCore provides a lightweight, high-performance IoC container `AspectCore.Injector` that can be seamlessly integrated with AOP while providing AOP features.

## Getting Started

`AspectCore.DependencyInjection` is built into the AspectCore.Core package. You can get it through nuget:

```
   Install-Package AspectCore.Core -pre
```

## Container and Service Registration

In AspectCore.Injector, the container is named `IServiceContext`. Use the default implementation of the container to create a container. It provides three ways to register services: type, instance, and factory:

```csharp
IServiceContext services = new ServiceContext();

// Register service using type
services.AddType<ILogger, Logger>();

// Register service using instance, service lifecycle is limited to singleton
services.AddInstance<ITaskService>(new TaskService());

// Register service using delegate factory
services.AddDelegate<ITaskService, TaskService>(resolver => new TaskService());
```

## Service Resolution

AspectCore.DependencyInjection resolves services through IServiceResolver:

```csharp
// Create service resolver
IServiceResolver serviceResolver = services.Build();

// Resolve single service
ISampleService sampleService = serviceResolver.Resolve<ISampleService>();

// Resolve single service and verify if it's null, throw exception if null
ISampleService sampleServiceRequired = serviceResolver.ResolveRequired<ISampleService>();

// Resolve service collection, returns empty collection if not registered
IEnumerable<ISampleService> sampleServices = serviceResolver.ResolveMany<ISampleService>();
```

## Dependency Injection

AspectCore.DependencyInjection provides constructor injection and property injection:

```csharp
public interface ISampleService
{
}

public class SampleService : ISampleService
{
    private readonly ISampleRepository _sampleRepository;
    private readonly ILogger _logger;

    // Constructor injection
    public SampleService(ISampleRepository sampleRepository, ILogger logger)
    {
        _sampleRepository = sampleRepository;
        _logger = logger;
    }
}

public interface ISampleRepository
{
}

public class SampleRepository : ISampleRepository
{
    // Property injection. Property injection conditions: marked with FromServiceContext attribute and allows set. Properties meeting conditions are automatically injected
    [FromServiceContext]
    public ILogger Logger { get; set; }
}
```

## Service Lifetimes

AspectCore.DependencyInjection provides the following lifetimes:

* **Transient** - Transient lifetime services are created each time they are requested. This lifetime is suitable for lightweight, stateless services.

* **Scoped** - Scoped lifetime services are created once per scope.

* **Singleton** - Singleton lifetime services are created the first time they are resolved, and every subsequent resolution will use the same instance. If your application needs singleton behavior, it's recommended to let the service container manage the service lifecycle rather than implementing singleton pattern and managing object lifecycle in your own classes.

## AOP Integration

AspectCore.DynamicProxy AOP integration is enabled by default in AspectCore.Injector and can be configured through the Configure method of IServiceContainer.

```
services.Configure(config =>
{
    config.Interceptors.AddTyped<SampleInterceptor>();
});
```

## Using AspectCore.Injector in ASP.NET Core

Install the AspectCore.Extensions.DependencyInjection nuget package:

```
    Install-Package AspectCore.Extensions.DependencyInjection -pre
```

Modify ConfigureServices:

```
public IServiceProvider ConfigureServices(IServiceCollection services)
{
    // Add your services...

    // Add services from IServiceCollection to ServiceContainer
    var container = services.ToServiceContainer();
    return container.Build();
}
```

Just two lines of code to use AspectCore.Injector to replace the default DependencyInjection in ASP.NET Core.

## Performance

Autofac is one of the more popular IoC containers in .NET/.NET Core. We use Autofac (version 4.6.2) as a performance comparison target. We compare performance from three aspects: resolving simple objects, property injection, and constructor injection.

The Benchmark class is written as follows:

```csharp
[AllStatisticsColumn]
[MemoryDiagnoser]
public class Benckmarks
{
    private readonly IServiceResolver serviceResolver;
    private readonly IContainer container;

    public Benckmarks()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder.RegisterType<Logger>().As<ILogger>().InstancePerDependency();
        containerBuilder.RegisterType<TaskService>().As<ITaskService>().InstancePerDependency();
        containerBuilder.RegisterType<SampleRepository>().As<ISampleRepository>().InstancePerDependency().PropertiesAutowired();
        containerBuilder.RegisterType<SampleService2>().As<ISampleService>().InstancePerDependency();
        container = containerBuilder.Build();

        var serviceContext = new ServiceContext();
        serviceContext.AddType<ILogger, Logger>(Lifetime.Transient);
        serviceContext.AddType<ITaskService, TaskService>(Lifetime.Transient);
        serviceContext.AddType<ISampleRepository, SampleRepository>(Lifetime.Transient);
        serviceContext.AddType<ISampleService, SampleService2>(Lifetime.Transient);
        serviceResolver = serviceContext.Build();
    }

    [Benchmark]
    public object Autofac_Sample_Resolve()
    {
        return container.Resolve<ITaskService>();
    }

    [Benchmark]
    public object AspectCore_Sample_Resolve()
    {
        return serviceResolver.Resolve<ITaskService>();
    }

    [Benchmark]
    public object Autofac_PropertyInjection()
    {
        return container.Resolve<ISampleRepository>();
    }

    [Benchmark]
    public object AspectCore_PropertyInjection()
    {
        return serviceResolver.Resolve<ISampleRepository>();
    }

    [Benchmark]
    public object Autofac_ConstructorInjection()
    {
        return container.Resolve<ISampleService>();
    }

    [Benchmark]
    public object AspectCore_ConstructorInjection()
    {
        return serviceResolver.Resolve<ISampleService>();
    }
}
```

Run Benchmark in Release mode:

```
BenchmarkDotNet=v0.10.8, OS=Windows 10 Threshold 2 (10.0.10586)
Processor=Intel Core i5-4590 CPU 3.30GHz (Haswell), ProcessorCount=4
Frequency=3215206 Hz, Resolution=311.0221 ns, Timer=TSC
dotnet cli version=2.0.0
  [Host]     : .NET Core 4.6.00001.0, 64bit RyuJIT
  DefaultJob : .NET Core 4.6.00001.0, 64bit RyuJIT

 |                          Method |        Mean |         Min |         Max |         Op/s |  Gen 0 | Allocated |
 |-------------------------------- |------------:|------------:|------------:|-------------:|-------:|----------:|
 |          Autofac_Sample_Resolve |   494.83 ns |   482.52 ns |   506.58 ns |  2,020,908.9 | 0.2384 |     752 B |
 |       AspectCore_Sample_Resolve |    88.52 ns |    87.92 ns |    89.31 ns | 11,296,837.3 | 0.0279 |      88 B |
 |       Autofac_PropertyInjection | 2,014.46 ns | 2,004.18 ns | 2,028.83 ns |    496,411.0 | 0.5875 |    1856 B |
 |    AspectCore_PropertyInjection |   307.55 ns |   303.61 ns |   310.74 ns |  3,251,544.6 | 0.1063 |     336 B |
 |    Autofac_ConstructorInjection | 1,465.71 ns | 1,454.43 ns |  480.38 ns |    682,263.5 | 0.6084 |    1920 B |
 | AspectCore_ConstructorInjection |   284.94 ns |   283.55 ns |   286.05 ns |  3,509,500.8 | 0.0987 |     312 B |
```

Sample: [IoC-Sample](https://github.com/AspectCore/IoC-Sample)
