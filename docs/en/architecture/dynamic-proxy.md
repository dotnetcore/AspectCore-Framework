# DynamicProxy Runtime Engine

DynamicProxy is AspectCore's runtime proxy engine, generating the IL of proxy types dynamically at runtime based on `System.Reflection.Emit`. This document explains its two major processes: **proxy type generation** and **runtime interception**. For overall positioning, see [Overall Architecture](./overview.md); for a comparison with the compile-time engine, see [Engine Comparison and Selection](./engine-comparison.md).

The code is located in `src/AspectCore.Core/DynamicProxy/`.

## 1. Proxy Type Generation

### 1.1 Entry Point and Compiler

`IProxyTypeGenerator` is the type factory, providing three methods: `CreateInterfaceProxyType(Type)` (no target), `CreateInterfaceProxyType(Type, Type)` (with target), and `CreateClassProxyType(Type, Type)`. The default implementation `ProxyTypeGenerator` (`ProxyTypeGenerator.cs:11`) uses `IAspectValidatorBuilder.Build()` at construction time to obtain the validator, and creates a `ProxyTypeCompiler`; it first performs entry validation (rejecting `ref struct`, requiring an interface/class form) before delegating to the compiler.

`ProxyTypeCompiler` (`ProxyBuilder/ProxyTypeCompiler.cs:14`) holds a **single** `ModuleBuilder` (assembly `AspectCore.DynamicProxy.Generator`, namespace `AspectCore.DynamicGenerated`, `AssemblyBuilderAccess.RunAndCollect`), and caches generation results with a lock keyed by type name to avoid duplicate generation. Three generation strategies:

- Targetless interface proxy: two phases — first build the stub implementation nodes and emit the stub type, then build the proxy nodes and emit.
- Interface proxy with target: goes through `InterfaceProxyBuilder`.
- Class proxy: goes through `ClassProxyBuilder`.

### 1.2 AST Construction

The proxy is not written directly as IL; instead, a **proxy type AST** is built first (`ProxyTypeNode`, containing field/constructor/method/property nodes), and then a visitor emits IL. The AST builders implement `IProxyTypeBuilder` (`ProxyBuilder/Builders/IProxyAstBuilder.cs:5`):

- **`ClassProxyBuilder`** (`Builders/ClassProxyAstBuilder.cs:13`): for class proxies, injects the `_activatorFactory` and `_implementation` fields, with `_implementation = this` (a class proxy wraps itself); marks the proxy type with `[NonAspect]` + `[Dynamically]`; iterates over visible and virtual methods/properties; handles the record's `<Clone>$` copy method and covariant returns.
- **`InterfaceProxyBuilder`** (`Builders/InterfaceProxyAstBuilder.cs:11`): an interface proxy with a target, where the constructor kind is `InterfaceProxyCtorWithFactoryAndTarget`, and `_implementation` points to the interface target instance.
- **`InterfaceImplBuilder`** (`Builders/InterfaceImplAstBuilder.cs:13`): the stub + proxy for a targetless interface proxy, and provides the shared `BuildProxyMethod`/`ResolveImplementationMethod`.

### 1.3 Method Body Decision

Which kind of method body each method generates is decided by `MethodBodyFactory.DecideBody` (`Builders/MethodBodyFactory.cs:15`):

1. Method marked `[NonAspect]` → **delegation body** (direct forward, no interception).
2. Otherwise, `validator.Validate(serviceMethod, strict:true) || validator.Validate(implementationMethod, false)` being true → **aspect activation body** (`AspectActivatorBody`, goes through interception).
3. Everything else → **delegation body**.

Method body node types (`ProxyBuilder/Nodes/MethodBodyNode.cs:6`):

| Node | Meaning |
|------|------|
| `DirectDelegationBody` | Directly calls the target method (when the target type is visible) |
| `ReflectorDelegationBody` | Calls via `MethodReflector` (when the target type is not visible, see issue #274) |
| `AspectActivatorBody` | The method body that goes through the interceptor pipeline |
| `StubBody` | The placeholder implementation for a targetless interface stub |
| `RecordCloneBody` | The record's `<Clone>$`/`<>Copy` copy method |
| `BackingFieldGetBody` / `BackingFieldSetBody` | Read/write of a stub property |

### 1.4 Return Type Dispatch (ReturnKind)

`DetermineReturnKind` (`Builders/MethodBodyFactory.cs:78`) maps the method return type to `ReturnKind` (`ProxyBuilder/ReturnKind.cs:3`):

```
void → Void
ref/ref readonly（IsByRef）→ RefSync
Task → Task            Task<T> → TaskOfT
ValueTask → ValueTask  ValueTask<T> → ValueTaskOfT
IAsyncEnumerable<T> → AsyncEnumerable
others -> Sync
```

### 1.5 IL Emission

`ILEmitVisitor` (`ProxyBuilder/Visitors/ILEmitVisitor.cs`) traverses the AST and emits real IL via `ILGenerator`: it defines the type/fields/constructors/methods/properties (`VisitProxyType:34`), emits the three kinds of constructors (`:113`), and emits method bodies according to the method body node type.

At the core is `VisitAspectActivatorBody` (`:460`): construct `AspectActivatorContext` → load `_activatorFactory` → call `IAspectActivatorFactory.Create()` → dispatch by `ReturnKind` (`EmitReturnValue:644`) → write back `byref` parameters → return. For `ref`/`ref readonly` returns, it wraps the value produced by the pipeline into a `StrongBox<T>` and uses `ldflda` to return the address of its field (because the pipeline is value-semantic, an on-heap store is needed to carry the reference; for details, see [C# Language Feature Adaptation](./language-features.md)).

## 2. Runtime Interception

The execution chain when a proxy method body is invoked:

### 2.1 Activator

`AspectActivator` (`AspectActivator.cs:26`) is created by `AspectActivatorFactory.Create()` on each invocation, and is the runtime entry point directly called by the emitted IL. It provides four methods based on the return form:

- `Invoke<TResult>` (sync/ref) — uses `NoSyncContextScope.Run` for incomplete tasks to avoid potential deadlock; takes `(TResult)context.ReturnValue`.
- `InvokeTask<TResult>` / `InvokeValueTask<TResult>` — after awaiting, converts to the corresponding `Task<T>`/`ValueTask<T>`.
- `InvokeAsyncEnumerable<TResult>` — streaming enumeration, wrapping exceptions item by item.

Exception handling: on failure, uses `ExceptionDispatchInfo` to preserve the original stack; when `IAspectConfiguration.ThrowAspectException` is true, wraps into an `AspectInvocationException`. `ReleaseContext` runs in the `finally`.

### 2.2 Context

`RuntimeAspectContext` (`AspectContext.Runtime.cs:13`) is the runtime implementation of `AspectContext`:

- `Complete()` (`:80`): if there is no target, `Break()`; otherwise takes the cached `MethodReflector` (selecting Callvirt/Call based on `IsCallvirt`), invokes the real target method, `await this.AwaitIfAsync(returnValue)`, and writes to `ReturnValue`.
- `Break()` (`:93`): short-circuits the pipeline, producing a default value for the return type (a `ref` return first takes the element type and then its default value).
- `Invoke(next)` → `next(this)`.

### 2.3 Pipeline Assembly

`AspectBuilderFactory`'s (`AspectBuilderFactory.cs:7`) `Create(context)` → `GetBuilder(serviceMethod, implementationMethod, predicateMethod)`, with the result cached in `IAspectCaching` keyed by the method triple. The pipeline seed is `new AspectBuilder(ctx => ctx.Complete(), null)`, and the collected interceptors are then appended one by one.

`AspectBuilder` (`AspectBuilder.cs:8`) wraps each interceptor as `next => context => interceptor.Invoke(context, next)`, and `Build()` folds from the tail (`Complete`) forward into a single `AspectDelegate` and caches it. This is the standard middleware chain:

```
interceptor₁.Invoke(ctx, next=
  interceptor₂.Invoke(ctx, next=
    …
      ctx.Complete()  // invoke the real target
  ))
```

### 2.4 Interceptor Collection

`InterceptorCollector` (`InterceptorCollector.cs:12`) aggregates interceptors from multiple selectors, then orders them by `Order` (`HandleSort`), deduplicates non-`AllowMultiple` ones (`HandleMultiple`), performs property injection on the interceptors that need it, and caches the result. Selectors:

- `ConfigureInterceptorSelector` (from `IAspectConfiguration.Interceptors`, respecting predicates + `NonAspectPredicates`) — `ConfigureInterceptorSelector.cs:10`
- `AttributeInterceptorSelector` (attribute interceptors on types and methods) — `AttributeInterceptorSelector.cs:8`
- `AttributeAdditionalInterceptorSelector` (implementation side + inheritance chain) — `AttributeAdditionalInterceptorSelector.cs:9`

### 2.5 Async Return Value Unwrapping

`AspectContextRuntimeExtensions` (`Extensions/AspectContextRuntimeExtensions.cs:11`) provides `AwaitIfAsync`/`IsAsync`/`UnwrapAsyncReturnValue`, using compiled and cached expressions to extract the result of `Task<T>`/`ValueTask<T>`; it recognizes `AsyncAspectAttribute`.

## 3. Cooperation with the IoC Container

In a DI scenario, `ServiceTable` (`DependencyInjection/ServiceTable.cs:11`) determines at registration time via `ServiceValidator` whether a service needs a proxy; if so, it calls `IProxyTypeGenerator` to generate the proxy type and wraps it as a `ProxyServiceDefinition`; `ServiceCallSiteResolver` constructs the proxy instance via reflection at resolution time and injects `IAspectActivatorFactory`. Therefore, what the user resolves from the container is an already-woven proxy instance. For IoC details, see [Dependency Injection Integration](../guide/dependency-injection.md).

## 4. Applicability and Limitations

- Generated at runtime, **with no need to change the build process**; any service managed by the container or created via `ProxyGenerator` can be proxied.
- Depends on `Reflection.Emit`, and is limited in fully AOT / trimming scenarios (the relevant APIs are annotated with `[RequiresDynamicCode]`/`[RequiresUnreferencedCode]`); such scenarios should use the [Source Generator engine](./source-generator.md) instead.
- Can only proxy inheritable/overridable members: virtual members of non-sealed classes and interface members; `sealed`, `ref struct`, and static members are not supported.
