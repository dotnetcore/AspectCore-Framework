# Overall Architecture

This document explains AspectCore's overall architecture, its layering relationships, and its two core execution flows (proxy generation and runtime interception). For terminology, see [Core Concepts](../getting-started/concepts.md); for package-level responsibilities, see [Module and Package Structure Design](./module-design.md).

## 1. Positioning

AspectCore is an AOP framework whose core capability is **weaving interceptors into service methods**. It provides two equivalent proxy generation engines:

| Engine | Weaving timing | Proxy generation approach | Key package |
|------|---------|-------------|--------|
| **DynamicProxy** | Runtime | Dynamically generates IL via `System.Reflection.Emit` | `AspectCore.Core` |
| **Source Generator** | Compile time | Generates C# source code via Roslyn | `AspectCore.SourceGenerator` |

Both engines share the same set of abstract contracts (`AspectCore.Abstractions`) and the same interceptor pipeline semantics, so from the user's perspective the interception behavior is consistent. For engine selection, see [Engine Comparison and Selection](./engine-comparison.md).

## 2. Layering

AspectCore's dependency direction flows from bottom to top, unidirectional and acyclic:

```
                 ┌─────────────────────────────────────────────┐
  Integration/feature     │ DI adapters: DependencyInjection / Autofac /      │
                 │          Windsor / LightInject / Hosting      │
                 │ Web:     AspNetCore                            │
                 │ Features:    AspectScope / DataValidation /        │
                 │          DataAnnotations / Configuration       │
                 └───────────────────┬─────────────────────────┘
                                     │ depends on
                 ┌───────────────────▼─────────────────────────┐
  Runtime core layer    │ AspectCore.Core                              │
                 │  - DynamicProxy runtime engine (IL emit)         │
                 │  - IoC container (ServiceContext/ServiceResolver)│
                 │  - interceptor pipeline & configuration                          │
                 └──────────┬───────────────────┬──────────────┘
                            │ depends on               │ depends on
       ┌────────────────────▼──────┐   ┌────────▼───────────────────┐
  Foundation │ AspectCore.Abstractions   │   │ AspectCore.Extensions.       │
        │ (contracts: interfaces/abstracts/attributes)  │   │ Reflection (high-perf reflection)     │
        └───────────────────────────┘   └──────────────────────────────┘

  Compile-time engine (standalone): AspectCore.SourceGenerator（Roslyn analyzer, no project deps）
```

- **Foundation layer**: `Abstractions` contains only contracts (interfaces, abstract classes, attributes, enums), with no implementation; `Extensions.Reflection` is a standalone high-performance reflection library that does not depend on other AspectCore packages, and is instead depended upon by `Core`.
- **Runtime core layer**: `Core` depends on `Abstractions` + `Reflection`, and implements the DynamicProxy runtime engine, the IoC container, and the interceptor pipeline.
- **Integration/feature layer**: all DI adapters and feature extensions depend on `Core` (a few depend only on `Abstractions`).
- **Compile-time engine**: `SourceGenerator` is a standalone Roslyn analyzer (`netstandard2.0`) with no project references; the proxy code it generates references types from `Core`/`Abstractions` at runtime.

## 3. Two Core Facts

To understand the AspectCore architecture, grasp two key facts:

### Fact 1: "Whether to intercept" is decided at proxy generation time; "interceptor selection/ordering" is decided at first invocation

- **Proxy generation phase**: `MethodBodyFactory.DecideBody` (`src/AspectCore.Core/DynamicProxy/ProxyBuilder/Builders/MethodBodyFactory.cs:15`) decides, for each method, whether to generate a "direct delegation" or an "aspect activation" method body, based on `IAspectValidator.Validate`.
- **First runtime invocation**: `InterceptorCollector` (`src/AspectCore.Core/DynamicProxy/InterceptorCollector.cs:12`) collects, orders, and deduplicates interceptors, and `AspectBuilderFactory` (`src/AspectCore.Core/DynamicProxy/AspectBuilderFactory.cs:7`) builds and caches the interceptor pipeline.

This split means: methods not determined to require interception are plain forwards in the proxy, with zero runtime overhead; methods that require interception only assemble the pipeline on first invocation, after which they go through the cache.

### Fact 2: The same `IAspectValidator` validation chain serves two places

`IAspectValidator` is used both at proxy generation time (deciding the method body type) and at container registration time (`ServiceValidator` in `ServiceTable` decides whether a service needs to be wrapped as a proxy). The validation chain is an ordered chain of responsibility (`AspectValidatorBuilder`, `src/AspectCore.Core/DynamicProxy/AspectValidatorBuilder.cs:9`).

## 4. Execution Flow 1: Proxy Type Generation (DynamicProxy)

```
IProxyTypeGenerator.CreateClassProxyType / CreateInterfaceProxyType
  → ProxyTypeCompiler（single ModuleBuilder, caches types by name）
    → build AST: ClassProxyBuilder / InterfaceProxyBuilder / InterfaceImplBuilder
        produces ProxyTypeNode (field/ctor/method/property node tree)
        each method picks a body node via MethodBodyFactory.DecideBody:
          - DirectDelegationBody / ReflectorDelegationBody(not intercepted)
          - AspectActivatorBody(intercepted)
          - StubBody / RecordCloneBody / BackingFieldGet|SetBody
    → ILEmitVisitor walks the AST and emits IL via ILGenerator
    → typeBuilder.CreateTypeInfo() yields the proxy Type
```

Key files: `ProxyTypeGenerator.cs:11`, `ProxyBuilder/ProxyTypeCompiler.cs:14`, `ProxyBuilder/Visitors/ILEmitVisitor.cs`. For details, see [DynamicProxy Runtime Engine](./dynamic-proxy.md).

## 5. Execution Flow 2: Runtime Interception

When a proxy method body (the IL generated by `AspectActivatorBody`) is invoked:

```
proxy method is invoked
  → build AspectActivatorContext (service/target/proxy method, parameters, ...)
  → IAspectActivatorFactory.Create() obtain IAspectActivator
  → dispatch by return type (ReturnKind):
      Invoke<T> / InvokeTask<T> / InvokeValueTask<T> / InvokeAsyncEnumerable<T>
  → AspectActivator：
      IAspectContextFactory.CreateContext(...)   // create RuntimeAspectContext
      IAspectBuilderFactory.Create(ctx).Build()  // assemble and cache the interceptor pipeline
      run pipeline: interceptor₁ → interceptor₂ → … → ctx.Complete()
        Complete() invokes the real target via MethodReflector, awaits async results, writes back ReturnValue
      take context.ReturnValue as the return; ref returns are carried by StrongBox<T>
  → finally: ReleaseContext
```

Key files: `AspectActivator.cs:26`, `AspectContext.Runtime.cs:80`, `AspectBuilder.cs:8`. The pipeline is a standard middleware-style chain: each interceptor receives the `AspectContext` and a `next` delegate, and can insert logic before and after calling `next`, or short-circuit by not calling `next` (`ctx.Break()`).

## 6. Where the Source Generator Engine Fits

At **compile time**, the Source Generator scans types marked with `[AspectCoreGenerateProxy]` and directly generates C# proxy source code (`ProxyEmitter`), and also generates an `ISourceGeneratedProxyRegistry` for runtime discovery. At runtime, `SourceGeneratedProxyTypeGenerator` looks up the table to obtain the proxy types already generated at compile time, skipping `Reflection.Emit`. It reuses the same runtime `IAspectActivator`/`IAspectContextFactory`/`IAspectBuilderFactory` semantics, so the interception behavior is consistent with DynamicProxy. For details, see [Source Generator Compile-time Engine](./source-generator.md).

## 7. Further Reading

- [Module and Package Structure Design](./module-design.md): explains the responsibilities and dependencies of the 14 packages layer by layer.
- [Engine Comparison and Selection](./engine-comparison.md): when to use which engine, and the behavior of `Strict`/`AllowRuntimeFallback`/`Auto`.
- [C# Language Feature Adaptation](./language-features.md): whether each C# version's features need adaptation in AOP Emit.
