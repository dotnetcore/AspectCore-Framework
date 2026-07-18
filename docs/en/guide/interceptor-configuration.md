# Interceptor Configuration

Global interceptors are registered via `ConfigureDynamicProxy(Action<IAspectConfiguration>)`, where `IAspectConfiguration.Interceptors` provides three registration approaches: `AddTyped`, `AddServiced`, and `AddDelegate`. All three can be accompanied by an `AspectPredicate[]` to constrain their scope. This page explains their differences and when to choose each.

## Attribute Interceptors vs Global Interceptors

- **Attribute interceptor**: apply the interceptor attribute to an interface/class/method, and its scope is the annotated location. See [Interceptor Basics](./interceptor.md).
- **Global interceptor**: register it once in configuration, applying to all services, or use a predicate to constrain it to a subset of services. Suitable for cross-cutting logic such as logging, monitoring, and transactions.

The entry point for global registration is `ConfigureDynamicProxy`:

```csharp
using AspectCore.Configuration;
using AspectCore.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

services.ConfigureDynamicProxy(config =>
{
    config.Interceptors.AddTyped<CustomInterceptorAttribute>();
});
```

## AddTyped: Register by Type

`AddTyped<TInterceptor>()` lets AspectCore create the interceptor instance itself. This is the most common approach, and **it is the only one that supports property injection inside the interceptor** (`[FromServiceContext]`):

```csharp
services.ConfigureDynamicProxy(config =>
{
    config.Interceptors.AddTyped<CustomInterceptorAttribute>();
});
```

If the interceptor has constructor parameters, pass them in via the `args` overload:

```csharp
public class CustomInterceptorAttribute : AbstractInterceptorAttribute
{
    private readonly string _name;
    public CustomInterceptorAttribute(string name) => _name = name;

    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        Console.WriteLine($"Before {_name}");
        await next(context);
        Console.WriteLine($"After {_name}");
    }
}

services.ConfigureDynamicProxy(config =>
{
    config.Interceptors.AddTyped<CustomInterceptorAttribute>(args: new object[] { "custom" });
});
```

> `AddTyped` also has a non-generic overload `AddTyped(Type interceptorType, ...)`, for specifying the type dynamically at runtime.

## AddServiced: Resolve from the Container

`AddServiced<TInterceptor>()` does not create the instance itself but instead **resolves the interceptor from the DI container**. This suits scenarios where the interceptor needs constructor injection—first register the interceptor as a service, then reference it via `AddServiced`:

```csharp
// 1. register the interceptor as a service (it may have constructor dependencies)
services.AddTransient<CustomInterceptorAttribute>(
    provider => new CustomInterceptorAttribute("custom"));

// 2. add it to global interceptors as an "already-registered service"
services.ConfigureDynamicProxy(config =>
{
    config.Interceptors.AddServiced<CustomInterceptorAttribute>();
});
```

`AddTyped` and `AddServiced` differ in their support for dependency injection:

| Registration approach | Property injection `[FromServiceContext]` | Constructor injection |
|----------|-------------------------------|------------|
| `AddTyped<T>()` | Works | Requires passing `args` manually |
| `AddServiced<T>()` (paired with `services.Add...<T>()`) | Not applicable | Done by the container |

## AddDelegate: Register a Delegate

`AddDelegate` registers a piece of delegate interception logic directly, with no need to define an interceptor type; suitable for lightweight, one-off aspects:

```csharp
services.ConfigureDynamicProxy(config =>
{
    config.Interceptors.AddDelegate(async (context, next) =>
    {
        Console.WriteLine("before");
        await next(context);
        Console.WriteLine("after");
    });
});
```

`AddDelegate` also has an overload with `order`, used to control its position in the interceptor chain:

```csharp
config.Interceptors.AddDelegate(async (context, next) => await next(context), order: 1);
```

## Constraining Scope with Predicates

All three registration approaches accept `params AspectPredicate[]`, used to constrain a global interceptor to a subset of services/methods. The `Predicates` factory supports the wildcard `*`:

```csharp
services.ConfigureDynamicProxy(config =>
{
    // only applies to services whose type name ends with Service
    config.Interceptors.AddTyped<CustomInterceptorAttribute>(Predicates.ForService("*Service"));
});
```

You can also pass an `AspectPredicate` delegate directly (`MethodInfo -> bool`) for finer-grained decisions:

```csharp
services.ConfigureDynamicProxy(config =>
{
    config.Interceptors.AddTyped<CustomInterceptorAttribute>(
        method => method.Name.EndsWith("Async"));
});
```

For the full usage of predicates (`ForNameSpace` / `ForService` / `ForMethod` / `Implement`) and how to exclude with `[NonAspect]`, see [Conditional Interception](./conditional-interception.md).

## Configuring in Other Hosts

The examples above are based on `Microsoft.Extensions.DependencyInjection`. The same `IAspectConfiguration.Interceptors` API is also used for:

- The built-in container `IServiceContext`: `serviceContext.Configure(config => config.Interceptors.AddTyped<T>())`.
- Third-party containers: the registration entry points for Autofac / Windsor / LightInject all accept the same `Action<IAspectConfiguration>`, see [Third-Party Containers](./third-party-containers.md).

## Next Steps

- [Conditional Interception](./conditional-interception.md) — full explanation of predicates and `[NonAspect]`.
- [Dependency Injection Integration](./dependency-injection.md) — details of the three injection approaches inside interceptors.
- [Async Interception](./async-interception.md) — handling async return values.
