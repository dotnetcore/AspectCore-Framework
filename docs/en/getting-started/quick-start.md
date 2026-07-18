# Quick start

This page uses a minimal runnable example to walk you through the full AspectCore flow: define an interceptor, apply it to a method, use `Microsoft.Extensions.DependencyInjection` to take over the container, then call the service and see the interception take effect.

## Prerequisites

- A .NET 6.0 or later project (a console app is fine).
- `AspectCore.Extensions.DependencyInjection` installed:

```bash
dotnet add package AspectCore.Extensions.DependencyInjection
```

For installation notes and package selection, see [Installation](./installation.md).

## 1. Define an interceptor

The most common approach is to inherit from `AbstractInterceptorAttribute`, which is itself an `Attribute` and can be applied directly to an interface, class, or method. In `Invoke`, call `next(context)` (or `context.Invoke(next)`) to hand control to the next stage; the code before and after it is your aspect logic:

```csharp
using System;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;

public class LoggingInterceptorAttribute : AbstractInterceptorAttribute
{
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        Console.WriteLine($"[Before] {context.ImplementationMethod.Name}");
        try
        {
            await next(context);
        }
        finally
        {
            Console.WriteLine($"[After] {context.ImplementationMethod.Name}");
        }
    }
}
```

## 2. Define a service and apply the interceptor

Apply the interceptor attribute to an interface method. AspectCore generates an interface proxy for `ICustomService` and weaves in the interception logic when `Call()` is invoked:

```csharp
using System;
using AspectCore.DynamicProxy;

public interface ICustomService
{
    [LoggingInterceptor]
    void Call();
}

public class CustomService : ICustomService
{
    public void Call() => Console.WriteLine("service calling...");
}
```

## 3. Take over the container with MS.DI and run

After registering the service, call `ConfigureDynamicProxy()` to enable dynamic proxying. When building the `ServiceProvider`, use `BuildDynamicProxyProvider()` (the simplest choice in a console scenario); the resolved instance is the woven proxy:

```csharp
using AspectCore.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddTransient<ICustomService, CustomService>();
services.ConfigureDynamicProxy();

var provider = services.BuildDynamicProxyProvider();
provider.GetRequiredService<ICustomService>().Call();
```

Running it outputs:

```text
[Before] Call
service calling...
[After] Call
```

## Taking over the container in ASP.NET Core / the generic host

Web applications usually do not call `BuildDynamicProxyProvider()` manually; instead they let the host take over container construction through an `IServiceProviderFactory`. Attach `DynamicProxyServiceProviderFactory` on `Program`/`CreateHostBuilder`, and leave everything else unchanged:

```csharp
using AspectCore.Extensions.DependencyInjection;

Host.CreateDefaultBuilder(args)
    .UseServiceProviderFactory(new DynamicProxyServiceProviderFactory())
    .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
```

Register services as usual in `ConfigureServices`, and call `services.ConfigureDynamicProxy()` once:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddTransient<ICustomService, CustomService>();
    services.ConfigureDynamicProxy();
}
```

## What a proxy can intercept

AspectCore's runtime proxy can only weave into **overridable** members:

- **Interface proxy**: all members on the interface can be intercepted (as in the example above).
- **Class proxy**: only `virtual` members of non-`sealed` classes are intercepted; `sealed` classes and static members cannot be intercepted.

For details, see [DynamicProxy runtime engine](../architecture/dynamic-proxy.md).

## Next steps

- [Core concepts](./concepts.md) — clarify terms such as aspect, interceptor, proxy, and aspect context.
- [Interceptor basics](../guide/interceptor.md) — attribute interceptors and global interceptors, `Order`, `AllowMultiple`.
- [Interceptor configuration](../guide/interceptor-configuration.md) — the three registration approaches and scope predicates.
