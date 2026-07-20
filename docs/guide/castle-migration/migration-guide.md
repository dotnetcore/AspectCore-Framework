# Castle DynamicProxy to AspectCore Migration Guide

This guide provides a comprehensive, step-by-step path for migrating from Castle DynamicProxy
(with or without Windsor) to AspectCore Framework.

## Table of Contents

1. [Overview](#overview)
2. [Container Registration](#container-registration)
3. [Interceptor API Migration](#interceptor-api-migration)
4. [Interceptor Selection](#interceptor-selection)
5. [Service Lifecycle](#service-lifecycle)
6. [Step-by-Step Migration Path](#step-by-step-migration-path)
7. [FAQ](#faq)

---

## Overview

### Why Migrate?

- **NativeAOT support**: Castle relies on `System.Reflection.Emit`, which is unavailable in NativeAOT. AspectCore's Source Generator provides compile-time proxy generation.
- **Native async interception**: AspectCore handles `Task<T>`, `ValueTask<T>`, and `IAsyncEnumerable<T>` without wrapper boilerplate.
- **Modern C# features**: `ref` returns, records, primary constructors, and partial properties are fully supported.
- **MSDI native**: No third-party container required; works directly with `Microsoft.Extensions.DependencyInjection`.
- **Better performance**: Lower allocation and faster steady-state throughput (see `benchmarks/AspectCore.Benchmarks.Competitive/`).

### Migration Strategy

We recommend a **gradual migration** using the `AspectCore.Extensions.CastleCompat` package:

1. Add AspectCore alongside Castle
2. Bridge existing Castle interceptors into AspectCore's pipeline
3. Migrate interceptors one at a time to native AspectCore
4. Remove Castle dependencies

---

## Container Registration

### Castle Windsor

```csharp
// Castle Windsor registration
var container = new WindsorContainer();
container.Register(
    Component.For<IMyService>()
        .ImplementedBy<MyService>()
        .Interceptors(InterceptorReference.ForType<LoggingInterceptor>())
        .Anywhere,
    Component.For<LoggingInterceptor>()
        .LifestyleTransient()
);
```

### AspectCore + MSDI

```csharp
// AspectCore with Microsoft.Extensions.DependencyInjection
var services = new ServiceCollection();

services.AddTransient<IMyService, MyService>();

services.ConfigureDynamicProxy(config =>
{
    config.Interceptors.AddTyped<LoggingInterceptor>(
        Predicates.ForService("*Service*"));
});

services.AddDynamicProxy();
var serviceProvider = services.BuildDynamicProxyProvider();
```

### Key Differences

| Aspect | Castle Windsor | AspectCore + MSDI |
|--------|---------------|-------------------|
| Container | `WindsorContainer` | `IServiceCollection` + `IServiceProvider` |
| Registration | `Component.For<>().ImplementedBy<>()` | `services.AddTransient<,>()` |
| Interceptor binding | `Interceptors(InterceptorReference...)` | `config.Interceptors.AddTyped<>()` |
| Resolution | `container.Resolve<T>()` | `serviceProvider.GetRequiredService<T>()` |
| Disposal | `container.Dispose()` | `serviceProvider.Dispose()` (if `ServiceProvider`) |

---

## Interceptor API Migration

### Castle IInterceptor

```csharp
using Castle.DynamicProxy;

public class LoggingInterceptor : IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        Console.WriteLine($"Before: {invocation.Method.Name}");
        invocation.Proceed();
        Console.WriteLine($"After: {invocation.Method.Name}, returned: {invocation.ReturnValue}");
    }
}
```

### AspectCore AbstractInterceptorAttribute

```csharp
using AspectCore.DynamicProxy;

public class LoggingInterceptor : AbstractInterceptorAttribute
{
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        Console.WriteLine($"Before: {context.ServiceMethod.Name}");
        await next(context);
        Console.WriteLine($"After: {context.ServiceMethod.Name}, returned: {context.ReturnValue}");
    }
}
```

### API Mapping Reference

| Castle (`IInvocation`) | AspectCore (`AspectContext`) |
|------------------------|----------------------------|
| `invocation.Method` | `context.ServiceMethod` |
| `invocation.MethodInvocationTarget` | `context.ImplementationMethod` |
| `invocation.Arguments` | `context.Parameters` |
| `invocation.ReturnValue` | `context.ReturnValue` |
| `invocation.InvocationTarget` | `context.Implementation` |
| `invocation.Proxy` | `context.Proxy` |
| `invocation.Proceed()` | `await next(context)` |
| `invocation.GetArgumentValue(i)` | `context.Parameters[i]` |
| `invocation.SetArgumentValue(i, v)` | `context.Parameters[i] = v` |
| `invocation.GenericArguments` | `context.ServiceMethod.GetGenericArguments()` |
| `invocation.TargetType` | `context.Implementation.GetType()` |

### Async Interception

Castle requires a separate `IAsyncInterceptor` interface (from `Castle.Core.AsyncInterceptor`):

```csharp
// Castle: async interception requires a wrapper
public class AsyncLoggingInterceptor : IAsyncInterceptor
{
    public void InterceptSynchronous(IInvocation invocation) { ... }
    public void InterceptAsynchronous(IInvocation invocation) { ... }
    public void InterceptAsynchronous<TResult>(IInvocation invocation) { ... }
}
```

AspectCore handles async natively - the same interceptor works for sync, `Task`, `ValueTask`, and streams:

```csharp
// AspectCore: one interceptor handles all return types
public class AsyncLoggingInterceptor : AbstractInterceptorAttribute
{
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        Console.WriteLine($"Before: {context.ServiceMethod.Name}");
        await next(context);
        Console.WriteLine($"After: {context.ServiceMethod.Name}");
    }
}
```

---

## Interceptor Selection

### Castle: ProxyGenerationOptions + IInterceptorSelector

```csharp
// Castle: using IInterceptorSelector
public class MySelector : IInterceptorSelector
{
    public IInterceptor[] SelectInterceptors(Type type, MethodInfo method, IInterceptor[] interceptors)
    {
        if (method.Name.StartsWith("Get"))
            return interceptors.Where(i => i is CachingInterceptor).ToArray();
        return interceptors;
    }
}

// Usage
var options = new ProxyGenerationOptions { Selector = new MySelector() };
var proxy = generator.CreateInterfaceProxyWithTarget<IService>(target, options, interceptors);
```

### AspectCore: Predicates and Attributes

```csharp
// Option 1: Global predicate-based configuration
services.ConfigureDynamicProxy(config =>
{
    // Apply to methods starting with "Get"
    config.Interceptors.AddTyped<CachingInterceptor>(
        method => method.Name.StartsWith("Get"));

    // Apply to all services matching a pattern
    config.Interceptors.AddTyped<LoggingInterceptor>(
        Predicates.ForService("*Service*"));

    // Exclude specific methods/services
    config.NonAspectPredicates.Add(
        method => method.Name == "ToString");
});

// Option 2: Attribute-based (on interface or class)
public interface IMyService
{
    [CachingInterceptor]
    string GetValue(string key);

    string SetValue(string key, string value); // not intercepted
}

// Option 3: Exclude from interception
[NonAspect]
public interface INotIntercepted { }
```

---

## Service Lifecycle

### Castle Windsor Lifestyles

```csharp
// Castle Windsor lifestyles
container.Register(
    Component.For<IService>().ImplementedBy<Service>().LifestyleTransient(),
    Component.For<ICache>().ImplementedBy<Cache>().LifestyleSingleton(),
    Component.For<ISession>().ImplementedBy<Session>().LifestyleScoped()
);
```

### MSDI Service Lifetimes

```csharp
// MSDI lifetimes (direct equivalent)
services.AddTransient<IService, Service>();
services.AddSingleton<ICache, Cache>();
services.AddScoped<ISession, Session>();
```

### Mapping

| Castle Windsor | MSDI |
|----------------|------|
| `LifestyleTransient()` | `AddTransient<,>()` |
| `LifestyleSingleton()` | `AddSingleton<,>()` |
| `LifestyleScoped()` | `AddScoped<,>()` |
| `LifestylePerThread()` | No direct equivalent (use scoped) |
| `LifestylePooled()` | No direct equivalent (use `ObjectPool<T>`) |
| `LifestyleBoundTo<T>()` | No direct equivalent |

---

## Step-by-Step Migration Path

### Phase 1: Add AspectCore alongside Castle (coexistence)

```xml
<!-- Add to your .csproj -->
<PackageReference Include="AspectCore.Extensions.CastleCompat" Version="x.y.z" />
```

```csharp
// Bridge existing Castle interceptors into AspectCore
services.AddCastleInterceptor(new MyExistingCastleInterceptor(),
    Predicates.ForService("*Service*"));

services.AddDynamicProxy();
```

At this point, your existing Castle interceptors run inside AspectCore's pipeline with no logic changes needed.

### Phase 2: Migrate interceptors one at a time

Convert each Castle `IInterceptor` to an AspectCore `AbstractInterceptorAttribute`:

```csharp
// Before (Castle)
public class TimingInterceptor : IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        var sw = Stopwatch.StartNew();
        invocation.Proceed();
        sw.Stop();
        Console.WriteLine($"{invocation.Method.Name} took {sw.ElapsedMilliseconds}ms");
    }
}

// After (AspectCore)
public class TimingInterceptor : AbstractInterceptorAttribute
{
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        var sw = Stopwatch.StartNew();
        await next(context);
        sw.Stop();
        Console.WriteLine($"{context.ServiceMethod.Name} took {sw.ElapsedMilliseconds}ms");
    }
}
```

### Phase 3: Remove Castle registration

```csharp
// Replace Castle Windsor container setup
// Before:
var container = new WindsorContainer();
container.Install(FromAssembly.This());
var serviceProvider = container.Resolve<IServiceProvider>();

// After:
var services = new ServiceCollection();
services.ConfigureDynamicProxy(config => { /* ... */ });
services.AddDynamicProxy();
var serviceProvider = services.BuildDynamicProxyProvider();
```

### Phase 4: Remove Castle packages

Once all interceptors are migrated:

```xml
<!-- Remove from .csproj -->
<!-- <PackageReference Include="Castle.Core" /> -->
<!-- <PackageReference Include="Castle.Windsor" /> -->
<!-- <PackageReference Include="AspectCore.Extensions.CastleCompat" /> -->

<!-- Keep only -->
<PackageReference Include="AspectCore.Core" Version="x.y.z" />
<PackageReference Include="AspectCore.Extensions.DependencyInjection" Version="x.y.z" />
```

---

## FAQ

### Q: Can I use both Castle and AspectCore interceptors on the same service?

**A:** Yes, during migration you can mix both. Castle interceptors run through the
`CastleInterceptorAdapter` and participate in AspectCore's pipeline alongside
native interceptors. Ordering is controlled by registration order and the `Order` property.

### Q: What about Castle's `IChangeProxyTarget`?

**A:** AspectCore does not support changing the proxy target mid-invocation. If you rely
on this Castle feature, you'll need to refactor to use a different pattern (e.g., a
service locator or factory pattern within the interceptor).

### Q: Will my async interceptors work with the compatibility shim?

**A:** The compatibility shim handles basic async scenarios (the method returns a Task/ValueTask
and the Castle interceptor calls Proceed). However, for full async-aware interception
(e.g., inspecting the Task result, handling cancellation), you should migrate to
native AspectCore interceptors.

### Q: What about performance impact during the migration period?

**A:** The compatibility shim adds a small overhead (one additional object allocation per
invocation for the `IInvocation` adapter). This is negligible for most applications
but can be measured using the competitive benchmarks. Once fully migrated to native
AspectCore interceptors, this overhead disappears.

### Q: Does AspectCore support intercepting non-virtual methods?

**A:** Like Castle, AspectCore's DynamicProxy engine requires virtual methods for class proxies.
However, when using interface-based proxies, all interface methods are interceptable
regardless of the implementation's virtual/non-virtual status.

### Q: What about ref/out parameters?

**A:** Both frameworks support `ref` and `out` parameters. The values are accessible via
`context.Parameters` (AspectCore) or `invocation.Arguments` (Castle).

### Q: What about ref return values?

**A:** Castle DynamicProxy does NOT support `ref` or `ref readonly` return values.
AspectCore supports them natively. If you have services with ref returns, they
cannot be proxied by Castle but can be proxied by AspectCore.

### Q: Is there a code analyzer to help with migration?

**A:** Not currently included, but the pattern is mechanical:
- `IInterceptor.Intercept(IInvocation)` becomes `AbstractInterceptorAttribute.Invoke(AspectContext, AspectDelegate)`
- `invocation.Proceed()` becomes `await next(context)`
- Property accesses map 1:1 (see the API mapping table above)

### Q: When should I NOT migrate?

**A:** Consider staying with Castle if:
- You heavily rely on Castle's mixin support
- You use `IChangeProxyTarget` extensively
- Your application is in maintenance mode with no plans for .NET upgrades
- You need Castle Windsor's specific lifecycle features (pooled, per-thread, bound-to)

### Q: Can I use the Source Generator after migration?

**A:** Yes! Once migrated to AspectCore, you can opt into the Source Generator engine for
NativeAOT compatibility. Add `[AspectCoreGenerateProxy]` to your service classes and
configure `ProxyEngine.SourceGenerator` in your setup.
