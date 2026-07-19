# Module and Package Structure Design

This document systematically explains AspectCore's module division: the responsibility boundary, public entry points, dependency direction, and layering principles of each package. This is the authoritative document for understanding code organization. For in-package implementation details, see the corresponding architecture documents; for usage-facing APIs, see the [Usage Guide](../guide/interceptor.md).

## 1. Layering Principles

AspectCore follows unidirectional dependencies: **contracts at the bottom, implementation in the middle, integration at the top**.

- Contracts (`Abstractions`) do not depend on any implementation, and provide a unified programming surface for the upper layers.
- The reflection library (`Extensions.Reflection`) is fully standalone, can be used on its own, and is depended upon by `Core`.
- The runtime core (`Core`) implements the DynamicProxy engine + the IoC container.
- Integration/feature packages depend only on `Core` or `Abstractions`, without horizontal coupling among themselves (`AspNetCore` and `DataAnnotations` are deliberate composition exceptions).
- The compile-time engine (`SourceGenerator`) is independent of the runtime, and only references fully qualified names of runtime types within its generated code.

Versioning and language level are managed uniformly by `build/common.props` (`LangVersion=10.0`, product version in `build/version.props`); `SourceGenerator` itself overrides this to `LangVersion=latest`.

## 2. Dependency Graph

```
Abstractions ◄──────────────┐
    ▲                        │
    │                        │
Extensions.Reflection ◄──┐   │
    ▲                    │   │
    │                    │   │
   Core ─────────────────┴───┘   （Core → Abstractions + Reflection）
    ▲
    ├── Extensions.DependencyInjection ── (MS.DI)
    ├── Extensions.Autofac ────────────── (Autofac)
    ├── Extensions.Windsor ────────────── (Castle.Windsor)
    ├── Extensions.LightInject ────────── (LightInject)
    ├── Extensions.Hosting ────────────── (MS.Hosting)  -> also depends on DependencyInjection
    ├── Extensions.AspectScope
    ├── Extensions.DataValidation ─────── (depends only on Abstractions + Reflection)
    ├── Extensions.DataAnnotations ────── -> also depends on DataValidation
    ├── Extensions.Configuration ──────── (depends only on Abstractions + Reflection)
    └── Extensions.AspNetCore ─────────── -> combines AspectScope + DataAnnotations + DependencyInjection

SourceGenerator (standalone Roslyn analyzer, no project references)
```

Dependency source: the `ProjectReference` entries in each package's `.csproj`.

## 3. Foundation Layer

### AspectCore.Abstractions — Contracts

Contains only interfaces, abstract classes, attributes, enums, and delegates, with no business implementation. Split into three namespaces:

- **`AspectCore.DynamicProxy` (interception)**:
  - `AspectContext` (the per-invocation context abstraction, containing `ReturnValue`/`Parameters`/`ServiceMethod`, etc., plus `Invoke`/`Complete`/`Break`) — `DynamicProxy/AspectContext.cs:9`
  - `IInterceptor` / `AbstractInterceptor` / `AbstractInterceptorAttribute` / `ServiceInterceptorAttribute` (the four forms of an interceptor) — `DynamicProxy/IInterceptor.cs:6`, `AbstractInterceptorAttribute.cs:11`
  - `AspectDelegate` (the pipeline delegate `delegate Task AspectDelegate(AspectContext)`) — `DynamicProxy/AspectDelegate.cs:5`
  - Validation: `IAspectValidator` / `IAspectValidatorBuilder` / `IAspectValidationHandler` — `DynamicProxy/IAspectValidator.cs:6`
  - Activation: `IAspectActivator` (`Invoke`/`InvokeTask`/`InvokeValueTask`/`InvokeAsyncEnumerable`) / `IAspectActivatorFactory` / `AspectActivatorContext` — `DynamicProxy/IAspectActivator.cs:7`
  - Pipeline: `IAspectBuilder` / `IAspectBuilderFactory` / `IInterceptorCollector` / `IInterceptorSelector` — `DynamicProxy/IAspectBuilderFactory.cs:7`
  - Proxy generation: `IProxyGenerator` / `IProxyTypeGenerator` — `DynamicProxy/IProxyGenerator.cs:6`
  - Engine selection and SG: `ProxyEngine` (enum) / `ProxyEngineOptions` / `ISourceGeneratedProxyRegistry` / `AspectCoreSourceGeneratedProxyRegistryAttribute` / `AspectCoreGenerateProxyAttribute` — `DynamicProxy/ProxyEngine.cs:6`
  - Marker attributes: `[NonAspect]` (opt-out) / `[Dynamically]` (marks a generated proxy type) — `DynamicProxy/NonAspectAttribute.cs:6`
- **`AspectCore.Configuration` (configuration)**: `IAspectConfiguration` (`Interceptors`/`ValidationHandlers`/`NonAspectPredicates`/`ThrowAspectException`), `AspectPredicate` (delegate), `InterceptorCollection`/`InterceptorFactory` — `Configuration/IAspectConfiguration.cs:7`
- **`AspectCore.DependencyInjection` (DI contract)**: `IServiceContext`, `IServiceResolver`, `ServiceDefinition` (and its `Type`/`Instance`/`Delegate` subtypes), `Lifetime` (enum), `IPropertyInjector`, `FromServiceContextAttribute`, etc. — `DependencyInjection/IServiceContext.cs:9`

### AspectCore.Extensions.Reflection — High-performance Reflection

A standalone library that uses `DynamicMethod` + IL emit to compile cached delegates, replacing the slow `System.Reflection` calls. It can be used independently of AOP.

- Unified entry point: the `ReflectorExtensions.GetReflector(...)` family of extension methods, mapping `MethodInfo`/`ConstructorInfo`/`FieldInfo`/`PropertyInfo`/`ParameterInfo`/`Type` to the corresponding reflector — `ReflectorExtensions.cs:6`
- Reflector classes: `MethodReflector` (with `Static`/`Call`/`OpenGeneric` variants), `ConstructorReflector`, `FieldReflector`, `PropertyReflector`, `TypeReflector`, `ParameterReflector`, `CustomAttributeReflector`, all derived from `MemberReflector<T>` — `MethodReflector.cs:10`, `MemberReflector.cs:7`
- Caching: `ReflectorCacheUtils<TMember,TReflector>` uses `ConcurrentDictionary.GetOrAdd` to ensure each reflector is compiled only once — `Internals/ReflectorUtils.cs:8`
- IL helpers: `Emit/ILGeneratorExtensions.cs` (`EmitLoadArg`/`EmitLdRef`/`EmitStRef`/type conversions, etc.)
- Dependencies: modern TFMs have no external dependencies; only `netstandard2.0` needs `System.Threading.Tasks.Extensions`, `System.Reflection.Emit.Lightweight`, and `System.Runtime.CompilerServices.Unsafe`

## 4. Runtime Core Layer

### AspectCore.Core — DynamicProxy Engine + IoC Container

Depends on `Abstractions` + `Extensions.Reflection`. It is the heart of the runtime, internally divided into three parts:

**(a) DynamicProxy runtime engine** (`DynamicProxy/` and `DynamicProxy/ProxyBuilder/`)
- Proxy type generation: `ProxyTypeGenerator` (`ProxyTypeGenerator.cs:11`) → `ProxyTypeCompiler` (single `ModuleBuilder`, cached by name) → AST builders (`ClassProxyBuilder`/`InterfaceProxyBuilder`/`InterfaceImplBuilder`) → `ILEmitVisitor` emits IL
- Runtime interception: `AspectActivator` (`AspectActivator.cs:26`), `RuntimeAspectContext` (`AspectContext.Runtime.cs:13`), `AspectBuilder`/`AspectBuilderFactory` (pipeline assembly and caching), `InterceptorCollector` (collection/ordering/deduplication)
- Return type dispatch: the `ReturnKind` enum (`Void`/`Sync`/`Task`/`TaskOfT`/`ValueTask`/`ValueTaskOfT`/`AsyncEnumerable`/`RefSync`) — `ProxyBuilder/ReturnKind.cs:3`

**(b) IoC container** (`DependencyInjection/`)
- `ServiceContext` (the registration manifest; `AddInternalServices` self-registers the entire DynamicProxy service graph) — `DependencyInjection/ServiceContext.cs:11`
- `ServiceResolver` (resolves by `Lifetime`: Singleton/Scoped caching, Transient direct construction) — `DependencyInjection/ServiceResolver.cs:10`
- `ServiceTable` (the registration index; **decides the proxy engine here** and wraps services needing interception as `ProxyServiceDefinition`) — `DependencyInjection/ServiceTable.cs:11`
- `ServiceCallSiteResolver` / `ConstructorCallSiteResolver` (compiled resolution delegates, constructor selection), `ServiceValidator` (the threshold for whether to proxy)

**(c) Configuration and validation** (`Configuration/` and `DynamicProxy/ValidationHandlers/`)
- `AspectConfiguration` (default configuration), `Predicates` (`ForNameSpace`/`ForService`/`ForMethod`/`Implement` predicate factories) — `Configuration/Predicates.cs:5`
- Validation chain of responsibility: `CacheAspectValidationHandler`(Order -101) → `OverwriteAspectValidationHandler`(Order 1) → `ConfigureAspectValidationHandler`(Order 11) → `AttributeAspectValidationHandler`(Order 13)
- Interceptor registration extensions: `AddTyped`/`AddServiced`/`AddDelegate` — `Configuration/Extensions/InterceptorCollectionExtensions.cs:9`

### AspectCore.SourceGenerator — Compile-time Engine

A standalone Roslyn incremental analyzer (`netstandard2.0`, `IsRoslynComponent`) with no project references. At compile time it scans `[AspectCoreGenerateProxy]` and generates C# proxy source code + `ISourceGeneratedProxyRegistry`.

- `AspectCoreProxyGenerator` (`IIncrementalGenerator`; candidate discovery and validation, emitting `ACSGxxx` diagnostics) — `AspectCoreProxyGenerator.cs:10`
- `Emit/ProxyEmitter.cs` (generates proxy source code, with inlined activation to avoid `AspectActivator` allocation; supports `ref` returns / `ref` properties / interface stubs)
- `Emit/RegistryEmitter.cs` (generates the registry + assembly attribute for runtime discovery)
- `RecordTypeUtils`/`Naming`/`TypeNameExtensions` (record recognition, naming, fully qualified names)
- Dependencies: `Microsoft.CodeAnalysis.CSharp`, `Microsoft.CodeAnalysis.Analyzers`

## 5. Integration Layer (DI Adapters)

These packages weave AspectCore's proxies into different containers; all depend on `Core`. What they have in common: they all provide `ConfigureDynamicProxyEngine(Action<ProxyEngineOptions>)` to switch to the Source Generator engine.

| Package | Target container | Entry API | Key dependency |
|----|---------|---------|---------|
| `Extensions.DependencyInjection` | Microsoft.Extensions.DependencyInjection | `IServiceCollection.ConfigureDynamicProxy(...)`, `BuildServiceContextProvider()`, `DynamicProxyServiceProviderFactory` | `Microsoft.Extensions.DependencyInjection` |
| `Extensions.Autofac` | Autofac | `ContainerBuilder.RegisterDynamicProxy(...)` (weaving via `PipelineBuilding` middleware) | `Autofac [7.0,8.0)` |
| `Extensions.Windsor` | Castle Windsor | `IWindsorContainer.AddAspectCoreFacility(...)` (in Facility form) | `Castle.Windsor 6.0.0` |
| `Extensions.LightInject` | LightInject | `IServiceContainer.RegisterDynamicProxy(...)` (in `Decorate` form) | `LightInject 6.6.4` |
| `Extensions.Hosting` | Generic Host | `IHostBuilder.UseServiceContext()` / `UseDynamicProxy()` / `ConfigureDynamicProxy()` | `Microsoft.Extensions.Hosting` (and depends on `DependencyInjection`) |

Entry points: `ServiceCollectionExtensions.cs:20`, `Autofac/ContainerBuilderExtensions.cs:16`, `Windsor/FacilityExtensions.cs:11`, `LightInject/ContainerBuilderExtensions.cs:32`, `Hosting/HostBuilderExtensions.cs:12`.

## 6. Feature/Extension Layer

| Package | Responsibility | Entry API | Dependency |
|----|------|---------|------|
| `Extensions.AspectScope` | Scoped aspect context and dispatch (scoped aspect) | `IServiceContext.AddAspectScope()` | `Core` |
| `Extensions.DataValidation` | Data validation framework/abstractions + validation interceptor `DataValidationInterceptorAttribute`(Order -999) | Assembled by DataAnnotations | `Abstractions` + `Reflection` + `System.ComponentModel.Annotations` |
| `Extensions.DataAnnotations` | Concrete validation implementation based on `System.ComponentModel.DataAnnotations` | `IServiceContext.AddDataAnnotations(...)` | `Core` + `DataValidation` |
| `Extensions.Configuration` | Binds values from `IConfiguration` to fields of resolved services | `IServiceContext.AddConfigurationInject()` + `[ConfigurationValue]`/`[ConfigurationBinding]` | `Abstractions` + `Reflection` + `Microsoft.Extensions.Configuration.*` |
| `Extensions.AspNetCore` | ASP.NET Core integration: scoped aspects, DataAnnotations validation, ModelState adaptation | `IServiceCollection.AddAspectScope()` / `AddDataAnnotations(...)` | Composes `AspectScope` + `DataAnnotations` + `DependencyInjection` (net6.0+ only) |

Entry points: `AspectScope/ServiceContainerExtensions.cs:9`, `DataAnnotations/ServiceContainerExtensions.cs:10`, `Configuration/ServiceContainerExtensions.cs:8`, `AspNetCore/Extensions/ServiceCollectionExtensions.cs:17`.

## 7. Tests, Samples, Benchmarks (Non-shipping)

- `tests/`: `AspectCore.Core.Tests` (including the `EngineParity/` dual-engine consistency tests), `AspectCore.E2E.Tests`, various container adapter tests, `AspectCore.Extensions.Reflection.Test`, etc. For details, see [Testing Strategy](../testing/testing-strategy.md).
- `sample/`: AspectScope, Autofac, DataAnnotations, and DependencyInjection console samples.
- `benchmark/`, `benchmarks/`: benchmark projects for Core and Reflection.

## 8. Boundaries and Constraints

- **Contracts do not sink into implementation**: new interception/DI abstractions go into `Abstractions`, and implementation goes into `Core`.
- **The reflection library stays standalone**: `Extensions.Reflection` must not reverse-depend on `Core`/`Abstractions`.
- **Feature packages have no horizontal coupling**: new feature packages should depend only on `Core`/`Abstractions`; when composition is needed (such as `AspNetCore`), declare it explicitly.
- **The two engines stay semantically aligned**: any change affecting interception semantics must cover both DynamicProxy and SourceGenerator, and be guarded by the `EngineParity` tests.
