# Interceptor Basics

Interceptors are the core of how AspectCore carries aspect logic. This page explains how to define an interceptor, apply it to an interface/class/method, and what the three behavior switches `Order`, `AllowMultiple`, and `Inherited` do.

## Defining an Interceptor

An interceptor implements the `IInterceptor` interface, whose core is a single `Invoke` method. AspectCore provides two base classes:

- `AbstractInterceptorAttribute`: itself an `Attribute`, it can be applied directly to an interface, class, or method, and can also be used for global registration.
- `AbstractInterceptor`: a plain base class, not an attribute, typically used only for global registration.

The signature of `Invoke` is fixed as `Task Invoke(AspectContext context, AspectDelegate next)`. Calling `next(context)` inside it hands control to the next stage of the pipeline; the code before and after is your aspect logic:

```csharp
using System;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;

public class CustomInterceptorAttribute : AbstractInterceptorAttribute
{
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        try
        {
            Console.WriteLine("Before service call");
            await next(context);
        }
        catch (Exception)
        {
            Console.WriteLine("Service threw an exception!");
            throw;
        }
        finally
        {
            Console.WriteLine("After service call");
        }
    }
}
```

> `AbstractInterceptorAttribute` carries `[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Interface, Inherited = false)]`, meaning it can only be applied to methods, classes, or interfaces, and by default is not passed down through inheritance.

## Attribute Interceptors vs Global Interceptors

The same interceptor can take effect in two ways:

- **Attribute interceptor**: apply the interceptor attribute to the target, and its scope is the annotated member. Suitable for weaving precisely and on demand.
- **Global interceptor**: register it via `ConfigureDynamicProxy(config => config.Interceptors.Add...)`, and it applies to all services (or those matching a predicate). Suitable for cross-cutting scenarios such as logging and monitoring that span all services.

This page focuses on where attribute interceptors are applied; the three global registration approaches (`AddTyped` / `AddServiced` / `AddDelegate`) and scope predicates are covered in [Interceptor Configuration](./interceptor-configuration.md).

## Applying to Methods, Classes, and Interfaces

`AbstractInterceptorAttribute` can be applied at three levels, with progressively wider scope:

**Applied to a method** — intercepts only that method:

```csharp
public interface ICustomService
{
    [CustomInterceptor]
    void Call();

    void Untouched();   // not intercepted
}
```

**Applied to an interface or class** — intercepts all proxiable members under it:

```csharp
[CustomInterceptor]
public interface ICustomService
{
    void Call();
    string Query();     // both methods are intercepted
}
```

For a **class**, only `virtual` members are woven (interface members are always proxiable). For which members can be proxied, see [DynamicProxy Runtime Engine](../architecture/dynamic-proxy.md).

## Reading Method Information

Inside an interceptor, obtain method metadata through `AspectContext`. Note that you use `ServiceMethod` / `ImplementationMethod` / `ProxyMethod`; `AspectContext` has no `ServiceDescriptor` member:

```csharp
public class MethodInfoInterceptorAttribute : AbstractInterceptorAttribute
{
    public override Task Invoke(AspectContext context, AspectDelegate next)
    {
        // service (declaring) type and method
        var serviceType = context.ServiceMethod.DeclaringType;
        Console.WriteLine($"Service: {serviceType.Name}.{context.ServiceMethod.Name}");

        // method on the implementation type
        Console.WriteLine($"Implementation: {context.ImplementationMethod.DeclaringType?.Name}");

        // parameters
        Console.WriteLine($"Parameters: {string.Join(", ", context.Parameters)}");

        return next(context);
    }
}
```

For more detail on reading and modifying parameters and return values, see [Async Interception](./async-interception.md) and [Common Scenarios](./common-scenarios.md).

## `Order`: Controlling Execution Order

When multiple interceptors match the same method, they form an interceptor chain. `Order` determines their ordering: the smaller the value, the more outer the position (it enters earlier and exits later). `AbstractInterceptorAttribute.Order` defaults to `0` and is read/write:

```csharp
public class LogInterceptorAttribute : AbstractInterceptorAttribute
{
    public LogInterceptorAttribute() => Order = 1;   // outer layer

    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        Console.WriteLine("[Log] enter");
        await next(context);
        Console.WriteLine("[Log] exit");
    }
}
```

## `AllowMultiple`: Whether Duplicate Application Is Allowed

`AllowMultiple` indicates whether the same interceptor type can take effect more than once on a single target. `AbstractInterceptorAttribute` defaults to `false` (duplicates not allowed). If you really need to stack interceptors of the same kind across multiple levels, override this property to return `true`:

```csharp
public class TagInterceptorAttribute : AbstractInterceptorAttribute
{
    public override bool AllowMultiple => true;

    public override Task Invoke(AspectContext context, AspectDelegate next)
        => next(context);
}
```

> `ServiceInterceptorAttribute`'s `AllowMultiple` is `true`.

## `Inherited`: Whether It Is Passed Down Through Inheritance

`Inherited` determines whether an interceptor applied to an interface/base-class member continues to take effect on derived implementations. It defaults to `false`. When set to `true`, the implementation class inherits the interceptor declared on the base type:

```csharp
public class AuditInterceptorAttribute : AbstractInterceptorAttribute
{
    public AuditInterceptorAttribute() => Inherited = true;

    public override Task Invoke(AspectContext context, AspectDelegate next)
        => next(context);
}
```

## Dependency Injection in Interceptors

An interceptor can obtain the services it needs; the approach depends on how it was registered:

- **Property injection**: a property in the interceptor with a `public get/set` annotated with `[FromServiceContext]` is injected automatically. Only works when registered with `AddTyped<T>()`.
- **Constructor injection**: requires the interceptor to be activated as a service, using `AddServiced<T>()` or `ServiceInterceptorAttribute`.
- **Service locator**: at any time you can obtain a service via `context.ServiceProvider.GetService<T>()`.

For the applicable conditions and registration approaches of all three, see [Interceptor Configuration](./interceptor-configuration.md) and [Dependency Injection Integration](./dependency-injection.md).

## Next Steps

- [Interceptor Configuration](./interceptor-configuration.md) — the three global registration approaches and scope.
- [Async Interception](./async-interception.md) — handling `Task` / `ValueTask` / `IAsyncEnumerable`.
- [Conditional Interception](./conditional-interception.md) — precisely controlling weaving scope with predicates and `[NonAspect]`.
