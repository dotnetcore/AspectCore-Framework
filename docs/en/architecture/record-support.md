# C# 9 Record Type Proxy Support

> This document explains the behavioral differences and design trade-offs of AspectCore's two AOP engines when proxying C# 9 `record` types.

## 1. Overview

AspectCore has two AOP engines:

| Engine | IL generation approach | Key files |
|------|------------|---------|
| **DynamicProxy** (runtime) | `System.Reflection.Emit` + AST architecture | `ILEmitVisitor.cs`, `ClassProxyAstBuilder.cs` |
| **Source Generator** (compile time) | Roslyn generating C# source code strings | `ProxyEmitter.cs`, `AspectCoreProxyGenerator.cs` |

Both engines support generating class proxies for C# 9 `record` types, and support the `with` expression (the immutable copy semantics of a record). However, because the two engines have different generation mechanisms, there are behavioral differences in the following two areas.

## 2. Equality Semantics Difference

### 2.1 Symptom

- **Source Generator**: the proxy class is generated as a `record class` (inheriting from the service record type). The C# compiler automatically synthesizes value-based `Equals`/`GetHashCode`/`ToString`.
- **DynamicProxy (IL emit)**: the proxy class is generated as a plain `class` (not a record), using reference equality.

Therefore, for two proxy instances `proxyA` and `proxyB` of the same record service type:

```csharp
// Source Generator engine: value equality
proxyA.Equals(proxyB)  // true (when all property values are equal)

// DynamicProxy engine: reference equality
proxyA.Equals(proxyB)  // false (always reference comparison)
```

### 2.2 Design Trade-off

Generating the proxy as a `record class` in the Source Generator is necessary: only then will the C# compiler synthesize the copy constructor and `with` support, so that the `with` expression works correctly. Its side effect is that the compiler also generates value-based `Equals`/`GetHashCode`/`ToString`.

The DynamicProxy engine directly generates a plain class via IL emit, and the record's `with` support is achieved by manually implementing the `<Clone>$`/`<>Copy` methods (using `MemberwiseClone`), without relying on compiler synthesis, so the proxy class retains reference equality semantics.

### 2.3 Recommendation

- If business code relies on equality comparison of proxy instances, be aware of the difference between the two engines.
- It is recommended to avoid directly comparing the equality of proxy instances in record proxy scenarios, or to clearly understand the current engine's behavior before doing so.
- If unified behavior is needed, you may consider explicitly overriding `Equals`/`GetHashCode` in the Source Generator's generated proxy to exclude infrastructure fields such as `_activatorFactory`, but this would increase the complexity of the generated code, and this approach is not adopted in the current version.

## 3. init-only Property Setter Difference (IL emit init setter)

### 3.1 Symptom

Properties of a record type typically use the `init` accessor (init-only setter) to guarantee immutable semantics.

- **Source Generator**: the generated proxy properties retain the `init` accessor (the `init` keyword is written directly into the generated source code).
- **DynamicProxy (IL emit)**: the generated proxy property setter is a plain `set` (rather than `init`).

### 3.2 Cause

When the DynamicProxy engine copies property attributes, `AttributeNodeFactory.cs` includes `System.Runtime.CompilerServices.IsExternalInitAttribute` in the skip list. The IL representation of an `init` accessor relies on the `modreq([IsExternalInit])` modifier, and after skipping that attribute, the proxy property's setter changes from `init` to `set`.

### 3.3 Impact

- A `with` expression generates its call code at compile time based on the static type, and does not enforce a check of `modreq([IsExternalInit])` at runtime, so `with` still works normally.
- However, the proxy property's setter changing from `init` to `set` means external code can directly modify the proxy property, bypassing the record's immutable semantics.

### 3.4 Recommendation

- If business code relies on the init-only immutable semantics of record proxy properties, be aware of the difference under the DynamicProxy engine where the setter becomes `set`.
- Under the Source Generator engine the init semantics remain unchanged.
- The current version chooses to explain this difference in the documentation, rather than retaining `modreq([IsExternalInit])` in the IL emit, because the latter would require additional handling of the required modifier beyond `DefineProperty`/`SetSetMethod`, increasing the complexity of IL emit, and the `with` expression functionality is unaffected.

## 4. Override Method Proxying

### 4.1 Symptom

For `override` methods declared in a derived record type (for example, overriding the base class's `Label()` method), the proxy behavior of the two engines:

- **Source Generator**: correctly generates the proxy for `override` methods after the fix.
- **DynamicProxy (IL emit)**: correctly generates the proxy for `override` methods.

### 4.2 Cause

In Roslyn's `IMethodSymbol` API, an `override` method's `IsVirtual` property is `false` and its `IsOverride` property is `true`. The Source Generator's `IsOverridable` method previously only checked `IsVirtual`, which caused `override` methods to be incorrectly filtered out and no proxy generated.

The fix: `IsOverridable` checks both `IsVirtual` and `IsOverride`, ensuring `override` methods are correctly recognized as proxyable methods.

### 4.3 Impact

After the fix, `override` methods in a derived record (such as `Label()`) will be correctly proxied, and interceptors can work normally.

## 5. The `<Clone>$` Method of a Derived Record (DynamicProxy Limitation)

### 5.1 Symptom

For a derived record type (for example, `DerivedRecordService : RecordClassService`), the DynamicProxy engine may throw a `TypeLoadException` when generating the proxy:

```
Return type in method '...<Clone>$()' on type '...' is not compatible with base type method '...<Clone>$()'
```

### 5.2 Cause

A derived record's `<Clone>$` method uses a covariant return type, returning the derived type rather than the base type. When DynamicProxy performs `DefineMethodOverride`, the runtime needs to verify the compatibility of the return type. For dynamically generated types, the runtime's handling of covariant return types is limited, resulting in a `TypeLoadException`.

### 5.3 Impact

- The DynamicProxy engine currently does not support generating class proxies for derived record types (involving the covariant return type of the `<Clone>$` method).
- The Source Generator engine is not affected by this limitation, and can correctly proxy derived record types.

### 5.4 Recommendation

- If you need to proxy a derived record type, it is recommended to use the Source Generator engine.
- A solution for this limitation of the DynamicProxy engine will be researched in a future version.

## 6. Related Files

| File | Description |
|------|------|
| `src/AspectCore.SourceGenerator/RecordTypeUtils.cs` | Record type detection helper methods (`IsRecord`, `IsRecordCopyMethod`, `IsRecordSynthesizedMember`) |
| `src/AspectCore.SourceGenerator/AspectCoreProxyGenerator.cs` | Source Generator candidate type discovery and filtering |
| `src/AspectCore.SourceGenerator/Emit/ProxyEmitter.cs` | Source Generator proxy code generation (including the `IsOverridable` override fix) |
| `src/AspectCore.Core/DynamicProxy/ProxyBuilder/Builders/ClassProxyAstBuilder.cs` | DynamicProxy record copy method AST construction (including the derived-record `<Clone>$` skip logic) |
| `src/AspectCore.Core/DynamicProxy/ProxyBuilder/Visitors/ILEmitVisitor.cs` | DynamicProxy IL emission (including `VisitRecordCloneBody`) |
| `src/AspectCore.Core/DynamicProxy/ProxyBuilder/Builders/AttributeNodeFactory.cs` | Property attribute filtering (including the `IsExternalInit` skip logic) |
| `src/AspectCore.Core/Utils/ReflectionUtils.cs` | The `IsRecordCopyMethod` extension method (DynamicProxy side) |
| `tests/AspectCore.Core.Tests/EngineParity/SourceGeneratorDynamicProxyParityTests.cs` | Record proxy tests (covering base class, derived, generic, and init-only property scenarios) |
