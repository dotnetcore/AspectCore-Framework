# Source Generator Compile-time Engine

The Source Generator is AspectCore's compile-time proxy engine, based on a Roslyn incremental generator (`IIncrementalGenerator`) that generates C# proxy source code for annotated types **at compile time**, so that the runtime directly uses these compiled proxy types without `Reflection.Emit`. It shares the same set of interception semantics as the [DynamicProxy Runtime Engine](./dynamic-proxy.md); for a comparison of the two, see [Engine Comparison and Selection](./engine-comparison.md).

The code is located in `src/AspectCore.SourceGenerator/`. This project is a `netstandard2.0` Roslyn analyzer (`IsRoslynComponent`, `LangVersion=latest`) with no project references, and its DLL is packaged into `analyzers/dotnet/cs`; it depends on `Microsoft.CodeAnalysis.CSharp` and `Microsoft.CodeAnalysis.Analyzers`.

## 1. How It Is Triggered

Generation is triggered by the `[AspectCore.DynamicProxy.AspectCoreGenerateProxy]` attribute (`AspectCoreProxyGenerator.cs:13`), supporting three placements:

- **Type level**: applied to a concrete class or interface. An interface must specify its implementation type (`[AspectCoreGenerateProxy(typeof(Impl))]`).
- **Assembly level (current compilation)**: `[assembly: AspectCoreGenerateProxy]` automatically discovers qualifying types within this assembly.
- **Assembly level (referenced assembly)**: if a referenced assembly is annotated at the assembly level, its qualifying types are included.

## 2. Incremental Generation Flow

In `Initialize`, `AspectCoreProxyGenerator` (`AspectCoreProxyGenerator.cs:10`) hooks up two candidate sources and merges them (`:15`):

1. **Syntax fast path**: `CreateSyntaxProvider` uses a predicate to match "type declarations with an attribute list", and then `GetCandidate` confirms via the symbol's fully qualified attribute name (`:125`).
2. **Referenced assembly discovery**: `CompilationProvider.SelectMany(GetReferencedAssemblyCandidates)` (`:44`).

After the two paths are merged, `RegisterSourceOutput` executes `Execute` (`:151`): for each candidate, it validates, decides interface/class proxy, and calls `ProxyEmitter` to generate `{ProxyTypeName}.g.cs`; as long as anything is generated, it additionally generates `AspectCoreSourceGeneratedProxyRegistry.g.cs`.

### Candidate Filtering

- `IsProxyableClassMethod` (`:460`): `Ordinary && !static && virtual && !sealed`, accessibility ∈ {public, protected, protected internal}, and not a record synthesized member.
- `IsProxyableClassProperty` (`:470`): same as above (property version).
- Auto-discovery (assembly level) additionally skips types containing event members, as well as types that are already explicitly annotated.

## 3. Diagnostics (ACSGxxx)

The generator reports diagnostics when it encounters unsupported situations (`Emit/GeneratorDiagnostics.cs`, category `AspectCore.SourceGenerator`):

| ID | Level | Meaning |
|----|------|------|
| ACSG002 | Warning | Nested types are not supported |
| ACSG003 | Warning | Event members are not supported |
| ACSG005 | Error | Cannot proxy a `sealed` type |
| ACSG006 | Error | The type is not visible to the generated code (requires public/internal) |
| ACSG007 | Error | A class proxy lacks an accessible constructor |
| ACSG008 | Error | Cannot proxy a `ref struct` |
| ACSG009 | Warning | A byref-like `params` parameter is not supported |

(ACSG001/ACSG004 are historically retained descriptors for "open generic type/method not supported"; generics are now supported and these are no longer actively triggered. The diagnostic titles/messages are in Chinese.)

## 4. Proxy Source Code Generation (ProxyEmitter)

`Emit/ProxyEmitter.cs` has two entry points: `EmitInterfaceProxy` (`:14`) and `EmitClassProxy` (`:112`). The generated proxy is a `sealed` type, marked with `[NonAspect]` + `[Dynamically]`, and its fields include `_activatorFactory`, `_aspectContextFactory`, `_aspectBuilderFactory`, `_aspectConfiguration`, `_serviceProvider`, `_implementation`, `_validator`, and `_cachedActivator` (used only on the async path).

- **Targetless interface proxy**: additionally generates an internal `{proxy}__Stub` implementing the interface, and provides two constructors (with and without a target).
- **Class proxy**: forwards the real base class constructors via `EmitClassConstructors` (skipping record copy constructors).
- **record**: generated as a `sealed record class` (letting the compiler synthesize the copy constructor / `with` support); non-records use `sealed class`. For the difference between the two engines on records, see [Record Type Support](./record-support.md).
- **`__Meta` reflection cache**: each proxy embeds a `private static class __Meta` caching the `MethodInfo` of `Service_*`/`Impl_*`/`Proxy_*`, and marked with trimming/AOT suppression attributes.

### Return Type Dispatch (ReturnKindKind)

`ReturnKind.Determine` (`ProxyEmitter.cs:1515`) maps to `ReturnKindKind` (`Void`/`Sync`/`Task`/`TaskOfT`/`ValueTask`/`ValueTaskOfT`/`AsyncEnumerable`/`RefSync`), with semantics aligned to DynamicProxy's `ReturnKind`.

### Inlined Activation (Performance-critical)

The method body generated by `EmitProxyInvokeBody` (`:858`):

1. Takes the cached service/implementation/proxy methods; re-resolves the implementation method by the runtime instance signature when necessary.
2. `if (!ShouldIntercept(...))` → **directly calls** the target (`ref`/`ref readonly` retains the `ref` prefix, preserving true aliasing).
3. Constructs `object[] __args` and `AspectActivatorContext`.
4. **The synchronous path fully inlines activation**: directly `CreateContext` → `GetBuilder` to take the cached pipeline → `Build()` → execute, propagating failures via `ExceptionDispatchInfo` and running incomplete tasks with `NoSyncContextScope.Run` — **skipping the `AspectActivator` allocation**.
5. **The asynchronous path**: reuses `_cachedActivator` (`AspectActivator` is stateless and reusable), calling `InvokeTask<T>`/`InvokeValueTask<T>`/`InvokeAsyncEnumerable<T>`.
6. `ref`/`out` parameters are written back from `__args[i]` after the pipeline.
7. **`ref`/`ref readonly` returns**: the value-semantic pipeline result is first stored into a `StrongBox<T>`, then `return ref __refBox.Value;` (for details, see [C# Language Feature Adaptation](./language-features.md)).

### Interface Stubs

`EmitInterfaceStubMembers`/`EmitStubMethod` (`:256`) generate minimal members for the interface and its inherited interfaces: stubs are generated only for abstract methods (default interface methods are omitted so they go through DIM); `out` is set to default, and non-void returns `default(T)`; a non-generic stub with a `ref` return returns a `ref` pointing to a `private static` slot, while a generic `ref` return stub throws `NotSupportedException`.

## 5. Runtime Discovery (RegistryEmitter)

`Emit/RegistryEmitter.cs` generates `AspectCoreSourceGeneratedProxyRegistry.g.cs`:

- An assembly attribute `[assembly: AspectCoreSourceGeneratedProxyRegistryAttribute(typeof(AspectCoreSourceGeneratedProxyRegistry))]` (`:17`), for the runtime to scan and discover.
- A `public sealed class AspectCoreSourceGeneratedProxyRegistry : ISourceGeneratedProxyRegistry`, implementing `TryGetProxyType(serviceType, implementationType, kind, out proxyType)` (`:27`), which internally matches entry by entry by kind + service key (generics normalized to their open definition).

On the runtime side, once enabled via `ProxyEngineOptions`, `SourceGeneratedProxyTypeGenerator` (in `AspectCore.Core`) has `ScanRegistries` reflectively scan the registry attributes on the assemblies in the `AppDomain` and instantiate them, thereby looking up the table to obtain the proxy types generated at compile time. For engine enabling and selection, see [Engine Comparison and Selection](./engine-comparison.md).

## 6. Applicability and Limitations

- **Reduces the dependency on dynamic code during proxy generation**: proxies are generated at compile time, and at runtime **proxy generation** does not need `Reflection.Emit`; combined with manual registry registration (`AddSourceGeneratedProxyRegistry<T>()`), it can work in scenarios without assembly scanning. Note that this is not equivalent to end-to-end NativeAOT — during interception the target call still goes through `MethodReflector` (`DynamicMethod`), and proxy construction also retains `[RequiresDynamicCode]` paths; for the boundary, see [Engine Comparison and Selection](./engine-comparison.md).
- **Requires explicit annotation**: only types with `[AspectCoreGenerateProxy]` (or assembly-level auto-discovery) will have proxies generated.
- **Not supported**: `sealed` classes, `ref struct`, nested types, event members, and byref-like `params` parameters (corresponding to the diagnostics in the table above).
- The behavioral differences from DynamicProxy are concentrated on records (equality / `init` setters) and the boundary of targetless interface stubs, both explained in the corresponding documents.
