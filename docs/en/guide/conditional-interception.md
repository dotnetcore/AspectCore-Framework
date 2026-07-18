# Conditional Interception

Conditional interception addresses the question of "which methods should be woven and which should not." AspectCore provides two complementary mechanisms: use `Predicates` predicates to **constrain** the scope of global interceptors (positive matching), and use `[NonAspect]` or `NonAspectPredicates` to **exclude** types/methods you do not want proxied (negative exclusion).

## The Join-Point Predicate AspectPredicate

`AspectPredicate` is essentially `delegate bool AspectPredicate(MethodInfo method)`: returning `true` for a candidate method means it matches. The `Predicates` factory provides four common construction approaches, and all string matching supports the wildcard `*`.

### Predicates.ForNameSpace

Matches by the namespace of the method's declaring type:

```csharp
using AspectCore.Configuration;

// the App1 namespace
config.Interceptors.AddTyped<LogInterceptorAttribute>(Predicates.ForNameSpace("App1"));

// wildcard: any namespace ending with .App1
config.Interceptors.AddTyped<LogInterceptorAttribute>(Predicates.ForNameSpace("*.App1"));
```

### Predicates.ForService

Matches by service type name (for generic types the <code>\`n</code> part is stripped, and it also tries to match the full name):

```csharp
// services whose type name ends with Service
config.Interceptors.AddTyped<LogInterceptorAttribute>(Predicates.ForService("*Service"));
```

### Predicates.ForMethod

Matches by method name; there is also `ForMethod(service, method)` to constrain both service and method at once:

```csharp
// all methods named Query or ending with Query
config.Interceptors.AddTyped<LogInterceptorAttribute>(Predicates.ForMethod("*Query"));

// constrain: Get* methods on *Service services
config.Interceptors.AddTyped<LogInterceptorAttribute>(Predicates.ForMethod("*Service", "Get*"));
```

### Predicates.Implement

Matches types that implement a specified interface or inherit from a specified base class:

```csharp
// all types that implement IRepository
config.Interceptors.AddTyped<LogInterceptorAttribute>(Predicates.Implement(typeof(IRepository)));
```

> `Implement` requires that the type passed in is a class or interface and cannot be `sealed`, otherwise it throws `ArgumentException`.

### Custom Predicates

When you need more complex decisions, pass a `MethodInfo -> bool` delegate directly:

```csharp
config.Interceptors.AddTyped<LogInterceptorAttribute>(
    method => method.Name.StartsWith("Get") && method.ReturnType != typeof(void));
```

### Combining Multiple Predicates

`AddTyped` / `AddServiced` / `AddDelegate` all accept `params AspectPredicate[]`. When multiple predicates are passed, it takes effect as long as **any one** matches:

```csharp
config.Interceptors.AddTyped<LogInterceptorAttribute>(
    Predicates.ForService("*Service"),
    Predicates.ForService("*Repository"));   // matches either Service or Repository
```

## Excluding with [NonAspect]

`[NonAspect]` explicitly marks an interface, class, or method as **not proxied**, with higher priority than global interceptor matching:

```csharp
using AspectCore.DynamicProxy;

[NonAspect]
public interface ICustomService
{
    void Call();
}
```

You can also exclude just a single method:

```csharp
public interface ICustomService
{
    void Call();

    [NonAspect]
    void Diagnostics();   // this method will not be proxied
}
```

## Global Exclusion with NonAspectPredicates

Besides annotating one by one, you can also exclude in bulk in configuration via `NonAspectPredicates`, which likewise supports wildcards. This suits excluding an entire namespace or a set of naming conventions:

```csharp
services.ConfigureDynamicProxy(config =>
{
    // services under the App1 namespace are not proxied
    config.NonAspectPredicates.AddNamespace("App1");

    // services under namespaces whose last segment is App1 are not proxied
    config.NonAspectPredicates.AddNamespace("*.App1");

    // the ICustomService interface is not proxied
    config.NonAspectPredicates.AddService("ICustomService");

    // interfaces and classes with the suffix Service are not proxied
    config.NonAspectPredicates.AddService("*Service");

    // methods named Query are not proxied
    config.NonAspectPredicates.AddMethod("Query");

    // methods with the suffix Query are not proxied
    config.NonAspectPredicates.AddMethod("*Query");
});
```

The methods `NonAspectPredicates` provides correspond to those of `Predicates`: `AddNamespace`, `AddService`, `AddMethod` (including the `AddMethod(service, method)` overload).

## Positive Matching vs Negative Exclusion

| Mechanism | Effect | Where it is used |
|------|------|----------|
| `Predicates.*` | Constrains which methods a global interceptor **matches** | Passed as `AspectPredicate[]` when registering an interceptor |
| `[NonAspect]` | Precisely excludes a specific type/method from **being proxied** | Applied to an interface/class/method |
| `NonAspectPredicates.*` | **Excludes proxying** in bulk by namespace/service/method | In `IAspectConfiguration` configuration |

The two kinds of mechanism can be stacked: first use predicates to delimit the interception scope, then use `[NonAspect]` / `NonAspectPredicates` to carve out individual exceptions.

## Next Steps

- [Interceptor Configuration](./interceptor-configuration.md) — how predicates work with the three registration approaches.
- [Core Concepts](../getting-started/concepts.md) — the conceptual background of predicates and `[NonAspect]`.
- [Overall Architecture](../architecture/overview.md) — when "whether to intercept" is decided.
