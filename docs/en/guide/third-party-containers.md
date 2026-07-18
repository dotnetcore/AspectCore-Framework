# Third-Party Containers

If your project already uses Autofac, Castle Windsor, or LightInject, you can enable dynamic proxying without switching to AspectCore's built-in container. Each container has a corresponding integration package that provides a one-line registration entry point and accepts the same `Action<IAspectConfiguration>` as other hosts for configuring interceptors. The Generic Host (`IHostBuilder`) has its own dedicated extension.

All integration entry points share the same interceptor configuration API; for how to define and register interceptors, see [Interceptor Configuration](./interceptor-configuration.md).

## Autofac

Package: `AspectCore.Extensions.Autofac`. Call `RegisterDynamicProxy(...)` on the `ContainerBuilder`:

```csharp
using AspectCore.Configuration;
using AspectCore.Extensions.Autofac;
using Autofac;

var containerBuilder = new ContainerBuilder();
// register your services ...

containerBuilder.RegisterDynamicProxy(config =>
{
    config.Interceptors.AddTyped<MethodExecuteLoggerInterceptor>(Predicates.ForService("*Service"));
});

var container = containerBuilder.Build();
```

`RegisterDynamicProxy` has two overloads: one accepts an optional `Action<IAspectConfiguration> configure`, and the other additionally accepts a pre-built `IAspectConfiguration`:

```csharp
// ContainerBuilder RegisterDynamicProxy(this ContainerBuilder, Action<IAspectConfiguration> configure = null)
// ContainerBuilder RegisterDynamicProxy(this ContainerBuilder, IAspectConfiguration configuration, Action<IAspectConfiguration> configure = null)
```

## Castle Windsor

Package: `AspectCore.Extensions.Windsor`. Call `AddAspectCoreFacility(...)` on the container:

```csharp
using AspectCore.Configuration;
using AspectCore.Extensions.Windsor;
using Castle.Windsor;

var container = new WindsorContainer().AddAspectCoreFacility(config =>
{
    config.Interceptors.AddTyped<CustomInterceptorAttribute>();
});
```

Calling it with no arguments uses the default configuration:

```csharp
var container = new WindsorContainer().AddAspectCoreFacility();
```

`AddAspectCoreFacility` provides two overloads for `IWindsorContainer` and `IKernel`, both accepting an optional `Action<IAspectConfiguration> configure`; the facility is idempotent, so adding it repeatedly does not register it more than once.

## LightInject

Package: `AspectCore.Extensions.LightInject`. Call `RegisterDynamicProxy(...)` on LightInject's `IServiceContainer`:

```csharp
using AspectCore.Configuration;
using AspectCore.Extensions.LightInject;
using LightInject;

var container = new ServiceContainer().RegisterDynamicProxy(config =>
{
    config.Interceptors.AddDelegate(
        next => ctx => next(ctx),
        Predicates.ForNameSpace("Your.Namespace"));
});
```

It likewise supports calling without configuration (`container.RegisterDynamicProxy();`), as well as an overload that additionally accepts a pre-built `IAspectConfiguration`.

## Generic Host (Hosting)

Package: `AspectCore.Extensions.Hosting`. It targets `IHostBuilder` and integrates the built-in container `IServiceContext` into the host.

Use `UseServiceContext()` to take over the container, optionally passing a delegate to do registration and configuration:

```csharp
using AspectCore.DependencyInjection;
using AspectCore.Extensions.Hosting;
using Microsoft.Extensions.Hosting;

hostBuilder.UseServiceContext(container =>
{
    container.AddType<IService, Service>();
    container.Configure(config =>
    {
        config.Interceptors.AddDelegate(async (ctx, next) =>
        {
            await next(ctx);
            ctx.ReturnValue = "proxy";
        });
    });
});
```

You can also use `UseDynamicProxy()` together with `ConfigureDynamicProxy(...)` to register interceptors on `IServiceCollection`:

```csharp
using AspectCore.Configuration;
using AspectCore.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

hostBuilder
    .UseDynamicProxy()
    .ConfigureDynamicProxy((services, config) =>
    {
        config.Interceptors.AddTyped<CustomInterceptorAttribute>();
    });
```

`UseServiceContext` also has an `Action<HostBuilderContext, IServiceContext>` overload and a parameterless overload; `ConfigureDynamicProxy` also has an overload with `HostBuilderContext`.

## Entry Points at a Glance

| Container | Package | Entry method | Extension target |
|------|----|----------|----------|
| Autofac | `AspectCore.Extensions.Autofac` | `RegisterDynamicProxy(...)` | `ContainerBuilder` |
| Castle Windsor | `AspectCore.Extensions.Windsor` | `AddAspectCoreFacility(...)` | `IWindsorContainer` / `IKernel` |
| LightInject | `AspectCore.Extensions.LightInject` | `RegisterDynamicProxy(...)` | `IServiceContainer` |
| Generic Host | `AspectCore.Extensions.Hosting` | `UseServiceContext()` / `UseDynamicProxy()` | `IHostBuilder` |

## Next Steps

- [Interceptor Configuration](./interceptor-configuration.md) — the three registration approaches of `IAspectConfiguration.Interceptors`.
- [Dependency Injection Integration](./dependency-injection.md) — the built-in container and MS.DI integration.
- [Module and Package Structure Design](../architecture/module-design.md) — the responsibility boundaries of each integration package.
