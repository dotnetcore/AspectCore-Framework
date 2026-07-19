# Reflection Extensions

`AspectCore.Extensions.Reflection` provides a set of high-performance reflection-invocation extensions. Its entry point is `GetReflector()`, which wraps members such as `MethodInfo`, `ConstructorInfo`, `PropertyInfo`, `FieldInfo`, and `Type` into corresponding reflectors, compiles accessors via IL, and caches them, achieving invocation performance about two orders of magnitude higher than native reflection and close to hard-coded calls. This package does not depend on AOP and can be used standalone.

## Installation

```bash
dotnet add package AspectCore.Extensions.Reflection
```

## Entry Point: GetReflector()

`GetReflector()` is a set of extension methods (static class `ReflectorExtensions`, namespace `AspectCore.Extensions.Reflection`) that return the corresponding reflector by member type:

| Extension target | Return type |
|----------|----------|
| `Type` / `TypeInfo` | `TypeReflector` |
| `ConstructorInfo` | `ConstructorReflector` |
| `MethodInfo` | `MethodReflector` |
| `PropertyInfo` | `PropertyReflector` |
| `FieldInfo` | `FieldReflector` |
| `ParameterInfo` | `ParameterReflector` |

`MethodInfo` and `PropertyInfo` also have overloads that take `CallOptions`.

> Attribute reflection (`CustomAttributeReflector`) is not obtained through `GetReflector()`, but is exposed through `ICustomAttributeReflectorProvider.CustomAttributeReflectors`.

## Method Invocation: MethodReflector

The usage is similar to `System.Reflection.MethodInfo`, but the invocation goes through an IL-compiled accessor:

```csharp
using AspectCore.Extensions.Reflection;

var method = typeof(MethodFakes).GetMethod("GetString");
var reflector = method.GetReflector();
var result = reflector.Invoke(new MethodFakes(), "lemon");   // -> "lemon"
```

The signature of `Invoke` is `object Invoke(object instance, params object[] parameters)`; static methods use `StaticInvoke(params object[] parameters)`.

## Constructor Invocation: ConstructorReflector

```csharp
var constructor = typeof(ConstructorFakes).GetTypeInfo().GetConstructor(new Type[0]);
var reflector = constructor.GetReflector();
var instance = (ConstructorFakes)reflector.Invoke();         // parameterless constructor
```

The signature of `Invoke` is `object Invoke(params object[] args)`.

## Property Access: PropertyReflector

```csharp
var property = typeof(PropertyFakes).GetTypeInfo().GetProperty("InstanceProperty");
var reflector = property.GetReflector();

var value = reflector.GetValue(fakes);         // read
reflector.SetValue(fakes, "new value");        // write
```

`GetValue(object instance)` / `SetValue(object instance, object value)`; static properties use `GetStaticValue()` / `SetStaticValue(object value)`.

## Field Access: FieldReflector

```csharp
var field = typeof(FieldFakes).GetTypeInfo().GetField("InstanceField");
var reflector = field.GetReflector();

var value = reflector.GetValue(fakes);         // read
reflector.SetValue(fakes, "new value");        // write
```

The signatures are the same as `PropertyReflector`: `GetValue` / `SetValue`, with static fields using `GetStaticValue()` / `SetStaticValue(object value)`.

## Caching Mechanism

Constructing a reflector has a cost (it must IL-compile the accessor), so `GetReflector()` internally caches per member through a static `ConcurrentDictionary` (`ReflectorCacheUtils<TMemberInfo, TReflector>`): repeatedly calling `GetReflector()` on the same `MemberInfo` compiles the accessor only once, after which the same reflector instance is reused. Therefore, on hot paths there is no need to cache the reflector yourself—just call it directly.

## Performance Magnitude

Official benchmarks (a historical environment, illustrative of magnitude only) show that a reflector delivers an improvement of about two orders of magnitude over native reflection, in the same magnitude as hard-coded calls. Taking method invocation as an example:

| Method | Mean |
|--------|------|
| Native_Call | 1.05 ns |
| Reflection_Call | 91.95 ns |
| Reflector_Call | 7.15 ns |

The optimization for obtaining attributes is especially pronounced. These numbers come from an old BenchmarkDotNet run, used to illustrate the difference in magnitude; actual behavior varies with the runtime environment.

## When to Use It

- When you need to perform reflection invocations frequently at runtime (such as serialization, mapping, dynamic dispatch) and care about performance.
- When you want "an API similar to native reflection" but do not want to bear the overhead of native reflection.
- You do not need AOP, and can reference this package standalone.

## Next Steps

- [Dependency Injection Integration](./dependency-injection.md) — AspectCore itself also relies on high-performance reflection internally.
- [Module and Package Structure Design](../architecture/module-design.md) — the place of reflection extensions in the overall architecture.
