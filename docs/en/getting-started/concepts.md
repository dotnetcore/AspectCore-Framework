# Core concepts

This page explains the terms that recur throughout AspectCore. Understanding them helps you read the usage guides and architecture docs that follow.

## Aspect

In aspect-oriented programming (AOP), an "aspect" refers to a cross-cutting concern extracted from multiple business objects — for example logging, caching, transactions, or validation. AspectCore lets you write this logic in interceptors and then weave it declaratively into target methods, instead of repeating the same calls in every business method.

## Interceptor

An interceptor is the unit that carries aspect logic. It implements the `IInterceptor` interface, whose core is a single `Invoke` method:

```csharp
Task Invoke(AspectContext context, AspectDelegate next);
```

Inside `Invoke`, the code before `next(context)` is the "before entering the method" logic and the code after it is the "after the method returns" logic; not calling `next` short-circuits the pipeline (skipping the original method). AspectCore provides several forms for defining interceptors:

- `AbstractInterceptorAttribute`: an attribute itself, which can be applied directly to an interface/class/method.
- `AbstractInterceptor`: a plain base class, usually used for global registration (not as an attribute).
- `ServiceInterceptorAttribute`: resolves the real interceptor instance from the container, suitable for interceptors that need constructor injection.
- Delegate interceptor: register a delegate directly with `AddDelegate((ctx, next) => ...)`, without defining a type.

For how to use each form, see [Interceptor basics](../guide/interceptor.md) and [Interceptor configuration](../guide/interceptor-configuration.md).

## Proxy

AspectCore does not modify your original class; instead it generates a **proxy type** that wraps the target object. When a proxy method is called, it first passes through the interceptor pipeline, then forwards to the real implementation. The instance you resolve from the container is in fact this proxy.

A proxy can only weave into overridable members: interface members, or `virtual` members of non-`sealed` classes. There are two engines for generating proxies:

- **DynamicProxy (runtime)**: the default engine, which generates proxies at runtime using `Reflection.Emit`, with no changes to the build process. See [DynamicProxy runtime engine](../architecture/dynamic-proxy.md).
- **SourceGenerator (compile-time)**: explicit opt-in; Roslyn generates the proxy types at compile time, suitable for AOT / trimming scenarios. See [Source Generator compile-time engine](../architecture/source-generator.md).

For the trade-offs between the two, see [Comparing and choosing between the two engines](../architecture/engine-comparison.md).

## Aspect context (AspectContext)

`AspectContext` is the context object that an interceptor's `Invoke` receives; it runs through a single method call. Common members:

| Member | Meaning |
|------|------|
| `ServiceMethod` | Method info on the service (interface/declaration). |
| `ImplementationMethod` | Method info on the implementation type. |
| `ProxyMethod` | Method info on the proxy type. |
| `Parameters` | The method's parameter array; readable and writable. |
| `ReturnValue` | The method's return value; readable and writable (only has a value after `next`). |
| `Implementation` | The proxied implementation instance. |
| `Proxy` | The proxy instance. |
| `ServiceProvider` | The service provider of the current scope; can be used as a service locator. |
| `AdditionalData` | A dictionary for passing data across interceptors. |
| `Invoke(next)` | Continue to the next stage of the pipeline. |
| `Break()` | Short-circuit the pipeline. |

> Note: `AspectContext` has no `ServiceDescriptor` member; to get the service type, use `context.ServiceMethod.DeclaringType`. The return value of an async method needs to be unwrapped with `context.UnwrapAsyncReturnValue()`; see [Async interception](../guide/async-interception.md).

## Join-point predicate (AspectPredicate)

A predicate decides "which methods get intercepted". It is essentially a `delegate bool AspectPredicate(MethodInfo method)`. The `Predicates` factory provides common construction methods and supports the wildcard `*`:

- `Predicates.ForNameSpace("App1")` — match by namespace.
- `Predicates.ForService("*Service")` — match by service type name.
- `Predicates.ForMethod("Query*")` — match by method name.
- `Predicates.Implement(typeof(IFoo))` — match types that implement a given interface/base class.

When registering interceptors globally, you can pass a predicate to limit the scope; see [Conditional interception](../guide/conditional-interception.md).

## The two engines

AspectCore has two engines for generating proxies, sharing the same interceptor configuration API (`ConfigureDynamicProxy(...)`, `IAspectConfiguration`, `Predicates`):

- When you make no new configuration, it uses the runtime **DynamicProxy** (the default, consistent with older versions' behavior).
- After explicitly enabling **SourceGenerator**, proxies can be generated at compile time, and the proxy types are looked up directly at runtime.

The engine is chosen through `ProxyEngineOptions.Engine` (`DynamicProxy` / `SourceGenerator` / `Auto`); for details, see [Comparing and choosing between the two engines](../architecture/engine-comparison.md).

## `[NonAspect]`

`[NonAspect]` is used to explicitly exclude an interface, class, or method so that it is **not proxied**. It is a switch precise to the type/method:

```csharp
[NonAspect]
public interface ICustomService
{
    void Call();
}
```

Besides the attribute, you can also exclude in bulk through `NonAspectPredicates` in the global configuration (wildcards supported); see [Conditional interception](../guide/conditional-interception.md).

## Further reading

- [Overall architecture](../architecture/overview.md) — layering and the runtime flow.
- [Module and package structure design](../architecture/module-design.md) — the responsibility boundaries of each package.
- [Interceptor basics](../guide/interceptor.md) — start defining your own interceptors.
