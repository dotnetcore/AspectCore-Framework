# Engine Comparison and Selection

AspectCore provides two equivalent proxy generation engines: the runtime [DynamicProxy](./dynamic-proxy.md) and the compile-time [Source Generator](./source-generator.md). The two share the same set of interception semantics (the same `IAspectActivator`/`AspectContext`/interceptor pipeline), so their **interception behavior is consistent**; the difference lies only in "when and how" the proxy is generated. This document helps you make a selection, and explains the behavior of the three switches in `ProxyEngineOptions`.

## 1. Comparison at a Glance

| Dimension | DynamicProxy | Source Generator |
|------|-------------|------------------|
| Proxy generation timing | Runtime | Compile time |
| Generation approach | `System.Reflection.Emit` emitting IL | Roslyn generating C# source code |
| Trigger condition | Service is managed by the container / created via `ProxyGenerator` | Type is marked with `[AspectCoreGenerateProxy]` (or assembly-level auto-discovery) |
| Requires build changes | No | Requires referencing `AspectCore.SourceGenerator` (analyzer) |
| AOT / trimming | Proxy generation depends on `Reflection.Emit` (marked `[RequiresDynamicCode]`) | Proxy **generation** does not need `Reflection.Emit`; but interception still routes the target call through `MethodReflector` (`DynamicMethod`), so it is not end-to-end NativeAOT |
| First-invocation cost | Runtime generation + pipeline assembly | Pipeline assembly only (the synchronous path also inlines activation, saving the `AspectActivator` allocation) |
| record equality | Reference equality (generates a plain class) | Value equality (generates a `record class`) |
| Enabled by default | Yes | No (explicit opt-in) |

## 2. Engine Selection (ProxyEngine)

Select via `ProxyEngineOptions.Engine` (`AspectCore.Abstractions/DynamicProxy/ProxyEngine.cs:6`):

- `ProxyEngine.DynamicProxy` (default): always generates at runtime.
- `ProxyEngine.SourceGenerator`: uses only the compile-time-generated artifacts, and errors when they are missing.
- `ProxyEngine.Auto`: prefers SG, and falls back to DynamicProxy per policy when missing (suitable for gradual migration).

**The default behavior is unchanged**: without explicit configuration, even if `AspectCore.SourceGenerator` is referenced, `DynamicProxy` is still used. The optional engine logic is enabled only by calling `ConfigureDynamicProxyEngine(...)`.

## 3. Behavior of the Three Switches

`ProxyEngineOptions` provides `Engine`, `AllowRuntimeFallback` (nullable bool), and `Strict`:

```csharp
services.ConfigureDynamicProxyEngine(o =>
{
    o.Engine = ProxyEngine.Auto;
    o.AllowRuntimeFallback = true; // whether to fall back to DynamicProxy when the generated proxy is missing
    o.Strict = false;              // strict mode: throw when the generated proxy is missing
});
```

Per the current implementation (`SourceGeneratedProxyTypeGenerator.CreateCore`, `AspectCore.Core/DynamicProxy/SourceGeneratedProxyTypeGenerator.cs:72`):

- `Engine = DynamicProxy`: always runtime DynamicProxy.
- `Engine = SourceGenerator`: uses only SG artifacts; **throws when missing** (does not fall back even if `AllowRuntimeFallback=true`).
- `Engine = Auto`: prefers SG; when missing, **allows by default** falling back to DynamicProxy.
  - Explicit `AllowRuntimeFallback=false` → throws when missing.
  - `Strict=true` → throws when missing (commonly used for a CI hard constraint that "everything that should be generated was generated").

The decision for the fallback switch: an explicit `AllowRuntimeFallback` takes precedence, otherwise only `Auto` defaults to `true` (`GetAllowRuntimeFallback`, `:115`).

## 4. Three Things to Enable the Source Generator

All must be satisfied at once:

1. **Reference the analyzer**: reference `AspectCore.SourceGenerator` as an analyzer (NuGet `PrivateAssets="all"`, or `ProjectReference` + `OutputItemType="Analyzer"`).
2. **Trigger generation**: mark the type with `[AspectCoreGenerateProxy]` (an interface must provide an implementation type).
3. **Select the engine at runtime**: `ConfigureDynamicProxyEngine(o => o.Engine = ProxyEngine.SourceGenerator | Auto)`.

Both MS.DI and ServiceContext support `ConfigureDynamicProxyEngine`; this API is provided consistently across the `Extensions.DependencyInjection`, `Extensions.Autofac`, and `Extensions.LightInject` adapters.

## 5. Manual Registry under AOT / Trimming

Runtime discovery of SG artifacts relies on two steps: scanning the assemblies for `[AspectCoreSourceGeneratedProxyRegistryAttribute]`, and then constructing the registry instance via a parameterless reflective constructor. Under trimming/AOT, scanning and reflection may be unavailable, causing "it was generated but the runtime can't find it". In that case, register manually:

```csharp
// MS.DI (generic)
services.AddSourceGeneratedProxyRegistry<AspectCore.SourceGenerated.AspectCoreSourceGeneratedProxyRegistry>();

// ServiceContext (instance)
serviceContext.AddSourceGeneratedProxyRegistry(new AspectCore.SourceGenerated.AspectCoreSourceGeneratedProxyRegistry());
```

> The registry type name/namespace depends on the project's actual generation result; the current generator by default outputs `AspectCore.SourceGenerated.AspectCoreSourceGeneratedProxyRegistry`.

> **Important: the NativeAOT boundary**: manual registry registration only solves the "runtime discovery of proxy types" problem; it **does not** mean end-to-end NativeAOT is supported:
> - Proxy **construction** still retains reflective paths (the relevant APIs are marked `[RequiresDynamicCode]`).
> - During interception, the target method call goes through `RuntimeAspectContext.Complete()` → `MethodReflector`, whose construction creates a `DynamicMethod` (`AspectCore.Extensions.Reflection/MethodReflector.cs:26`).
>
> Therefore the value of the Source Generator is to **reduce the dependency on `Reflection.Emit`/dynamic code during the proxy-generation phase**, rather than being verified NativeAOT support. The repository currently has no NativeAOT publish/run test; if your goal is full NativeAOT, verify it yourself and mind the runtime constraints above.

## 6. Selection Recommendations

- **Gradual migration / local development**: `Auto` (fallback allowed by default), get it running first.
- **CI hard constraint**: `Auto + Strict=true` (or `Auto + AllowRuntimeFallback=false`), to ensure everything that should be generated was generated.
- **Trimming / reduce proxy-generation dynamic code**: `SourceGenerator` + manual registry registration (note that runtime reflection/dynamic-code constraints above still apply, and this is not verified end-to-end NativeAOT).
- **Cannot change the type / do not want to annotate**: stay on the default `DynamicProxy`.

## 7. Consistency Guarantee

Any change affecting interception semantics must cover both engines, and be guarded by the dual-engine consistency tests under `tests/AspectCore.Core.Tests/EngineParity/`. The known behavioral differences (record equality, `init` setters, and the `ref` return boundary of targetless interface stubs) are explained in [Record Type Support](./record-support.md) and [C# Language Feature Adaptation](./language-features.md).
