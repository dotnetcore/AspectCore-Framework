# C# Language Feature AOP Emit Adaptation Analysis

> Goal: to survey the language features introduced from C# 6 through the latest version (C# 13), analyze which ones require AspectCore adaptation during the AOP Emit process, and provide adaptation plans by priority.

## 1. Background

AspectCore has two AOP engines:

| Engine | IL generation approach | Key files |
|------|------------|---------|
| **DynamicProxy** (runtime) | `System.Reflection.Emit` + AST architecture | `ILEmitVisitor.cs`, `ClassProxyBuilder.cs`, `MethodBodyFactory.cs` |
| **Source Generator** (compile time) | Roslyn generating C# source code strings | `ProxyEmitter.cs`, `AspectCoreProxyGenerator.cs` |

Current status:

- `LangVersion` is set to `10.0` (core library); the Source Generator uses `latest`
- Supports `net6.0` ~ `net9.0` + `netstandard2.0/2.1`
- The two engines follow different paths in method body generation: DynamicProxy dispatches to the `AspectActivator.Invoke*` family of methods via the `ReturnKind` enum; the Source Generator generates different inline code depending on sync/async

### 1.1 The Core AOP Emit Flow

The Emit process of the runtime DynamicProxy:

1. **Build phase**: `ClassProxyBuilder` / `InterfaceProxyBuilder` / `InterfaceImplBuilder` construct the AST (`ProxyTypeNode`)
2. **Emit phase**: `ILEmitVisitor` traverses the AST and emits IL opcodes via `ILGenerator`
3. **Method body decision**: `MethodBodyFactory.DecideBody()` selects the method body type (direct delegation, reflector delegation, aspect activator, stub)

The Emit process of the compile-time Source Generator:

1. **Discovery phase**: `AspectCoreProxyGenerator` discovers types marked with `[AspectCoreGenerateProxy]`
2. **Generation phase**: `ProxyEmitter` generates C# source code (string concatenation), compiled by the C# compiler

## 2. Already-adapted Features

| C# version | Feature | Adaptation approach | Involved files |
|---------|------|---------|---------|
| C# 7.0 | ValueTuple tuples | Handled as an ordinary generic return value, covered by the `ReturnKind.Sync` path | `ILEmitVisitor.cs:EmitReturnValue` |
| C# 7.0 | ref/out/in parameters | The IL path writes back values via `EmitLdRef`/`EmitStRef`; the SG path emits the `ref`/`out`/`in` keywords | `ILEmitVisitor.cs:374-406,443-464`; `ProxyEmitter.cs:879-901` |
| C# 7.2 | private protected | Handled as an ordinary access modifier | Constructor/method generation logic |
| C# 8.0 | Nullable\<T\> value types | `ILGeneratorExtensions` has a complete `EmitNullableConversion` family of methods | `ILGeneratorExtensions.cs` |
| C# 9.0 | Covariant return types | The most complete modern feature support: dedicated infrastructure such as `GetCovariantReturnMethods()`, `IsCovariantReturnMethod()`, `IsOverriddenByCovariantReturnMethod()`, etc. | `TypeExtensions.cs:96-216`; `MethodInfoExtensions.cs:38-62`; `ClassProxyBuilder.cs:152-289` |
| C# 5+ | async/await (Task/Task\<T\>/ValueTask/ValueTask\<T\>) | The `ReturnKind` enum corresponds to the return type, calling `Invoke<TResult>`/`InvokeTask<T>`/`InvokeValueTask<T>` respectively | `ILEmitVisitor.cs`; `MethodBodyFactory.cs` |
| C# 8.0 | ✅ Async streams (IAsyncEnumerable\<T\>) | Both DynamicProxy and SG return a lazy async stream via `InvokeAsyncEnumerable<T>`; the interceptor chain runs during enumeration, and the `AspectContext` is released after enumeration ends, is canceled, or throws. `IAsyncDisposable.DisposeAsync` is already supported by the existing `ValueTask` return value path | `IAspectActivator.cs`; `AspectActivator.cs`; `ILEmitVisitor.cs`; `ProxyEmitter.cs` |
| C# 2+ | Generics (type level + method level) | `GenericParameterNodeFactory` extracts generic parameters and constraints; IL uses `DefineGenericParameters`; SG forwards the type parameters + `where` constraints | `ILEmitVisitor.DefineGenericParameters`; `ProxyEmitter.cs` |
| C# 8.0 | ✅ Nullable reference types (NRT) | DynamicProxy: `AttributeNodeFactory.SkippedAttributeFullNames` filters out 7 kinds of compiler-generated attributes such as `[NullableContext]`/`[Nullable]`; SG: the `CompilerGeneratedAttributeNames` blacklist does not forward them | `AttributeNodeFactory.cs:14-40`; `ProxyEmitter.cs:1041-1051` |
| C# 11.0 | ✅ Generic attributes | DynamicProxy: `BuildCustomAttribute` adds an `IsGenericType` branch, resolving the constructor from the closed generic type; named property/field lookup adds null-safety | `ILEmitVisitor.cs:626-658` |
| C# 10.0 | ✅ CallerArgumentExpression | DynamicProxy: the proxy method uses the original parameter names, so `[CallerArgumentExpression]` references are correct; SG: added to the `ForwardedAttributeNames` whitelist for forwarding | `ProxyEmitter.cs:1054-1059` |
| C# 12.0 | ✅ Collection expressions (CollectionBuilder) | DynamicProxy: the `[CollectionBuilder]` attribute is automatically copied along with custom attributes; SG: added to the `ForwardedAttributeNames` whitelist for forwarding | `ProxyEmitter.cs:1054-1059` |
| C# 12.0 | ✅ Experimental attribute | DynamicProxy: automatically copied along with custom attributes (correct behavior); SG: added to the `ForwardedAttributeNames` whitelist for forwarding | `ProxyEmitter.cs:1054-1059` |
| C# 13.0 | ✅ Attributes on type parameters | DynamicProxy: `GenericParameterNodeFactory` fully copies generic parameter attributes; SG: `EmitTypeParameterWithAttributes` forwards non-blacklisted attributes | `ProxyEmitter.cs:1192-1196` |
| C# 13.0 | ✅ Method group natural type | No changes needed — pure compiler behavior, not involving IL or attributes | — |
| C# 12.0 | ✅ Primary Constructors | DynamicProxy: `SkippedAttributeFullNames` filters out `[PrimaryConstructorParameters]`; constructor parameters are correctly forwarded via `base()`; SG: `CompilerGeneratedAttributeNames` filters out this attribute, and `EmitClassConstructors` uses `EmitParameterDecl` to forward parameters (including `params`/`ref`/`out`/`in`) | `AttributeNodeFactory.cs:22`; `ProxyEmitter.cs:1050,383-412` |
| C# 13.0 | ✅ params collections (params IEnumerable\<T\>) | DynamicProxy: the `[ParamCollection]` attribute is automatically forwarded along with custom attributes (not in the skip list); SG: `EmitParameterDecl` detects `IsParams` and emits the `params` keyword, from which the compiler synthesizes `ParamCollectionAttribute` (not explicitly forwarded, to avoid CS0674); byref-like params (such as `ReadOnlySpan<T>`) report ACSG009 and are skipped from generation | `ProxyEmitter.cs:893-963` |
| C# 9.0 | ✅ init-only property setters | DynamicProxy: `PropertyNode` adds `RequiredCustomModifiers`/`OptionalCustomModifiers`, and `ILEmitVisitor` passes the custom modifiers via `DefineProperty`/`DefineMethod`/`DefineConstructor` to preserve the init-only modreq metadata; SG: `ProxyEmitter` detects `IsInitOnly` and generates the `init` keyword, using reflection to call directly in non-override scenarios to avoid a compilation restriction | `PropertyNode.cs`; `ILEmitVisitor.cs`; `ProxyEmitter.cs` |
| C# 11.0 | ✅ required members | DynamicProxy: preserves the required metadata via custom modifiers; SG: `ProxyEmitter` detects `IsRequired` and generates the `required` keyword, and `EmitConstructorAttributes` forwards the `[SetsRequiredMembers]` attribute to the proxy constructor | `ProxyEmitter.cs`; `ILEmitVisitor.cs` |
| C# 13.0 | ✅ Partial Properties | DynamicProxy: `PropertyNode` adds an `IsPartial` flag, set by `ClassProxyAstBuilder`/`InterfaceImplAstBuilder`; SG: `ProxyEmitter` detects the partial keyword via syntax analysis, generates stub accessors for declaration-only partial properties of an abstract class, and uses normal delegation code for interfaces and non-abstract classes | `PropertyNode.cs`; `ClassProxyAstBuilder.cs`; `InterfaceImplAstBuilder.cs`; `ProxyEmitter.cs` |
| C# 10.0 | ✅ Interpolated String Handlers | When a custom interpolated handler struct (non-ref struct) is used as a parameter/return value, it is correctly handled via the `Sync` return path; covers `$"..."` interpolation lowering (the compiler generates the handler and passes it into the proxy method) and `[InterpolatedStringHandlerArgument]` parameter forwarding; the interceptor chain can inspect and replace struct return values | `InterpolatedStringHandlerAndIndexRangeParityTests.cs` |
| C# 8.0 | ✅ System.Index / System.Range | `Index`/`Range` are handled as ordinary structs, correctly handled via the `Sync` return path; both parameters and return values are correctly forwarded; the interceptor chain can inspect and replace the return value | `InterpolatedStringHandlerAndIndexRangeParityTests.cs` |
| C# 9.0 | ✅ record types | Both engines support record class proxying + the `with` expression; SG generates a `record class` proxy, and DynamicProxy manually implements `<Clone>$`/`<>Copy`. For known differences, see [Record Type Support](./record-support.md) (equality, init setter, derived-record `<Clone>$` limitation) | `RecordTypeUtils.cs`; `ClassProxyAstBuilder.cs`; `ILEmitVisitor.cs`; `ProxyEmitter.cs` |
| C# 7.0 | ✅ ref / ref readonly returns | Added `ReturnKind.RefSync`; under the value-semantic pipeline, the return value is materialized into a `StrongBox<T>`, returning `ref box.Value`; the non-intercepted path preserves true ref aliasing; `MethodReflector.EmitReturn` handles byref return dereferencing | `ReturnKind.cs`; `MethodBodyFactory.cs`; `ILEmitVisitor.cs`; `ProxyEmitter.cs`; `MethodReflector*.cs` |

## 3. Features Requiring Adaptation

### 3.1 P0 — Features Already Adapted and Those Still Pending

#### 3.1.1 ✅ Async streams (IAsyncEnumerable\<T\> / IAsyncDisposable) — C# 8.0 — Adapted

**Implementation result**

- `ReturnKind` adds `AsyncEnumerable`; DynamicProxy and Source Generator call `IAspectActivator.InvokeAsyncEnumerable<T>` respectively.
- The async stream stays lazy: the interceptor chain and the target method execute on first enumeration; when the stream completes, is canceled, or an exception occurs during enumeration, the `AspectContext` is released.
- `IAsyncDisposable.DisposeAsync()` returns a `ValueTask`, and interception is already completed via the existing `ValueTask` proxy path.
- The `netstandard2.0` target introduces `Microsoft.Bcl.AsyncInterfaces` to expose the C# 8 async interfaces.

**Verification coverage**

- DynamicProxy: async stream enumeration, exception propagation during enumeration, and `IAsyncDisposable.DisposeAsync()` interception.
- Source Generator: consistency of async stream return type behavior with DynamicProxy.

**Involved implementation**: `IAspectActivator.cs`, `AspectActivator.cs`, `MethodBodyFactory.cs`, `ILEmitVisitor.cs`, `ProxyEmitter.cs`, `InterceptUtils.cs`.

---

#### 3.1.2 ✅ init-only property setters — C# 9.0 — Adapted

**Implementation result**

- DynamicProxy: `PropertyNode` adds `RequiredCustomModifiers`/`OptionalCustomModifiers` fields, reading the init-only modreq metadata from `PropertyInfo.GetRequiredCustomModifiers()`/`GetOptionalCustomModifiers()`.
- DynamicProxy: `ILEmitVisitor.VisitProperty` passes the custom modifiers via the `DefineProperty` overload; `VisitMethod` and `EmitClassProxyCtor` also pass the custom modifiers of parameters and return values via `DefineMethod`/`DefineConstructor`.
- Source Generator: `ProxyEmitter` detects `accessor.IsInitOnly` and generates the `init` keyword rather than `set`; in non-override scenarios it uses `forceReflectiveDirectCall` (calling the init accessor via reflection) to avoid the compilation restriction of directly calling an init accessor.
- Added `GetRequiredCustomModifiers`/`GetOptionalCustomModifiers`/`GetConstructorParameterCustomModifiers` helper methods, with try-catch fault tolerance.

**Verification coverage**

- `InitRequiredMembersParityTests` (dual engine): verifies that proxy properties preserve the init-only modreq metadata, interceptors work normally, and interface proxies (with/without target) preserve the init-only metadata.

**Involved implementation**: `PropertyNode.cs`, `ClassProxyAstBuilder.cs`, `InterfaceImplAstBuilder.cs`, `ILEmitVisitor.cs`, `ProxyEmitter.cs`.

---

#### 3.1.3 ✅ required members — C# 11.0 — Adapted

**Implementation result**

- DynamicProxy: preserves the `required` property's metadata via custom modifiers (`RequiredCustomModifiers`/`OptionalCustomModifiers`), so `IsRequired` can be detected on the proxy property via reflection.
- Source Generator: `ProxyEmitter` detects `prop.IsRequired` and generates the `required` keyword before the property declaration; the new `EmitConstructorAttributes` method detects and forwards the `[SetsRequiredMembers]` attribute to the proxy constructor, so that `required` members can be set in an object initializer.
- The `required` attribute usually works in tandem with `init`, and the two adaptations have been merged.

**Verification coverage**

- `InitRequiredMembersParityTests` (dual engine): verifies that proxy properties preserve the `required` metadata (`IsRequiredProperty` check), the proxy constructor carries the `[SetsRequiredMembers]` attribute, and `required` properties can be set via an object initializer.

**Involved implementation**: `ProxyEmitter.cs`, `ILEmitVisitor.cs`, `PropertyNode.cs`.

---

#### 3.1.4 record types — C# 9.0

**Problem description**

Records are `sealed` by default, and synthesize members such as `Equals`, `GetHashCode`, `Deconstruct`, and `<>Copy`. Current handling:

- Source Generator: rejects sealed non-abstract types (`IsSealed && !IsAbstract` → ACSG003 diagnostic)
- DynamicProxy: **no sealed check**, crashes at runtime

**Scope of impact**

- `AspectCoreProxyGenerator.cs:105` rejects sealed types
- `ILEmitVisitor.cs` has no sealed type check
- Record synthesized members (`<>Copy`, the `with` expression) have no special handling
- `record struct` (value-type record) is not handled

**Adaptation plan**

1. Add a sealed type check to DynamicProxy (aligning with SG), erroring early rather than crashing at runtime
2. For `record class` (non-sealed scenarios): correctly forward the synthesized members; the `with` expression requires the `<>Copy` method to be proxied
3. For `record struct`: value-type proxying requires additional handling (boxing/unboxing)
4. Consider supporting `record` interface proxying (a record interface is not sealed)
5. A record's `init` properties need to work in tandem with the init adaptation

**Adaptation difficulty**: ⭐⭐⭐⭐

---

#### 3.1.5 ref struct — C# 7.2 ⚠️ Partially adapted

ref structs (such as `Span<T>`, `ReadOnlySpan<T>`) have CLR-enforced restrictions: they cannot be boxed, cannot be used as an interface implementation, cannot be used as a class field, and cannot be used in `async`/`iterator` methods. Two independent scenarios must be distinguished:

**(a) ref struct as a proxy target type — ✅ rejected**

- DynamicProxy: `ProxyTypeGenerator` checks `type.IsByRefLike` at the entry (`RejectRefStruct`) and rejects a ref struct as a proxy target, failing early rather than crashing at runtime.
- Source Generator: `AspectCoreProxyGenerator` reports ACSG008 for a candidate type that `IsRefLikeType` and skips generation.

**(b) byref-like parameters/return values on an intercepted method — ❌ still unsupported**

- Only the **non-intercepted direct-call path** passes ref struct parameters/return values correctly (IL direct pass-through, or the SG direct call).
- Once a method enters the **interception path**, both engines pack the arguments into `object[]`: DynamicProxy's `EmitInitializeMetaData` calls `EmitConvertToObject` on each parameter (`ILEmitVisitor.cs`), and SG's `EmitArgumentsArray` emits `new object[]{...}` (`ProxyEmitter.cs`). A ref struct cannot be boxed to `object`, so it throws `InvalidProgramException` at runtime (for example, an intercepted `int Length(Span<int>)` throws exactly this).
- SG currently reports ACSG009 only for byref-like `params` parameters (see 3.2.2); a non-`params` `Span<T>` parameter on an intercepted method has no dedicated diagnostic and fails at runtime.

**Conclusion**: ref struct proxy targets are safely rejected; but "byref-like parameters/return values on an intercepted method" is not yet supported — a known limitation.

---

#### 3.1.6 ✅ ref return methods — C# 7.0 — Adapted

**Implementation result**

- `ReturnKind` adds `RefSync` (`ReturnKindKind.RefSync` on the SG side). `MethodBodyFactory.DetermineReturnKind` detects a runtime ref return via `ReturnType.IsByRef`; SG detects it via `IMethodSymbol.RefKind` (`Ref`/`RefReadOnly`).
- The interceptor pipeline is value-semantic (`AspectContext.ReturnValue` is `object`), so an intercepted ref return value is materialized into a `StrongBox<T>`, and the proxy method returns `ref box.Value` — a managed pointer to a heap slot, which remains valid after the method returns.
  - DynamicProxy: `ILEmitVisitor.VisitAspectActivatorBody` uses `newobj StrongBox<T>` + `ldflda Value` to return the reference; `EmitReturnValue` calls `Invoke<TElement>` with the element type.
  - Source Generator: `EmitProxyInvokeBody` generates `new StrongBox<T>()` and `return ref __refBox.Value`; the method signature prepends `ref`/`ref readonly`.
- Methods that do not hit interception go through the original direct path (DynamicProxy `DirectDelegationBody` / SG `return ref {directCall}`), preserving **true ref aliasing** semantics.
- `MethodReflector` (the dynamic method that calls the target during interception) adds ref return handling: `EmitReturn` first `ldind`/`ldobj` dereferences a `ReturnType.IsByRef` and then boxes it as a value type.
- SG targetless interface stub: a ref return stub returns a `ref` to a static default slot; a generic ref return stub throws `NotSupportedException`.
- `RuntimeAspectContext.Break()` first does `GetElementType()` for a ref return type and then takes the default value, to avoid the `T&` default value unbox failure.

**Behavioral boundary**

- ✅ Read: `ref x = ref proxy.Foo()` reads the correct value (including the value after interceptor replacement).
- ✅ Interceptors can inspect/replace the return value.
- ✅ The returned ref is writable (points to the StrongBox heap slot).
- ⚠️ When intercepted, a write through the returned ref **does not** flow back to the target object's original storage — this is an inherent limitation of the value-semantic pipeline, consistent with the existing ref/out parameter copy-back semantics.
- `ref readonly` returns are treated as read-only.

**Verification coverage**

- `RefReturnParityTests` (dual engine × 4 TFM): class proxy reading of ref/ref readonly/reference type returns, interceptor replacement, the returned ref being writable, and interface proxy (with target) ref return + interception.
- `RefReturnScenarios` (E2E, runtime engine): ref return read value + interception, interceptor replacement, ref readonly read value.

**Involved implementation**: `ReturnKind.cs`, `MethodBodyFactory.cs`, `ILEmitVisitor.cs`, `AspectContext.Runtime.cs`, `MethodUtils.cs`, `MethodReflector.cs`/`.Call.cs`/`.Static.cs`, `ProxyEmitter.cs`.

**Adaptation difficulty**: ⭐⭐⭐⭐

### 3.2 P1 — Missing Functionality but Does Not Cause Crashes

#### 3.2.1 ✅ Primary Constructors — C# 12.0 — Adapted

**Problem description**

The current constructor forwarding logic uses `GetParameters()` for generic handling, unaware of primary constructor semantics. For a class's primary constructor, the parameters need to be correctly forwarded via `base()`; for a record's primary constructor, `init` properties and the `with` expression are also involved.

**Scope of impact**

- `ClassProxyCtorFromBase` forwards parameters but does not handle the case where primary constructor parameters are captured by properties
- `ProxyEmitter.EmitClassConstructors` forwards via `: base(...)`, but is not aware of the primary constructor

**Adaptation plan**

1. Detect the primary constructor (`[PrimaryConstructorParameters]` on the `ConstructorInfo` or a type characteristic)
2. Ensure the primary constructor parameters are correctly forwarded to `base()`
3. For a record primary constructor, handle the `<>Copy` of `init` properties and the `with` expression
4. Test the interaction of primary constructor parameters with property initializers

**Adaptation difficulty**: ⭐⭐⭐

**✅ Implemented**:

| Engine | Change | Involved files |
|------|------|---------|
| DynamicProxy | `AttributeNodeFactory.SkippedAttributeFullNames` adds `PrimaryConstructorParametersAttribute`, to prevent the compiler-generated attribute from being copied to the proxy type | `AttributeNodeFactory.cs:22` |
| DynamicProxy | `ClassProxyAstBuilder.BuildConstructors` already correctly forwards primary constructor parameters to `base()` (the existing logic needs no modification) | `ClassProxyAstBuilder.cs:84-125` |
| Source Generator | `CompilerGeneratedAttributeNames` adds `PrimaryConstructorParametersAttribute` | `ProxyEmitter.cs:1050` |
| Source Generator | `EmitClassConstructors` uses `EmitParameterDecl` to forward parameters, preserving `params`/`ref`/`out`/`in` semantics | `ProxyEmitter.cs:383-412` |
| Tests | Added `PrimaryConstructorTests` (DynamicProxy) and `PrimaryConstructorAndParamsCollectionParityTests` (dual engine), covering class/record primary constructors + interceptors | `tests/AspectCore.Core.Tests/` |

---

#### 3.2.2 ✅ params collections (params IEnumerable\<T\>) — C# 13.0 — Adapted

**Problem description**

No handling. A `params` collection parameter has a `[ParamCollection]` attribute in the method signature, and the proxy method needs to correctly forward this attribute and preserve the `params` semantics.

**Scope of impact**

- `ParameterNodeFactory` does not check the `[ParamCollection]` attribute
- IL emission does not forward the `[ParamCollection]` attribute
- SG does not generate the `params` modifier

**Adaptation plan**

1. Add an `IsParamsCollection` flag to `ParameterNode`
2. Detect and forward the `[ParamCollection]` attribute
3. Forward `[ParamCollection]` in IL via `SetCustomAttribute`
4. Generate the `params` modifier in SG

**Adaptation difficulty**: ⭐⭐

**✅ Implemented**:

| Engine | Change | Involved files |
|------|------|---------|
| DynamicProxy | The `[ParamCollection]` attribute is not in `SkippedAttributeFullNames`, so it is automatically forwarded via `ParameterNodeFactory.FromParameterInfo` → `AttributeNodeFactory.FromCustomAttributes`; `ILEmitVisitor` sets it onto the parameter via `SetCustomAttribute` | `ParameterNodeFactory.cs:35-59`; `ILEmitVisitor.cs:254-283` |
| Source Generator | `EmitParameterDecl` detects `p.IsParams` and emits the `params` keyword (covering `params IEnumerable<T>`, `params T[]`); byref-like parameters such as `params ReadOnlySpan<T>` report ACSG009, to avoid the subsequent `object[]` boxing error | `ProxyEmitter.cs:893-963` |
| Source Generator | Does not explicitly forward `ParamCollectionAttribute`: the compiler automatically synthesizes this attribute on a `params` collection parameter, and emitting it explicitly would cause CS0674 | `ProxyEmitter.cs:1130-1142` |
| Source Generator | `EmitClassConstructors` constructor parameters also use `EmitParameterDecl`, preserving the `params` semantics | `ProxyEmitter.cs:394` |
| Tests | Added `ParamsCollectionTests` (DynamicProxy) and `PrimaryConstructorAndParamsCollectionParityTests` (dual engine), covering `params IEnumerable<int>`, `params string[]` + interceptors | `tests/AspectCore.Core.Tests/` |

> **Known boundary**: `params ReadOnlySpan<T>` is a byref-like `params` parameter (a ref struct, see 3.1.5); the SG side reports ACSG009 and skips generation — because a ref struct cannot be passed through an `object[]` parameter array. This is a current known limitation, outside the scope of this params-collection adaptation.

---

#### 3.2.3 ref field / scoped modifier — C# 11.0 ✅ Adapted

**Background**

`ref` fields and the `scoped` keyword in a ref struct affect parameter passing; if the `scoped` modifier is not preserved, an invalid signature may be generated in a `ref struct` context.

**Implementation result**

- Source Generator: `EmitParameterDecl` detects the compiler-generated `[ScopedRefAttribute]` (because Roslyn 4.10.0 has no `IParameterSymbol.IsScoped`), and emits the `scoped` modifier before `ref`/`out`/`in` to preserve parameter semantics.
- A ref struct type itself is not a proxy target (consistent with 3.1.5: DynamicProxy rejects it, and SG reports ACSG008).

**Verification coverage**: `RefStructAndScopedParityTests` (dual engine) covers the forwarding and interception of `scoped ref`/`scoped in` parameters.

---

#### 3.2.4 ✅ Partial Properties — C# 13.0 — Adapted

**Implementation result**

- DynamicProxy: `PropertyNode` adds an `IsPartial` flag; `ClassProxyAstBuilder` and `InterfaceImplAstBuilder` set this flag when building the property node (compatible with pre-.NET 9.0 via a `MethodBase.IsPartialMethod` polysemy polyfill).
- Source Generator: `ProxyEmitter` detects the `partial` keyword modifier via syntax analysis; it generates stub accessors (with no implementation body) for declaration-only partial property accessors in an abstract class, skipping meta fields (no reflective dispatch needed); interfaces and non-abstract classes use normal delegation code.

**Verification coverage**

- DynamicProxy: class proxy, interface proxy (with/without target), read-only partial property + interceptor.
- Source Generator: consistency of partial property behavior with DynamicProxy.
- E2E: 6 E2E tests cover partial property scenarios (class proxy, interface proxy + target, read-only, mixed accessors).

**Involved implementation**: `PropertyNode.cs`, `ClassProxyAstBuilder.cs`, `InterfaceImplAstBuilder.cs`, `ProxyEmitter.cs`.

---

#### 3.2.5 ✅ Interpolated String Handlers — C# 10.0 — Adapted

**Implementation result**

- When a custom interpolated handler struct (non-ref struct) is used as a parameter and return value, it is correctly handled via the `Sync` return path: DynamicProxy uses the `Invoke<TResult>` generic method, and the Source Generator generates direct delegation code.
- The interceptor chain can correctly inspect and replace the struct return value (`ctx.ReturnValue` is the boxed struct).
- `DefaultInterpolatedStringHandler` is an internal compiler type and is not used as an ordinary method parameter/return type; a custom handler struct covers the actual usage scenarios.
- A handler parameter with `[InterpolatedStringHandlerArgument]`: the compiler performs lowering when the call site uses `$"..."` syntax (constructing the handler + `AppendLiteral`/`AppendFormatted`), and the proxy method correctly forwards the compiler-generated handler struct.

**Verification coverage**

- `InterpolatedStringHandlerAndIndexRangeParityTests` (dual engine):
  - Pass-through and interceptor replacement of a custom handler struct as a return value;
  - Forwarding of a custom handler struct as a parameter;
  - **Real `$"..."` interpolation lowering**: calling `proxy.HandlerToUpper($"Test{number}")`, verifying that the compiler-generated handler is correctly forwarded by the proxy;
  - **`[InterpolatedStringHandlerArgument]`**: calling `proxy.FormatWithCategory("INFO", $"Value is {value}")`, verifying that a handler parameter with associated parameters is correctly forwarded.

**Involved implementation**: No engine code modification needed — the `Sync` return path already correctly handles struct-type return values and parameters.

---

#### 3.2.6 ✅ System.Index / System.Range — C# 8.0 — Adapted

**Implementation result**

- `Index` and `Range` are handled as ordinary structs, correctly handled via the `Sync` return path.
- `Index`/`Range` parameters are correctly forwarded to the target method (method calls such as `Index.GetOffset`, `Range.GetOffsetAndLength` work normally).
- The interceptor chain can correctly inspect and replace `Index`/`Range` return values.

**Verification coverage**

- `InterpolatedStringHandlerAndIndexRangeParityTests` (dual engine): `Index` return value pass-through + interception, `Range` return value pass-through + interception, `Index` parameter forwarding, `Range` parameter forwarding, and interceptor replacement of `Index`/`Range` return values.

**Involved implementation**: No engine code modification needed — the `Sync` return path already correctly handles struct-type return values and parameters.

### 3.3 P2 — Compile-time Annotations, Minimal Impact or No Changes Needed

The following features are all compile-time annotations or internal compiler behavior, and do not affect the core semantics of the proxy IL. However, the difference between the two engines in attribute forwarding (DynamicProxy copies everything vs. Source Generator forwards nothing) leads to different adaptation needs.

#### Comparison of the Current Attribute Forwarding Mechanism

| Dimension | DynamicProxy (IL emit) | Source Generator |
|------|----------------------|-----------------|
| Type attributes | Fully copies implementation type attributes + marker attributes (`ClassProxyAstBuilder.cs:42-49`) | Only emits `[NonAspect]` + `[Dynamically]` (`ProxyEmitter.cs:52-53,142-143`) |
| Method attributes | Fully copies service method attributes (`InterfaceImplAstBuilder.cs:369-373`) | Does not forward |
| Parameter attributes | Fully copies parameter attributes (`ParameterNodeFactory.cs:45-49`) | Does not forward |
| Property attributes | Fully copies property attributes (`ClassProxyAstBuilder.cs:243-248`) | Does not forward |
| Constructor attributes | Fully copies constructor attributes (`ClassProxyAstBuilder.cs:102-106`) | Does not forward |
| Generic parameter attributes | Fully copies generic parameter attributes (`GenericParameterNodeFactory.cs:49`) | Does not forward (forwards constraints only) |
| Attribute filtering | **No filtering** — `AttributeNodeFactory.FromCustomAttributes` blindly copies all `CustomAttributeData` | No filtering — simply does not forward |
| Generic attribute support | **Not supported** — `BuildCustomAttribute` (`ILEmitVisitor.cs:619-648`) does not handle generic attribute types | Not applicable |

---

#### 3.3.1 ✅ Nullable Reference Types (NRT) — C# 8.0 — Adapted

**Feature description**

NRT expresses nullability annotations via `[NullableContextAttribute]` (type level, marking the default nullability) and `[NullableAttribute]` (member level, marking the nullability of a specific member). These are compile-time annotations and do not affect the runtime IL semantics.

**Current status**

- **DynamicProxy**: the `[NullableContext]` and `[Nullable]` attributes are blindly copied to the proxy type/method/parameter. Since `AttributeNodeFactory.FromCustomAttributes` does no filtering, these compiler-generated attributes are forwarded verbatim.
  - Potential problem: the `[NullableContext]` on the proxy type may conflict with the proxy's own nullability context; the constructor parameter of the `[Nullable]` attribute (`byte` or `byte[]`) is read in `BuildCustomAttribute` via `ReadAttributeValue`, and for a `byte[]`-typed array value there may be a type-inference problem (an empty array or an all-null array).
- **Source Generator**: forwards no attributes at all. The SG-generated proxy type has no NRT annotations, so the consumer loses nullability information when using the proxy type, potentially producing unnecessary CS8602/CS8603 warnings.
  - Partial mitigation: SG already emits `notnull` in generic constraints (`ProxyEmitter.cs:970-1031`), but this only covers the `notnull` constraint of generic parameters, not the nullability annotations of ordinary reference types.

**Adaptation plan**

| Engine | Change | Priority |
|------|------|--------|
| DynamicProxy | 1. Add filtering logic in `AttributeNodeFactory.FromCustomAttributes` to skip the `[NullableContext]` and `[Nullable]` attributes (avoiding a nullability context conflict on the proxy type)<br>2. Or: keep copying but ensure `BuildCustomAttribute` correctly handles the `byte[]` constructor parameter | Low |
| Source Generator | 1. Emit the corresponding nullability annotations (`?` suffix) on the generated proxy type, methods, and parameters<br>2. Or add `#nullable enable` at the top of the generated file and use `!`/`?` annotations<br>3. Lowest-cost approach: add `#nullable disable` in the generated file to avoid consumer warnings | Low |

**Testing suggestion**: use `#nullable enable` service types and interfaces, and verify that the proxy type's nullability behavior is consistent with the original type.

---

#### 3.3.2 ✅ Generic Attributes — C# 11.0 — Adapted

**Feature description**

C# 11 allows an attribute class to be generic, for example `[SomeAttribute<T>]`. In IL, the `AttributeType` of a generic attribute is a closed generic type (such as `SomeAttribute<int>`), and its `ConstructorArguments` may contain the values of the type parameters.

**Current status**

- **DynamicProxy**: `BuildCustomAttribute` (`ILEmitVisitor.cs:619-648`) **does not handle generic attribute types**. When it encounters `[SomeAttribute<T>]`:
  - `data.AttributeType` is a closed generic type (such as `SomeAttribute<int>`)
  - `data.Constructor` is the constructor of the generic attribute type
  - `new CustomAttributeBuilder(data.Constructor, ...)` may fail because the constructor signature does not match the closed generic type
  - `attributeTypeInfo.GetProperty(n.MemberName)` and `GetField(n.MemberName)` may return wrong results when looking up named arguments on the generic attribute type
  - **Result**: an `ArgumentException` or `MissingMethodException` is thrown at runtime
- **Source Generator**: forwards no attributes at all, and generic attributes are simply lost.

**Adaptation plan**

| Engine | Change | Priority |
|------|------|--------|
| DynamicProxy | 1. Detect `data.AttributeType.IsGenericType` in `BuildCustomAttribute`<br>2. For a generic attribute, use `data.AttributeType` (already closed) rather than the open generic definition<br>3. Use the closed generic type's constructor when constructing the `CustomAttributeBuilder`<br>4. Perform named property/field lookup on the closed generic type as well | Medium |
| Source Generator | 1. Add attribute forwarding logic in `ProxyEmitter` (see the comprehensive plan in 3.3.7)<br>2. Emit generic attributes in the `[SomeAttribute<int>]` format | Low |

**Testing suggestion**: define a generic attribute class `[GenericAttr<T>]`, apply it to a service type/method/parameter, and verify that the proxy generates successfully and the attribute can be read via reflection.

---

#### 3.3.3 ✅ CallerArgumentExpression — C# 10.0 — Adapted

**Feature description**

`[CallerArgumentExpression("parameterName")]` is applied to a method parameter, allowing the method to obtain the text of the expression the caller passed for a parameter. For example, `void Foo(int value, [CallerArgumentExpression("value")] string expression = null)`.

**Current status**

- **DynamicProxy**: the `[CallerArgumentExpression]` attribute is blindly copied onto the proxy method's parameter. `BuildCustomAttribute` can correctly handle its string constructor parameter.
  - **Problem**: the proxy method's parameter name may differ from the original method's (for example, the proxy method uses `arg0`, `arg1`), causing the parameter name referenced by `[CallerArgumentExpression("value")]` not to exist in the proxy method, and the compiler reports CS8917.
  - **Actual situation**: AspectCore proxy methods use the original parameter names (`ParameterNodeFactory.FromConstructor`/`FromMethod` preserve the original names), so there is usually no problem. But if the proxy method's parameter order differs from the original method's (for example, a constructor proxy inserts an `IAspectActivatorFactory` parameter in front), the parameter position referenced by `CallerArgumentExpression` may shift.
- **Source Generator**: forwards no parameter attributes at all, and `[CallerArgumentExpression]` is lost. The caller of the proxy method cannot obtain the expression text.

**Adaptation plan**

| Engine | Change | Priority |
|------|------|--------|
| DynamicProxy | 1. Verify that the proxy method's parameter names are consistent with the original method's (currently satisfied)<br>2. For a constructor proxy (with an `IAspectActivatorFactory` parameter inserted in front), ensure the parameter name referenced by `[CallerArgumentExpression]` is still valid<br>3. No special handling needed, the current behavior is basically correct | Low |
| Source Generator | 1. Detect and forward the `[CallerArgumentExpression]` attribute in `ProxyEmitter.EmitParameterDecl`<br>2. Generation format: `[CallerArgumentExpression("paramName")] ref/out/in Type paramName`<br>3. Ensure the referenced parameter name is consistent with the SG-generated parameter name | Low |

**Testing suggestion**: an interface/class with a method parameter carrying `[CallerArgumentExpression]`, and verify that the expression text is correctly passed after the proxy call.

---

#### 3.3.4 ✅ Collection Expressions — C# 12.0 — Adapted

**Feature description**

A collection expression (such as `[1, 2, 3]`) compiles to a call to the builder method specified by the `[CollectionBuilder]` attribute. `[CollectionBuilder]` is applied to a type, specifying that the type supports collection expression initialization.

**Current status**

- **DynamicProxy**: the `[CollectionBuilder]` attribute is blindly copied onto the proxy type. `BuildCustomAttribute` can correctly handle its two string constructor parameters (`Type builderType` and `string methodName`).
  - **Problem**: the constructor parameter of `[CollectionBuilder]` is a `Type` (`builderType`), and `ReadAttributeValue` needs to correctly handle a `Type`-typed attribute value. The current `ReadAttributeValue` may not support a `Type`-typed attribute value.
- **Source Generator**: forwards no type attributes at all, and `[CollectionBuilder]` is lost. The proxy type does not support collection expression initialization.

**Adaptation plan**

| Engine | Change | Priority |
|------|------|--------|
| DynamicProxy | 1. Confirm the handling of `Type`-typed attribute values in `BuildCustomAttribute` / `ReadAttributeValue` (`CustomAttributeTypedArgument.Value` returns a `Type` object for a `Type` type, and should be passable directly)<br>2. The current behavior is basically correct; need to verify whether the `Type` parameter of `[CollectionBuilder]` is correctly copied | Low |
| Source Generator | 1. Forward the type-level `[CollectionBuilder]` attribute in `ProxyEmitter`<br>2. Generation format: `[global::System.Runtime.CompilerServices.CollectionBuilder(typeof(BuilderType), "MethodName")]` | Low |

**Testing suggestion**: a service type with `[CollectionBuilder]`, and verify that the proxy type also supports collection expression initialization.

---

#### 3.3.5 ✅ Experimental attribute — C# 12.0 — Adapted

**Feature description**

`[Experimental("DiagnosticId")]` marks a type/member as experimental, and the compiler emits the specified diagnostic warning when it is used. This is a pure compile-time annotation.

**Current status**

- **DynamicProxy**: the `[Experimental]` attribute is blindly copied. `BuildCustomAttribute` can correctly handle its string constructor parameter.
  - **Effect**: the proxy type is also marked as experimental, and a consumer using the proxy will also receive an experimental warning. This is **correct behavior**.
- **Source Generator**: forwards nothing, and `[Experimental]` is lost. The proxy type does not trigger the experimental diagnostic, and a consumer may use experimental functionality without knowing it.

**Adaptation plan**

| Engine | Change | Priority |
|------|------|--------|
| DynamicProxy | No change needed, the current behavior is correct (the blind copy already includes `[Experimental]`) | — |
| Source Generator | 1. Forward the type-level and method-level `[Experimental]` attribute in `ProxyEmitter`<br>2. Generation format: `[global::System.Diagnostics.CodeAnalysis.Experimental("DiagnosticId")]` | Low |

**Testing suggestion**: a service type with `[Experimental]`, and verify that the SG-generated proxy type also carries this attribute.

---

#### 3.3.6 ✅ Attributes on Type Parameters — C# 13.0 — Adapted

**Feature description**

C# 13 allows applying attributes directly on type parameters, such as `interface IFoo<[SomeAttr] T>` or `void Bar<[SomeAttr] T>()`. The attribute is applied to the generic parameter itself, not to the generic constraint.

**Current status**

- **DynamicProxy**: generic parameter attributes are fully copied via `GenericParameterNodeFactory.cs:49`, and `ILEmitVisitor.cs:598-599,614-615` calls `SetCustomAttribute` on the `GenericParameterBuilder`.
  - **Problem**: the same problem as generic attributes (3.3.2) — if the attribute on the type parameter is itself a generic attribute, `BuildCustomAttribute` does not support it.
  - **Another problem**: `AttributeNodeFactory.FromCustomAttributes` does no filtering, so compiler-generated attributes (such as `[NullableContext]`) are also copied onto the generic parameter.
- **Source Generator**: `EmitGenericConstraints` (`ProxyEmitter.cs:970-1031`) only forwards generic constraints (`class`, `struct`, `unmanaged`, `notnull`, `new()`, base type, interfaces), and **does not forward generic parameter attributes**.

**Adaptation plan**

| Engine | Change | Priority |
|------|------|--------|
| DynamicProxy | 1. Confirm that the generic parameter attribute forwarding logic is correct (currently basically correct)<br>2. Fix the generic attribute support problem (see 3.3.2)<br>3. Optional: filter out compiler-generated attributes (such as `[NullableContext]`) | Low |
| Source Generator | 1. Add the emission of generic parameter attributes in `EmitGenericConstraints`<br>2. Generation format: `void Bar<[SomeAttr] T>() where T : class`<br>3. The attribute should be placed before the type parameter name, and the constraint in the `where` clause | Low |

**Testing suggestion**: a generic interface/method with type parameter attributes, and verify that the attributes on the proxy type's generic parameters can be read via reflection.

---

#### 3.3.7 ✅ Method Group Natural Type — C# 13.0 — Adapted

**Feature description**

C# 13 improves the conversion of method groups to delegate types, so that method groups have a "natural type" in more contexts, reducing explicit type conversions. This is pure compiler behavior and produces no new IL structure or attributes.

**Current status**

- **DynamicProxy**: no impact. The proxy method's signature is consistent with the original method's, and method group conversion is handled by the consumer code's compiler.
- **Source Generator**: no impact. Same as above.

**Adaptation plan**

No changes needed. This feature is entirely implemented at the C# compiler level, and does not involve runtime IL or attributes.

---

#### 3.3.8 Source Generator Comprehensive Attribute Forwarding Plan

Since the Source Generator currently **forwards no custom attributes at all**, all the "SG does not forward" problems in 3.3.1~3.3.6 above need a comprehensive solution:

**Plan A: Selective attribute forwarding (recommended)**

Add attribute forwarding logic to `ProxyEmitter`, forwarding only "safe" attributes:

```csharp
// New method added to ProxyEmitter.cs
private static void EmitAttributes(StringBuilder sb, ISymbol symbol, string indent, AttributeTarget target)
{
    foreach (var attr in symbol.GetAttributes())
    {
        // Skip compiler-generated attributes
        if (IsCompilerGeneratedAttribute(attr)) continue;
        
        // Skip AspectCore's own marker attributes
        if (IsAspectCoreMarkerAttribute(attr)) continue;
        
        // emit the attribute
        sb.Append(indent).Append('[').Append(FormatAttribute(attr)).AppendLine("]");
    }
}
```

The whitelist of attributes to forward (P2-related):
- `[Experimental]`
- `[CollectionBuilder]`
- `[CallerArgumentExpression]`
- `[Nullable]` / `[NullableContext]` (or replace with `#nullable enable`)
- Type parameter attributes (non-compiler-generated)

The blacklist of attributes to filter out:
- `[CompilerGenerated]`
- `[NonAspect]`, `[Dynamically]` (AspectCore's own markers)
- `[AspectCoreGenerateProxy]`
- All attributes under the `AspectCore.DynamicProxy.*` namespace

**Plan B: Full attribute forwarding**

Similar to the DynamicProxy behavior, forward all non-compiler-generated attributes to the SG-generated proxy type. The advantage is consistent behavior; the disadvantage is that it may introduce unnecessary attributes (for example, `[Obsolete]` would cause consumers of the proxy to also receive warnings).

**Recommendation**: adopt Plan A (selective forwarding), and prioritize implementing forwarding of the three attributes `[Experimental]`, `[CollectionBuilder]`, and `[CallerArgumentExpression]`.

#### 3.3.9 DynamicProxy Comprehensive Attribute Filtering Plan

DynamicProxy currently blindly copies all attributes, and needs to add filtering logic to avoid problems:

```csharp
// New filtering logic added to AttributeNodeFactory.cs
public static List<AttributeNode> FromCustomAttributes(IEnumerable<CustomAttributeData> attributeDataList)
{
    var result = new List<AttributeNode>();
    foreach (var data in attributeDataList)
    {
        // Skip compiler-generated attributes
        if (IsCompilerGeneratedAttribute(data.AttributeType)) continue;
        
        // Skip attributes that cannot be safely rebuilt (e.g. generic attributes not yet supported)
        if (data.AttributeType.IsGenericType && !SupportsGenericAttribute(data)) continue;
        
        result.Add(new AttributeNode(data));
    }
    return result;
}

private static bool IsCompilerGeneratedAttribute(Type attributeType)
{
    return attributeType.Namespace == "System.Runtime.CompilerServices"
        && (attributeType.Name == "NullableContextAttribute"
            || attributeType.Name == "NullableAttribute"
            || attributeType.Name == "CompilerGeneratedAttribute"
            || attributeType.Name == "IsReadOnlyAttribute"
            || attributeType.Name == "IsByRefLikeAttribute");
}
```

**Recommendation**: prioritize filtering out the `[CompilerGenerated]` attribute family, then fix generic attribute support (3.3.2).

## 4. Features Requiring No Adaptation

The following features are pure syntactic sugar that compile to standard IL, and the AOP Emit layer does not need to be aware of them:

| C# version | Feature | Reason |
|---------|------|------|
| C# 6 | String interpolation, nameof, null-conditional, exception filters, using static, expression-bodied members, auto-property initializers, index initializers | All compile to standard method calls/property access |
| C# 7 | Pattern matching (is/switch), local functions, out variables, discards, binary literals, digit separators, throw expressions | Compile to standard IL patterns |
| C# 7.1 | async Main, default literal, inferred tuple element names | No proxy impact |
| C# 7.3 | Tuple equality, field: attributes on properties, improved overload resolution | No proxy impact |
| C# 8 | switch expressions, using declarations, static local functions, ??=, unmanaged constructed types | No proxy impact |
| C# 9 | Top-level statements, target-typed new, pattern matching enhancements, static anonymous functions, lambda discard parameters | No proxy impact |
| C# 10 | Global using, lambda improvements, extended property patterns, #line improvements | No proxy impact |
| C# 11 | Raw string literals, list patterns, file-local types, UTF-8 strings, unsigned right shift, relaxed shift | No proxy impact |
| C# 12 | using alias for any type, inline arrays, lambda default parameters | No proxy impact |
| C# 13 | New lock object, post-null check, indexer parameters, new string escape | No proxy impact |

## 5. Adaptation Priority Overview

```
urgency ────────────────────────────────────────────────────────►

P0 (all done)             P1 (missing feature)      P2 (annotations/low-pri)
┌─────────────────┐      ┌─────────────────┐      ┌─────────────────┐
│ [x] record type   │      │                 │      │ [x] NRT annotations│
│ [x] ref return    │      │                 │      │ [x] generic attrs  │
│                  │      │                 │      │ [x] CallerArgument │
│                  │      │                 │      │ [x] collection expr │
│                  │      │                 │      │ [x] Experimental    │
│                  │      │                 │      │ [x] type-param attr │
│                  │      │                 │      │ [x] method group    │
└─────────────────┘      └─────────────────┘      └─────────────────┘

[x] Done (P0): async streams (IAsyncEnumerable<T>) & IAsyncDisposable, init-only, required members, record types, ref/ref readonly returns
[x] Done (P1): scoped modifier, primary constructors, params collections, partial properties, interpolated string handlers, Index/Range
[!] Partially adapted: ref struct (proxy target rejected; byref-like parameters/return values on an intercepted method still unsupported, see 3.1.5)
[x] Done (P2): NRT, generic attributes, CallerArgumentExpression, collection expressions, Experimental, type-parameter attributes, method group natural type
```

## 6. Detailed Explanation of Key Adaptation Points

### 6.1 ✅ IAsyncEnumerable\<T\> / IAsyncDisposable — Adapted

**Implementation approach**: `IAsyncEnumerable<T>` is integrated into both proxy engines via `IAspectActivator.InvokeAsyncEnumerable<T>`. The async iterator is created and the interceptor chain executes when the caller first enumerates, and it then forwards the target stream; regardless of normal completion, early cancellation, or exceptional exit, the `AspectContext` is released in the iterator's `finally`. `IAsyncDisposable.DisposeAsync()` uses the existing `ValueTask` return path, so it can be intercepted as well.

**Implementation location**:

- DynamicProxy: `ReturnKind.AsyncEnumerable`, `ILEmitVisitor.EmitReturnValue`, `AspectActivator.InvokeAsyncEnumerable<T>`.
- Source Generator: `ReturnKindKind.AsyncEnumerable` and `ProxyEmitter.EmitProxyInvokeBody`.
- Windsor: `InterceptUtils` selects the corresponding activator based on the `IAsyncEnumerable<T>` return type.

**Verification**: covers stream enumeration for both dynamic proxy and source-generated proxy, covers exception propagation during enumeration, and the interception of `IAsyncDisposable.DisposeAsync()`.

### 6.2 ✅ init-only setters — Adapted

**Implementation approach**: the `init` accessor preserves its metadata via custom modifiers (modreq). DynamicProxy passes `RequiredCustomModifiers`/`OptionalCustomModifiers` in `DefineProperty`/`DefineMethod`/`DefineConstructor`; the Source Generator detects `IsInitOnly` to generate the `init` keyword, and in non-override scenarios uses reflection to call directly to avoid a compilation restriction.

**Implementation location**:

- DynamicProxy: `PropertyNode.RequiredCustomModifiers`/`OptionalCustomModifiers`, `ILEmitVisitor.VisitProperty`/`VisitMethod`/`EmitClassProxyCtor`.
- Source Generator: `ProxyEmitter.EmitPropertyDeclaration`/`EmitAccessorBody` (`IsInitOnly` check), `forceReflectiveDirectCall`.

**Verification**: covers init-only property metadata preservation and interceptor execution for both class proxies and interface proxies (with/without target).

### 6.3 ✅ required members — Adapted

**Implementation approach**: `required` properties are detected via `IsRequired` and the `required` keyword is generated in SG; the constructor forwards the `[SetsRequiredMembers]` attribute via `EmitConstructorAttributes`, so that an object initializer can set `required` members. DynamicProxy preserves the required metadata via custom modifiers.

**Implementation location**:

- DynamicProxy: `PropertyNode` custom modifiers, `ILEmitVisitor`.
- Source Generator: `ProxyEmitter` (`IsRequired` check, `EmitConstructorAttributes`, `EmitAttributes`).

**Verification**: covers `required` metadata preservation on proxy properties, `[SetsRequiredMembers]` forwarding on the proxy constructor, and setting `required` properties via an object initializer.

### 6.4 record types

**Core challenge**: records are sealed by default, and synthesized members (`<>Copy`, `Equals`, `GetHashCode`, `Deconstruct`) need to be correctly proxied.

**Implementation points**:

- Add a sealed type check to DynamicProxy, erroring early
- Non-sealed record class: forward the synthesized members; the `with` expression requires `<>Copy` to be proxied
- record struct: value-type proxying needs to handle boxing/unboxing
- record interface: can be an interface proxy target

### 6.5 ref struct (partially adapted)

**Status**: a ref struct cannot be boxed and cannot be a class field. Two scenarios:

- **Proxy target type**: ✅ rejected — the entry checks `type.IsByRefLike` (DynamicProxy) / reports ACSG008 (SG), failing early.
- **byref-like parameters/return values on an intercepted method**: ❌ still unsupported — the interception path packs arguments into `object[]`, and a `Span<T>` cannot be boxed, throwing `InvalidProgramException` at runtime; only the non-intercepted direct-call path passes them through. See 3.1.5.

### 6.6 ✅ ref return methods — Adapted

**Core challenge**: a `ref` return method returns a reference to a variable rather than a value, but the AspectCore interceptor pipeline is value-semantic (`AspectContext.ReturnValue` is `object`), and cannot preserve true aliasing to the target's original storage after interception.

**Implementation approach**: added `ReturnKind.RefSync`. When interception is hit, the pipeline runs normally to obtain the value, materializes it into a `StrongBox<T>`, and returns `ref box.Value` (the heap slot address is valid after the method returns); when interception is not hit, the direct path preserves true ref aliasing. `MethodReflector.EmitReturn` first dereferences (`ldind`/`ldobj`) a byref return and then boxes it.

**Implementation location**:

- DynamicProxy: `MethodBodyFactory.DetermineReturnKind` (`IsByRef`), `ILEmitVisitor.VisitAspectActivatorBody`/`EmitReturnValue`, `MethodUtils.StrongBox*`, `AspectContext.Runtime.Break()`.
- Source Generator: `ReturnKind.Determine` (`RefKind`), `ProxyEmitter.EmitProxyMethod` (signature `ref`/`ref readonly`), `EmitProxyInvokeBody` (`StrongBox<T>` + `return ref`), `EmitStubMethod` (ref stub).
- Reflection: `EmitReturn` of `MethodReflector`/`.Call`/`.Static`.

**Behavioral boundary**: read values and interceptor replacement work normally; the returned ref is writable but, when intercepted, a write does not flow back to the target (consistent with ref/out parameter copy-back); `ref readonly` is treated as read-only.

**Verification**: `RefReturnParityTests` (dual engine), `RefReturnScenarios` (E2E).

## 7. Summary

| Category | Count | Key items |
|------|------|--------|
| ✅ Adapted | 26 categories | Covariant returns, async/await, IAsyncEnumerable, IAsyncDisposable, init-only, required, partial properties, interpolated handlers, Index/Range, generics, Nullable\<T\>, ref/out/in parameters, ValueTuple, NRT, generic attributes, CallerArgumentExpression, collection expressions, Experimental, type parameter attributes, method group natural type, primary constructors, params collections, scoped modifier, record types, ref return |
| ⚠️ Partially adapted | 1 item | ref struct: proxy target rejected; byref-like parameters/return values on an intercepted method still unsupported (see 3.1.5) |
| 🔴 P0 to adapt | 0 items | — |
| 🟡 P1 to adapt | 0 items | — |
| 🟢 P2 adapted | 7 items | ✅ NRT, generic attributes, CallerArgumentExpression, collection expressions, Experimental, type parameter attributes, method group natural type |
| ⚪ No adaptation needed | 30+ items | Pure syntactic sugar, compiles to standard IL |

**Current status**: features from C# 6 ~ C# 13 requiring adaptation are largely complete. The `record` types (including the known limitation of derived records under DynamicProxy, see [Record Type Support](./record-support.md)) and `ref`/`ref readonly` return methods — the two P0 items — are adapted. The one remaining known limitation is **ref struct as a byref-like parameter/return value on an intercepted method**: the proxy target type is safely rejected, but the interception path's `object[]` boxing prevents passing `Span<T>` and the like (see 3.1.5).

> **Change log**: all 7 P2 items were adapted in commit `2733202`; P1 primary constructors and params collections were adapted in commit `11ad20b`; P0 ref struct/scoped was adapted in commit `3f8669d`; P0 init-only and required members were adapted in commit `4bf323d`; P0 async streams (IAsyncEnumerable/IAsyncDisposable) were adapted in commit `5770dbb`; P1 partial properties were adapted in commit `a748efc`; P1 interpolated handlers and Index/Range were adapted in commit `fe319f9`; P0 record types were adapted in commit `ea68a9c`; P0 ref/ref readonly returns were adapted in this round (the `StrongBox<T>` value-semantic approach + dual-engine test coverage).
