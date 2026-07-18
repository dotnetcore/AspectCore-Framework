# C# 语言特性 AOP Emit 适配分析

> 目标：梳理从 C# 6 到最新版本（C# 13）引入的语言特性，分析哪些在 AOP Emit 过程中需要 AspectCore 适配，并按优先级给出适配方案。

## 1. 背景

AspectCore 拥有两套 AOP 引擎：

| 引擎 | IL 生成方式 | 关键文件 |
|------|------------|---------|
| **DynamicProxy**（运行时） | `System.Reflection.Emit` + AST 架构 | `ILEmitVisitor.cs`、`ClassProxyBuilder.cs`、`MethodBodyFactory.cs` |
| **Source Generator**（编译时） | Roslyn 生成 C# 源码字符串 | `ProxyEmitter.cs`、`AspectCoreProxyGenerator.cs` |

当前状态：

- `LangVersion` 设为 `10.0`（核心库），Source Generator 使用 `latest`
- 支持 `net6.0` ~ `net9.0` + `netstandard2.0/2.1`
- 两套引擎在方法体生成上有不同路径：DynamicProxy 通过 `ReturnKind` 枚举分发到 `AspectActivator.Invoke*` 系列方法；Source Generator 根据同步/异步生成不同的内联代码

### 1.1 AOP Emit 核心流程

运行时 DynamicProxy 的 Emit 过程：

1. **构建阶段**：`ClassProxyBuilder` / `InterfaceProxyBuilder` / `InterfaceImplBuilder` 构造 AST（`ProxyTypeNode`）
2. **Emit 阶段**：`ILEmitVisitor` 遍历 AST，通过 `ILGenerator` 发射 IL 操作码
3. **方法体决策**：`MethodBodyFactory.DecideBody()` 选择方法体类型（直接委托、反射器委托、切面激活器、桩）

编译时 Source Generator 的 Emit 过程：

1. **发现阶段**：`AspectCoreProxyGenerator` 发现带 `[AspectCoreGenerateProxy]` 的类型
2. **生成阶段**：`ProxyEmitter` 生成 C# 源码（字符串拼接），由 C# 编译器编译

## 2. 已适配的特性

| C# 版本 | 特性 | 适配方式 | 涉及文件 |
|---------|------|---------|---------|
| C# 7.0 | ValueTuple 元组 | 作为普通泛型返回值处理，`ReturnKind.Sync` 路径覆盖 | `ILEmitVisitor.cs:EmitReturnValue` |
| C# 7.0 | ref/out/in 参数 | IL 路径用 `EmitLdRef`/`EmitStRef` 写回值；SG 路径 emit `ref`/`out`/`in` 关键字 | `ILEmitVisitor.cs:374-406,443-464`；`ProxyEmitter.cs:879-901` |
| C# 7.2 | private protected | 作为普通访问修饰符处理 | 构造器/方法生成逻辑 |
| C# 8.0 | Nullable\<T\> 值类型 | `ILGeneratorExtensions` 有完整的 `EmitNullableConversion` 系列方法 | `ILGeneratorExtensions.cs` |
| C# 9.0 | 协变返回类型 | 最完善的现代特性支持：`GetCovariantReturnMethods()`、`IsCovariantReturnMethod()`、`IsOverriddenByCovariantReturnMethod()` 等专用基础设施 | `TypeExtensions.cs:96-216`；`MethodInfoExtensions.cs:38-62`；`ClassProxyBuilder.cs:152-289` |
| C# 5+ | async/await（Task/Task\<T\>/ValueTask/ValueTask\<T\>） | `ReturnKind` 枚举对应返回类型，分别调用 `Invoke<TResult>`/`InvokeTask<T>`/`InvokeValueTask<T>` | `ILEmitVisitor.cs`；`MethodBodyFactory.cs` |
| C# 8.0 | ✅ 异步流（IAsyncEnumerable\<T\>） | DynamicProxy 和 SG 均通过 `InvokeAsyncEnumerable<T>` 返回惰性异步流；枚举时执行拦截器链，且在枚举结束、取消或异常后释放 `AspectContext`。`IAsyncDisposable.DisposeAsync` 已由既有 `ValueTask` 返回值路径支持 | `IAspectActivator.cs`；`AspectActivator.cs`；`ILEmitVisitor.cs`；`ProxyEmitter.cs` |
| C# 2+ | 泛型（类型级 + 方法级） | `GenericParameterNodeFactory` 提取泛型参数和约束；IL 用 `DefineGenericParameters`；SG 转发类型参数 + `where` 约束 | `ILEmitVisitor.DefineGenericParameters`；`ProxyEmitter.cs` |
| C# 8.0 | ✅ 可空引用类型（NRT） | DynamicProxy：`AttributeNodeFactory.SkippedAttributeFullNames` 过滤 `[NullableContext]`/`[Nullable]` 等 7 种编译器生成属性；SG：`CompilerGeneratedAttributeNames` 黑名单不转发 | `AttributeNodeFactory.cs:14-40`；`ProxyEmitter.cs:1041-1051` |
| C# 11.0 | ✅ 泛型属性 | DynamicProxy：`BuildCustomAttribute` 新增 `IsGenericType` 分支，从闭合泛型类型解析构造函数；命名属性/字段查找增加 null-safety | `ILEmitVisitor.cs:626-658` |
| C# 10.0 | ✅ CallerArgumentExpression | DynamicProxy：代理方法使用原始参数名，`[CallerArgumentExpression]` 引用正确；SG：加入 `ForwardedAttributeNames` 白名单转发 | `ProxyEmitter.cs:1054-1059` |
| C# 12.0 | ✅ 集合表达式（CollectionBuilder） | DynamicProxy：`[CollectionBuilder]` 属性随自定义属性自动复制；SG：加入 `ForwardedAttributeNames` 白名单转发 | `ProxyEmitter.cs:1054-1059` |
| C# 12.0 | ✅ Experimental 属性 | DynamicProxy：随自定义属性自动复制（正确行为）；SG：加入 `ForwardedAttributeNames` 白名单转发 | `ProxyEmitter.cs:1054-1059` |
| C# 13.0 | ✅ 类型参数属性 | DynamicProxy：`GenericParameterNodeFactory` 全量复制泛型参数属性；SG：`EmitTypeParameterWithAttributes` 转发非黑名单属性 | `ProxyEmitter.cs:1192-1196` |
| C# 13.0 | ✅ 方法组自然类型 | 无需改动 — 纯编译器行为，不涉及 IL 或属性 | — |
| C# 12.0 | ✅ 主构造函数（Primary Constructors） | DynamicProxy：`SkippedAttributeFullNames` 过滤 `[PrimaryConstructorParameters]`；构造函数参数通过 `base()` 正确转发；SG：`CompilerGeneratedAttributeNames` 过滤该属性，`EmitClassConstructors` 使用 `EmitParameterDecl` 转发参数（含 `params`/`ref`/`out`/`in`） | `AttributeNodeFactory.cs:22`；`ProxyEmitter.cs:1050,383-412` |
| C# 13.0 | ✅ params 集合（params IEnumerable\<T\>） | DynamicProxy：`[ParamCollection]` 属性随自定义属性自动转发（不在跳过列表中）；SG：`EmitParameterDecl` 检测 `IsParams` 并 emit `params` 关键字，编译器据此合成 `ParamCollectionAttribute`（不显式转发，避免 CS0674）；byref-like params（如 `ReadOnlySpan<T>`）报告 ACSG009 并跳过生成 | `ProxyEmitter.cs:893-963` |
| C# 9.0 | ✅ init-only 属性设置器 | DynamicProxy：`PropertyNode` 新增 `RequiredCustomModifiers`/`OptionalCustomModifiers`，`ILEmitVisitor` 通过 `DefineProperty`/`DefineMethod`/`DefineConstructor` 传递 custom modifiers 保持 init-only modreq 元数据；SG：`ProxyEmitter` 检测 `IsInitOnly` 生成 `init` 关键字，非 override 场景使用反射直接调用避免编译限制 | `PropertyNode.cs`；`ILEmitVisitor.cs`；`ProxyEmitter.cs` |
| C# 11.0 | ✅ required 成员 | DynamicProxy：通过 custom modifiers 保留 required 元数据；SG：`ProxyEmitter` 检测 `IsRequired` 生成 `required` 关键字，`EmitConstructorAttributes` 转发 `[SetsRequiredMembers]` 属性到代理构造函数 | `ProxyEmitter.cs`；`ILEmitVisitor.cs` |
| C# 13.0 | ✅ 部分属性（Partial Properties） | DynamicProxy：`PropertyNode` 新增 `IsPartial` 标志，`ClassProxyAstBuilder`/`InterfaceImplAstBuilder` 设置该标志；SG：`ProxyEmitter` 通过语法分析检测 partial 关键字，为抽象类声明-only partial 属性生成 stub 访问器，接口和非抽象类使用正常委托代码 | `PropertyNode.cs`；`ClassProxyAstBuilder.cs`；`InterfaceImplAstBuilder.cs`；`ProxyEmitter.cs` |
| C# 10.0 | ✅ 插值字符串处理器（Interpolated String Handlers） | 自定义插值处理器 struct（非 ref struct）作为参数/返回值时，通过 `Sync` 返回路径正确处理；覆盖 `$"..."` 插值 lowering（编译器生成 handler 后传入代理方法）和 `[InterpolatedStringHandlerArgument]` 参数转发；拦截器链可检查和替换 struct 返回值 | `InterpolatedStringHandlerAndIndexRangeParityTests.cs` |
| C# 8.0 | ✅ System.Index / System.Range | `Index`/`Range` 作为普通 struct，通过 `Sync` 返回路径正确处理；参数和返回值均正确转发；拦截器链可检查和替换返回值 | `InterpolatedStringHandlerAndIndexRangeParityTests.cs` |
| C# 9.0 | ✅ record 类型 | 两套引擎均支持 record class 类代理 + `with` 表达式；SG 生成 `record class` 代理，DynamicProxy 手动实现 `<Clone>$`/`<>Copy`。已知差异见 [Record 类型支持](./record-support.md)（相等性、init setter、派生 record `<Clone>$` 限制） | `RecordTypeUtils.cs`；`ClassProxyAstBuilder.cs`；`ILEmitVisitor.cs`；`ProxyEmitter.cs` |
| C# 7.0 | ✅ ref / ref readonly 返回 | 新增 `ReturnKind.RefSync`；值语义管线下将返回值物化到 `StrongBox<T>`，返回 `ref box.Value`；未拦截路径保持真 ref 别名；`MethodReflector.EmitReturn` 处理 byref 返回解引用 | `ReturnKind.cs`；`MethodBodyFactory.cs`；`ILEmitVisitor.cs`；`ProxyEmitter.cs`；`MethodReflector*.cs` |

## 3. 需要适配的特性

### 3.1 P0 — 已完成适配与仍待处理的特性

#### 3.1.1 ✅ 异步流（IAsyncEnumerable\<T\> / IAsyncDisposable）— C# 8.0 — 已适配

**实现结果**

- `ReturnKind` 新增 `AsyncEnumerable`；DynamicProxy 和 Source Generator 分别调用 `IAspectActivator.InvokeAsyncEnumerable<T>`。
- 异步流保持惰性：拦截器链和目标方法在首次枚举时执行；流完成、取消或枚举期间发生异常时，`AspectContext` 都会释放。
- `IAsyncDisposable.DisposeAsync()` 返回 `ValueTask`，已通过既有 `ValueTask` 代理路径完成拦截。
- `netstandard2.0` 目标引入 `Microsoft.Bcl.AsyncInterfaces`，以公开 C# 8 异步接口。

**验证覆盖**

- DynamicProxy：异步流枚举、枚举期异常传播与 `IAsyncDisposable.DisposeAsync()` 拦截。
- Source Generator：与 DynamicProxy 的异步流返回类型行为一致性。

**涉及实现**：`IAspectActivator.cs`、`AspectActivator.cs`、`MethodBodyFactory.cs`、`ILEmitVisitor.cs`、`ProxyEmitter.cs`、`InterceptUtils.cs`。

---

#### 3.1.2 ✅ init-only 属性设置器 — C# 9.0 — 已适配

**实现结果**

- DynamicProxy：`PropertyNode` 新增 `RequiredCustomModifiers`/`OptionalCustomModifiers` 字段，从 `PropertyInfo.GetRequiredCustomModifiers()`/`GetOptionalCustomModifiers()` 读取 init-only 的 modreq 元数据。
- DynamicProxy：`ILEmitVisitor.VisitProperty` 通过 `DefineProperty` 重载传递 custom modifiers；`VisitMethod` 和 `EmitClassProxyCtor` 也通过 `DefineMethod`/`DefineConstructor` 传递参数和返回值的 custom modifiers。
- Source Generator：`ProxyEmitter` 检测 `accessor.IsInitOnly`，生成 `init` 关键字而非 `set`；在非 override 场景使用 `forceReflectiveDirectCall`（通过反射调用 init 访问器），避免直接调用 init 访问器的编译限制。
- 新增 `GetRequiredCustomModifiers`/`GetOptionalCustomModifiers`/`GetConstructorParameterCustomModifiers` 辅助方法，带 try-catch 容错。

**验证覆盖**

- `InitRequiredMembersParityTests`（双引擎）：验证代理属性保留 init-only modreq 元数据、拦截器正常工作、接口代理（有/无 target）均保留 init-only 元数据。

**涉及实现**：`PropertyNode.cs`、`ClassProxyAstBuilder.cs`、`InterfaceImplAstBuilder.cs`、`ILEmitVisitor.cs`、`ProxyEmitter.cs`。

---

#### 3.1.3 ✅ required 成员 — C# 11.0 — 已适配

**实现结果**

- DynamicProxy：通过 custom modifiers（`RequiredCustomModifiers`/`OptionalCustomModifiers`）保留 `required` 属性的元数据，代理属性在反射时可检测到 `IsRequired`。
- Source Generator：`ProxyEmitter` 检测 `prop.IsRequired`，在属性声明前生成 `required` 关键字；新增 `EmitConstructorAttributes` 方法，检测并转发 `[SetsRequiredMembers]` 属性到代理构造函数，使对象初始化器中可设置 `required` 成员。
- `required` 属性通常与 `init` 联动，两者适配已合并完成。

**验证覆盖**

- `InitRequiredMembersParityTests`（双引擎）：验证代理属性保留 `required` 元数据（`IsRequiredProperty` 检查）、代理构造函数带有 `[SetsRequiredMembers]` 属性、可通过对象初始化器设置 `required` 属性。

**涉及实现**：`ProxyEmitter.cs`、`ILEmitVisitor.cs`、`PropertyNode.cs`。

---

#### 3.1.4 record 类型 — C# 9.0

**问题描述**

Record 默认 `sealed`，且合成了 `Equals`、`GetHashCode`、`Deconstruct`、`<>Copy` 等成员。当前处理：

- Source Generator：拒绝 sealed 非抽象类型（`IsSealed && !IsAbstract` → ACSG003 诊断）
- DynamicProxy：**无 sealed 检查**，运行时崩溃

**影响范围**

- `AspectCoreProxyGenerator.cs:105` 拒绝 sealed 类型
- `ILEmitVisitor.cs` 无 sealed 类型检查
- Record 合成成员（`<>Copy`、`with` 表达式）无特殊处理
- `record struct`（值类型 record）无处理

**适配方案**

1. DynamicProxy 增加 sealed 类型检查（与 SG 对齐），提前报错而非运行时崩溃
2. 对于 `record class`（非 sealed 场景）：正确转发合成成员，`with` 表达式需要 `<>Copy` 方法被代理
3. 对于 `record struct`：值类型代理需要额外处理（装箱/拆箱）
4. 考虑支持 `record` 接口代理（record interface 不是 sealed）
5. Record 的 `init` 属性需与 init 适配联动

**适配难度**：⭐⭐⭐⭐

---

#### 3.1.5 ref struct — C# 7.2 ⚠️ 部分适配

ref struct（如 `Span<T>`、`ReadOnlySpan<T>`）有 CLR 强制限制：不能装箱、不能作为接口实现、不能作为类字段、不能用于 `async`/`iterator` 方法。要区分两种独立场景：

**(a) ref struct 作为代理目标类型 — ✅ 已拒绝**

- DynamicProxy：`ProxyTypeGenerator` 入口检查 `type.IsByRefLike`（`RejectRefStruct`），拒绝把 ref struct 作为代理目标并提前报错，而非运行时崩溃。
- Source Generator：`AspectCoreProxyGenerator` 对 `IsRefLikeType` 的候选类型报告 ACSG008 并跳过生成。

**(b) 被拦截方法中的 byref-like 参数/返回值 — ❌ 仍不支持**

- 只有**未命中拦截的直连路径**能正确传递 ref struct 参数/返回值（IL 直接透传、SG 直接调用）。
- 一旦方法进入**拦截路径**，两套引擎都会把参数装进 `object[]`：DynamicProxy 的 `EmitInitializeMetaData` 对每个参数 `EmitConvertToObject`（`ILEmitVisitor.cs`），SG 的 `EmitArgumentsArray` 生成 `new object[]{...}`（`ProxyEmitter.cs`）。ref struct 无法装箱到 `object`，因此运行时会抛 `InvalidProgramException`（例如对带拦截器的 `int Length(Span<int>)` 实测即抛此异常）。
- SG 目前只对 `params` 的 byref-like 参数显式报告 ACSG009（见 3.2.2）；非 `params` 的 `Span<T>` 参数在被拦截方法里没有专门诊断，会在运行时失败。

**结论**：ref struct 代理目标已被安全拒绝；但「被拦截方法的 byref-like 参数/返回值」尚未支持，是已知限制。

---

#### 3.1.6 ✅ ref 返回方法 — C# 7.0 — 已适配

**实现结果**

- `ReturnKind` 新增 `RefSync`（SG 侧 `ReturnKindKind.RefSync`）。`MethodBodyFactory.DetermineReturnKind` 通过 `ReturnType.IsByRef` 检测运行时 ref 返回；SG 通过 `IMethodSymbol.RefKind`（`Ref`/`RefReadOnly`）检测。
- 拦截器管线是值语义的（`AspectContext.ReturnValue` 是 `object`），因此被拦截的 ref 返回值会被物化到 `StrongBox<T>`，代理方法返回 `ref box.Value`——托管指针指向堆槽，方法返回后依然有效。
  - DynamicProxy：`ILEmitVisitor.VisitAspectActivatorBody` 用 `newobj StrongBox<T>` + `ldflda Value` 返回引用；`EmitReturnValue` 用元素类型调用 `Invoke<TElement>`。
  - Source Generator：`EmitProxyInvokeBody` 生成 `new StrongBox<T>()` 并 `return ref __refBox.Value`；方法签名前置 `ref`/`ref readonly`。
- 未命中拦截的方法走原有直连路径（DynamicProxy `DirectDelegationBody` / SG `return ref {directCall}`），保持**真 ref 别名**语义。
- `MethodReflector`（拦截时调用目标的动态方法）新增 ref 返回处理：`EmitReturn` 对 `ReturnType.IsByRef` 先 `ldind`/`ldobj` 解引用再按值类型装箱。
- SG 无 target 接口 stub：ref 返回 stub 返回 `ref` 到静态默认槽；泛型 ref 返回 stub 抛 `NotSupportedException`。
- `RuntimeAspectContext.Break()` 对 ref 返回类型先 `GetElementType()` 再取默认值，避免 `T&` 默认值 unbox 失败。

**行为边界**

- ✅ 读：`ref x = ref proxy.Foo()` 读到正确值（含拦截器替换后的值）。
- ✅ 拦截器可检查/替换返回值。
- ✅ 返回的 ref 可写（指向 StrongBox 堆槽）。
- ⚠️ 被拦截时通过返回 ref 的写入**不会**回流到目标对象的原始存储——这是值语义管线的固有限制，与既有 ref/out 参数拷回语义一致。
- `ref readonly` 返回按只读处理。

**验证覆盖**

- `RefReturnParityTests`（双引擎 × 4 TFM）：类代理 ref/ref readonly/引用类型返回读值、拦截器替换、返回 ref 可写、接口代理（有 target）ref 返回 + 拦截。
- `RefReturnScenarios`（E2E，运行时引擎）：ref 返回读值+拦截、拦截器替换、ref readonly 读值。

**涉及实现**：`ReturnKind.cs`、`MethodBodyFactory.cs`、`ILEmitVisitor.cs`、`AspectContext.Runtime.cs`、`MethodUtils.cs`、`MethodReflector.cs`/`.Call.cs`/`.Static.cs`、`ProxyEmitter.cs`。

**适配难度**：⭐⭐⭐⭐

### 3.2 P1 — 功能缺失但不导致崩溃

#### 3.2.1 ✅ 主构造函数（Primary Constructors）— C# 12.0 — 已适配

**问题描述**

当前构造函数转发逻辑用 `GetParameters()` 通用处理，不感知主构造函数语义。对于 class 的主构造函数，参数需要被 `base()` 正确转发；对于 record 的主构造函数，还涉及 `init` 属性和 `with` 表达式。

**影响范围**

- `ClassProxyCtorFromBase` 虽然转发参数，但不处理主构造函数参数被属性捕获的情况
- `ProxyEmitter.EmitClassConstructors` 使用 `: base(...)` 转发，但不感知主构造函数

**适配方案**

1. 检测主构造函数（`ConstructorInfo` 上的 `[PrimaryConstructorParameters]` 或类型特征）
2. 确保主构造函数参数被正确转发到 `base()`
3. 对于 record 主构造函数，处理 `init` 属性和 `with` 表达式的 `<>Copy`
4. 测试主构造函数参数与属性初始化器的交互

**适配难度**：⭐⭐⭐

**✅ 已实现**：

| 引擎 | 改动 | 涉及文件 |
|------|------|---------|
| DynamicProxy | `AttributeNodeFactory.SkippedAttributeFullNames` 新增 `PrimaryConstructorParametersAttribute`，避免编译器生成属性被复制到代理类型 | `AttributeNodeFactory.cs:22` |
| DynamicProxy | `ClassProxyAstBuilder.BuildConstructors` 已正确转发主构造函数参数到 `base()`（原有逻辑无需修改） | `ClassProxyAstBuilder.cs:84-125` |
| Source Generator | `CompilerGeneratedAttributeNames` 新增 `PrimaryConstructorParametersAttribute` | `ProxyEmitter.cs:1050` |
| Source Generator | `EmitClassConstructors` 使用 `EmitParameterDecl` 转发参数，保持 `params`/`ref`/`out`/`in` 语义 | `ProxyEmitter.cs:383-412` |
| 测试 | 新增 `PrimaryConstructorTests`（DynamicProxy）和 `PrimaryConstructorAndParamsCollectionParityTests`（双引擎），覆盖 class/record 主构造函数 + 拦截器 | `tests/AspectCore.Core.Tests/` |

---

#### 3.2.2 ✅ params 集合（params IEnumerable\<T\>）— C# 13.0 — 已适配

**问题描述**

无处理。`params` 集合参数在方法签名中有 `[ParamCollection]` 属性，代理方法需要正确转发该属性并保持 `params` 语义。

**影响范围**

- `ParameterNodeFactory` 不检查 `[ParamCollection]` 属性
- IL 发射不转发 `[ParamCollection]` 属性
- SG 不生成 `params` 修饰符

**适配方案**

1. `ParameterNode` 增加 `IsParamsCollection` 标志
2. 检测 `[ParamCollection]` 属性并转发
3. IL 中使用 `SetCustomAttribute` 转发 `[ParamCollection]`
4. SG 中生成 `params` 修饰符

**适配难度**：⭐⭐

**✅ 已实现**：

| 引擎 | 改动 | 涉及文件 |
|------|------|---------|
| DynamicProxy | `[ParamCollection]` 属性不在 `SkippedAttributeFullNames` 中，因此通过 `ParameterNodeFactory.FromParameterInfo` → `AttributeNodeFactory.FromCustomAttributes` 自动转发；`ILEmitVisitor` 通过 `SetCustomAttribute` 设置到参数 | `ParameterNodeFactory.cs:35-59`；`ILEmitVisitor.cs:254-283` |
| Source Generator | `EmitParameterDecl` 检测 `p.IsParams` 并 emit `params` 关键字（覆盖 `params IEnumerable<T>`、`params T[]`）；`params ReadOnlySpan<T>` 等 byref-like 参数报告 ACSG009，避免后续 `object[]` 装箱错误 | `ProxyEmitter.cs:893-963` |
| Source Generator | 不显式转发 `ParamCollectionAttribute`：编译器在 `params` 集合参数上自动合成该属性，显式发射会导致 CS0674 | `ProxyEmitter.cs:1130-1142` |
| Source Generator | `EmitClassConstructors` 构造函数参数也使用 `EmitParameterDecl`，保持 `params` 语义 | `ProxyEmitter.cs:394` |
| 测试 | 新增 `ParamsCollectionTests`（DynamicProxy）和 `PrimaryConstructorAndParamsCollectionParityTests`（双引擎），覆盖 `params IEnumerable<int>`、`params string[]` + 拦截器 | `tests/AspectCore.Core.Tests/` |

> **已知边界**：`params ReadOnlySpan<T>` 属于 byref-like 的 `params` 参数（ref struct，见 3.1.5），SG 侧报告 ACSG009 并跳过生成——因为 ref struct 无法通过 `object[]` 参数数组传递。这是当前的已知限制，不在本次 params 集合适配范围内。

---

#### 3.2.3 ref 字段 / scoped 修饰符 — C# 11.0 ✅ 已适配

**背景**

ref struct 中的 `ref` 字段和 `scoped` 关键字会影响参数传递；若不保持 `scoped` 修饰符，可能在 `ref struct` 上下文中生成无效签名。

**实现结果**

- Source Generator：`EmitParameterDecl` 检测编译器生成的 `[ScopedRefAttribute]`（因 Roslyn 4.10.0 无 `IParameterSymbol.IsScoped`），在 `ref`/`out`/`in` 前 emit `scoped` 修饰符，保持参数语义。
- ref struct 类型本身不作为代理目标（与 3.1.5 一致，DynamicProxy 拒绝、SG 报 ACSG008）。

**验证覆盖**：`RefStructAndScopedParityTests`（双引擎）覆盖 `scoped ref`/`scoped in` 参数的转发与拦截。

---

#### 3.2.4 ✅ 部分属性（Partial Properties）— C# 13.0 — 已适配

**实现结果**

- DynamicProxy：`PropertyNode` 新增 `IsPartial` 标志；`ClassProxyAstBuilder` 和 `InterfaceImplAstBuilder` 在构建属性节点时设置该标志（通过 `MethodBase.IsPartialMethod` 多义性 polyfill 兼容 pre-.NET 9.0）。
- Source Generator：`ProxyEmitter` 通过语法分析检测 `partial` 关键字修饰符；为抽象类中声明-only 的 partial 属性访问器生成 stub 访问器（无实现体），跳过 meta 字段（无需反射分发）；接口和非抽象类使用正常委托代码。

**验证覆盖**

- DynamicProxy：类代理、接口代理（有/无 target）、只读 partial 属性 + 拦截器。
- Source Generator：与 DynamicProxy 的 partial 属性行为一致性。
- E2E：6 个 E2E 测试覆盖 partial 属性场景（类代理、接口代理 + target、只读、混合访问器）。

**涉及实现**：`PropertyNode.cs`、`ClassProxyAstBuilder.cs`、`InterfaceImplAstBuilder.cs`、`ProxyEmitter.cs`。

---

#### 3.2.5 ✅ 插值字符串处理器（Interpolated String Handlers）— C# 10.0 — 已适配

**实现结果**

- 自定义插值处理器 struct（非 ref struct）作为参数和返回值时，通过 `Sync` 返回路径正确处理：DynamicProxy 使用 `Invoke<TResult>` 泛型方法，Source Generator 生成直接委托代码。
- 拦截器链可正确检查和替换 struct 返回值（`ctx.ReturnValue` 为装箱后的 struct）。
- `DefaultInterpolatedStringHandler` 是编译器内部类型，不作为普通方法参数/返回类型使用；自定义 handler struct 覆盖了实际使用场景。
- 带 `[InterpolatedStringHandlerArgument]` 的 handler 参数：编译器在调用点使用 `$"..."` 语法时进行 lowering（构造 handler + `AppendLiteral`/`AppendFormatted`），代理方法正确转发编译器生成的 handler struct。

**验证覆盖**

- `InterpolatedStringHandlerAndIndexRangeParityTests`（双引擎）：
  - 自定义 handler struct 作为返回值的透传和拦截器替换；
  - 自定义 handler struct 作为参数的转发；
  - **真正的 `$"..."` 插值 lowering**：调用 `proxy.HandlerToUpper($"Test{number}")`，验证编译器生成的 handler 被代理正确转发；
  - **`[InterpolatedStringHandlerArgument]`**：调用 `proxy.FormatWithCategory("INFO", $"Value is {value}")`，验证带关联参数的 handler 参数被正确转发。

**涉及实现**：无需修改引擎代码 — `Sync` 返回路径已正确处理 struct 类型返回值和参数。

---

#### 3.2.6 ✅ System.Index / System.Range — C# 8.0 — 已适配

**实现结果**

- `Index` 和 `Range` 作为普通 struct，通过 `Sync` 返回路径正确处理。
- `Index`/`Range` 参数被正确转发到目标方法（`Index.GetOffset`、`Range.GetOffsetAndLength` 等方法调用正常工作）。
- 拦截器链可正确检查和替换 `Index`/`Range` 返回值。

**验证覆盖**

- `InterpolatedStringHandlerAndIndexRangeParityTests`（双引擎）：`Index` 返回值透传+拦截、`Range` 返回值透传+拦截、`Index` 参数转发、`Range` 参数转发、`Index`/`Range` 返回值的拦截器替换。

**涉及实现**：无需修改引擎代码 — `Sync` 返回路径已正确处理 struct 类型返回值和参数。

### 3.3 P2 — 编译时注解，影响极小或无需改动

以下特性均为编译时注解或编译器内部行为，不影响代理 IL 的核心语义。但两套引擎在属性转发上的差异（DynamicProxy 全量复制 vs Source Generator 完全不转发）导致了不同的适配需求。

#### 当前属性转发机制对比

| 维度 | DynamicProxy (IL emit) | Source Generator |
|------|----------------------|-----------------|
| 类型属性 | 全量复制实现类型属性 + 标记属性（`ClassProxyAstBuilder.cs:42-49`） | 仅 emit `[NonAspect]` + `[Dynamically]`（`ProxyEmitter.cs:52-53,142-143`） |
| 方法属性 | 全量复制服务方法属性（`InterfaceImplAstBuilder.cs:369-373`） | 不转发 |
| 参数属性 | 全量复制参数属性（`ParameterNodeFactory.cs:45-49`） | 不转发 |
| 属性属性 | 全量复制属性属性（`ClassProxyAstBuilder.cs:243-248`） | 不转发 |
| 构造函数属性 | 全量复制构造函数属性（`ClassProxyAstBuilder.cs:102-106`） | 不转发 |
| 泛型参数属性 | 全量复制泛型参数属性（`GenericParameterNodeFactory.cs:49`） | 不转发（仅转发约束） |
| 属性过滤 | **无过滤** — `AttributeNodeFactory.FromCustomAttributes` 盲目复制所有 `CustomAttributeData` | 无过滤 — 直接不转发 |
| 泛型属性支持 | **不支持** — `BuildCustomAttribute`（`ILEmitVisitor.cs:619-648`）不处理泛型属性类型 | 不适用 |

---

#### 3.3.1 ✅ 可空引用类型（Nullable Reference Types, NRT）— C# 8.0 — 已适配

**特性说明**

NRT 通过 `[NullableContextAttribute]`（类型级别，标记默认可空性）和 `[NullableAttribute]`（成员级别，标记特定成员的可空性）来表达可空性注解。这些是编译时注解，不影响运行时 IL 语义。

**当前状态**

- **DynamicProxy**：`[NullableContext]` 和 `[Nullable]` 属性被盲目复制到代理类型/方法/参数上。由于 `AttributeNodeFactory.FromCustomAttributes` 不做任何过滤，这些编译器生成的属性会被原样转发。
  - 潜在问题：代理类型上的 `[NullableContext]` 可能与代理自身的可空性上下文冲突；`[Nullable]` 属性的构造函数参数（`byte` 或 `byte[]`）在 `BuildCustomAttribute` 中通过 `ReadAttributeValue` 读取，对于 `byte[]` 类型的数组值可能存在类型推断问题（空数组或全 null 数组）。
- **Source Generator**：完全不转发任何属性。SG 生成的代理类型没有 NRT 注解，消费者在使用代理类型时会丢失可空性信息，可能产生不必要的 CS8602/CS8603 警告。
  - 部分缓解：SG 在泛型约束中已 emit `notnull`（`ProxyEmitter.cs:970-1031`），但这仅覆盖泛型参数的 `notnull` 约束，不覆盖普通引用类型的可空性注解。

**适配方案**

| 引擎 | 改动 | 优先级 |
|------|------|--------|
| DynamicProxy | 1. 在 `AttributeNodeFactory.FromCustomAttributes` 中添加过滤逻辑，跳过 `[NullableContext]` 和 `[Nullable]` 属性（避免代理类型上的可空性上下文冲突）<br>2. 或者：保留复制但确保 `BuildCustomAttribute` 正确处理 `byte[]` 构造函数参数 | 低 |
| Source Generator | 1. 在生成的代理类型、方法、参数上 emit 对应的可空性注解（`?` 后缀）<br>2. 或在生成文件顶部添加 `#nullable enable` 并使用 `!`/`?` 标注<br>3. 最低成本方案：在生成文件中添加 `#nullable disable` 避免消费者警告 | 低 |

**测试建议**：使用 `#nullable enable` 的服务类型和接口，验证代理类型的可空性行为与原始类型一致。

---

#### 3.3.2 ✅ 泛型属性（Generic Attributes）— C# 11.0 — 已适配

**特性说明**

C# 11 允许属性类是泛型的，例如 `[SomeAttribute<T>]`。在 IL 中，泛型属性的 `AttributeType` 是一个闭合泛型类型（如 `SomeAttribute<int>`），其 `ConstructorArguments` 可能包含类型参数的值。

**当前状态**

- **DynamicProxy**：`BuildCustomAttribute`（`ILEmitVisitor.cs:619-648`）**不处理泛型属性类型**。当遇到 `[SomeAttribute<T>]` 时：
  - `data.AttributeType` 是闭合泛型类型（如 `SomeAttribute<int>`）
  - `data.Constructor` 是泛型属性类型的构造函数
  - `new CustomAttributeBuilder(data.Constructor, ...)` 可能因构造函数签名与闭合泛型类型不匹配而失败
  - `attributeTypeInfo.GetProperty(n.MemberName)` 和 `GetField(n.MemberName)` 在泛型属性类型上查找命名参数时可能返回错误结果
  - **结果**：运行时抛出 `ArgumentException` 或 `MissingMethodException`
- **Source Generator**：完全不转发任何属性，泛型属性直接丢失。

**适配方案**

| 引擎 | 改动 | 优先级 |
|------|------|--------|
| DynamicProxy | 1. 在 `BuildCustomAttribute` 中检测 `data.AttributeType.IsGenericType`<br>2. 对泛型属性，使用 `data.AttributeType`（已闭合）而非开放泛型定义<br>3. 构造 `CustomAttributeBuilder` 时使用闭合泛型类型的构造函数<br>4. 命名属性/字段查找也在闭合泛型类型上进行 | 中 |
| Source Generator | 1. 在 `ProxyEmitter` 中添加属性转发逻辑（见 3.3.7 综合方案）<br>2. 泛型属性 emit 为 `[SomeAttribute<int>]` 格式 | 低 |

**测试建议**：定义泛型属性类 `[GenericAttr<T>]`，应用于服务类型/方法/参数，验证代理生成成功且属性可通过反射读取。

---

#### 3.3.3 ✅ CallerArgumentExpression — C# 10.0 — 已适配

**特性说明**

`[CallerArgumentExpression("parameterName")]` 应用于方法参数，允许方法获取调用方传入参数时的表达式文本。例如 `void Foo(int value, [CallerArgumentExpression("value")] string expression = null)`。

**当前状态**

- **DynamicProxy**：`[CallerArgumentExpression]` 属性被盲目复制到代理方法的参数上。`BuildCustomAttribute` 能正确处理其字符串构造函数参数。
  - **问题**：代理方法的参数名可能与原始方法不同（如代理方法使用 `arg0`、`arg1`），导致 `[CallerArgumentExpression("value")]` 引用的参数名在代理方法中不存在，编译器报错 CS8917。
  - **实际情况**：AspectCore 代理方法使用原始参数名（`ParameterNodeFactory.FromConstructor`/`FromMethod` 保留原始名称），所以通常不会有问题。但如果代理方法的参数顺序与原始方法不同（如构造函数代理在前面插入了 `IAspectActivatorFactory` 参数），`CallerArgumentExpression` 引用的参数位置可能偏移。
- **Source Generator**：完全不转发参数属性，`[CallerArgumentExpression]` 丢失。代理方法的调用方无法获取表达式文本。

**适配方案**

| 引擎 | 改动 | 优先级 |
|------|------|--------|
| DynamicProxy | 1. 验证代理方法参数名与原始方法一致（当前已满足）<br>2. 对于构造函数代理（前面插入了 `IAspectActivatorFactory` 参数），确保 `[CallerArgumentExpression]` 引用的参数名仍然有效<br>3. 无需特殊处理，当前行为基本正确 | 低 |
| Source Generator | 1. 在 `ProxyEmitter.EmitParameterDecl` 中检测并转发 `[CallerArgumentExpression]` 属性<br>2. 生成格式：`[CallerArgumentExpression("paramName")] ref/out/in Type paramName`<br>3. 确保引用的参数名与 SG 生成的参数名一致 | 低 |

**测试建议**：方法参数带 `[CallerArgumentExpression]` 的接口/类，验证代理调用后表达式文本正确传递。

---

#### 3.3.4 ✅ 集合表达式（Collection Expressions）— C# 12.0 — 已适配

**特性说明**

集合表达式（如 `[1, 2, 3]`）编译为对 `[CollectionBuilder]` 属性指定的 builder 方法的调用。`[CollectionBuilder]` 应用于类型，指定该类型支持集合表达式初始化。

**当前状态**

- **DynamicProxy**：`[CollectionBuilder]` 属性被盲目复制到代理类型上。`BuildCustomAttribute` 能正确处理其两个字符串构造函数参数（`Type builderType` 和 `string methodName`）。
  - **问题**：`[CollectionBuilder]` 的构造函数参数是 `Type` 类型（`builderType`），`ReadAttributeValue` 需要正确处理 `Type` 类型的属性值。当前 `ReadAttributeValue` 可能不支持 `Type` 类型的属性值。
- **Source Generator**：完全不转发类型属性，`[CollectionBuilder]` 丢失。代理类型不支持集合表达式初始化。

**适配方案**

| 引擎 | 改动 | 优先级 |
|------|------|--------|
| DynamicProxy | 1. 在 `BuildCustomAttribute` / `ReadAttributeValue` 中确认 `Type` 类型属性值的处理（`CustomAttributeTypedArgument.Value` 对 `Type` 类型返回 `Type` 对象，应能直接传递）<br>2. 当前行为基本正确，需验证 `[CollectionBuilder]` 的 `Type` 参数是否被正确复制 | 低 |
| Source Generator | 1. 在 `ProxyEmitter` 中转发类型级 `[CollectionBuilder]` 属性<br>2. 生成格式：`[global::System.Runtime.CompilerServices.CollectionBuilder(typeof(BuilderType), "MethodName")]` | 低 |

**测试建议**：带 `[CollectionBuilder]` 的服务类型，验证代理类型也支持集合表达式初始化。

---

#### 3.3.5 ✅ Experimental 属性 — C# 12.0 — 已适配

**特性说明**

`[Experimental("DiagnosticId")]` 标记类型/成员为实验性的，编译器在使用时发出指定的诊断警告。这是一个纯编译时注解。

**当前状态**

- **DynamicProxy**：`[Experimental]` 属性被盲目复制。`BuildCustomAttribute` 能正确处理其字符串构造函数参数。
  - **效果**：代理类型也会被标记为实验性的，消费者使用代理时也会收到实验性警告。这是**正确行为**。
- **Source Generator**：完全不转发，`[Experimental]` 丢失。代理类型不触发实验性诊断，消费者可能在不知情的情况下使用实验性功能。

**适配方案**

| 引擎 | 改动 | 优先级 |
|------|------|--------|
| DynamicProxy | 无需改动，当前行为正确（盲目复制已包含 `[Experimental]`） | — |
| Source Generator | 1. 在 `ProxyEmitter` 中转发类型级和方法级 `[Experimental]` 属性<br>2. 生成格式：`[global::System.Diagnostics.CodeAnalysis.Experimental("DiagnosticId")]` | 低 |

**测试建议**：带 `[Experimental]` 的服务类型，验证 SG 生成的代理类型也带有该属性。

---

#### 3.3.6 ✅ 类型参数属性（Attributes on Type Parameters）— C# 13.0 — 已适配

**特性说明**

C# 13 允许在类型参数上直接应用属性，如 `interface IFoo<[SomeAttr] T>` 或 `void Bar<[SomeAttr] T>()`. 属性应用于泛型参数本身，而非泛型约束。

**当前状态**

- **DynamicProxy**：泛型参数属性通过 `GenericParameterNodeFactory.cs:49` 全量复制，`ILEmitVisitor.cs:598-599,614-615` 在 `GenericParameterBuilder` 上 `SetCustomAttribute`。
  - **问题**：与泛型属性（3.3.2）相同的问题 — 如果类型参数上的属性本身是泛型属性，`BuildCustomAttribute` 不支持。
  - **另一个问题**：`AttributeNodeFactory.FromCustomAttributes` 不做过滤，编译器生成的属性（如 `[NullableContext]`）也会被复制到泛型参数上。
- **Source Generator**：`EmitGenericConstraints`（`ProxyEmitter.cs:970-1031`）仅转发泛型约束（`class`、`struct`、`unmanaged`、`notnull`、`new()`、基类型、接口），**不转发泛型参数属性**。

**适配方案**

| 引擎 | 改动 | 优先级 |
|------|------|--------|
| DynamicProxy | 1. 确认泛型参数属性转发逻辑正确（当前基本正确）<br>2. 修复泛型属性支持问题（见 3.3.2）<br>3. 可选：过滤编译器生成的属性（如 `[NullableContext]`） | 低 |
| Source Generator | 1. 在 `EmitGenericConstraints` 中增加泛型参数属性的 emit<br>2. 生成格式：`void Bar<[SomeAttr] T>() where T : class`<br>3. 属性应放在类型参数名前，约束放在 `where` 子句中 | 低 |

**测试建议**：带类型参数属性的泛型接口/方法，验证代理类型的泛型参数上属性可通过反射读取。

---

#### 3.3.7 ✅ 方法组自然类型（Method Group Natural Type）— C# 13.0 — 已适配

**特性说明**

C# 13 改进了方法组到委托类型的转换，使得方法组在更多上下文中具有"自然类型"，减少显式类型转换。这是纯编译器行为，不产生新的 IL 结构或属性。

**当前状态**

- **DynamicProxy**：无影响。代理方法的签名与原始方法一致，方法组转换由消费者代码的编译器处理。
- **Source Generator**：无影响。同上。

**适配方案**

无需任何改动。此特性完全在 C# 编译器层面实现，不涉及运行时 IL 或属性。

---

#### 3.3.8 Source Generator 属性转发综合方案

由于 Source Generator 当前**完全不转发任何自定义属性**，上述 3.3.1~3.3.6 中所有"SG 不转发"的问题都需要一个综合解决方案：

**方案 A：选择性属性转发（推荐）**

在 `ProxyEmitter` 中新增属性转发逻辑，仅转发"安全"的属性：

```csharp
// ProxyEmitter.cs 新增方法
private static void EmitAttributes(StringBuilder sb, ISymbol symbol, string indent, AttributeTarget target)
{
    foreach (var attr in symbol.GetAttributes())
    {
        // 跳过编译器生成的属性
        if (IsCompilerGeneratedAttribute(attr)) continue;
        
        // 跳过 AspectCore 自身的标记属性
        if (IsAspectCoreMarkerAttribute(attr)) continue;
        
        //  emit 属性
        sb.Append(indent).Append('[').Append(FormatAttribute(attr)).AppendLine("]");
    }
}
```

需要转发的属性白名单（P2 相关）：
- `[Experimental]`
- `[CollectionBuilder]`
- `[CallerArgumentExpression]`
- `[Nullable]` / `[NullableContext]`（或用 `#nullable enable` 替代）
- 类型参数属性（非编译器生成的）

需要过滤的属性黑名单：
- `[CompilerGenerated]`
- `[NonAspect]`、`[Dynamically]`（AspectCore 自身标记）
- `[AspectCoreGenerateProxy]`
- 所有 `AspectCore.DynamicProxy.*` 命名空间下的属性

**方案 B：全量属性转发**

类似 DynamicProxy 的行为，将所有非编译器生成属性转发到 SG 生成的代理类型上。优点是行为一致；缺点是可能引入不必要的属性（如 `[Obsolete]` 会导致代理使用者也收到警告）。

**建议**：采用方案 A（选择性转发），优先实现 `[Experimental]`、`[CollectionBuilder]`、`[CallerArgumentExpression]` 三个属性的转发。

#### 3.3.9 DynamicProxy 属性过滤综合方案

DynamicProxy 当前盲目复制所有属性，需要增加过滤逻辑以避免问题：

```csharp
// AttributeNodeFactory.cs 新增过滤逻辑
public static List<AttributeNode> FromCustomAttributes(IEnumerable<CustomAttributeData> attributeDataList)
{
    var result = new List<AttributeNode>();
    foreach (var data in attributeDataList)
    {
        // 跳过编译器生成的属性
        if (IsCompilerGeneratedAttribute(data.AttributeType)) continue;
        
        // 跳过无法安全重建的属性（如泛型属性暂不支持）
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

**建议**：优先过滤 `[CompilerGenerated]` 属性族，然后修复泛型属性支持（3.3.2）。

## 4. 无需适配的特性

以下特性为纯语法糖，编译为标准 IL，AOP Emit 层无需感知：

| C# 版本 | 特性 | 原因 |
|---------|------|------|
| C# 6 | 字符串插值、nameof、null-conditional、异常过滤器、using static、表达式体成员、自动属性初始化器、索引初始化器 | 均编译为标准方法调用/属性访问 |
| C# 7 | 模式匹配（is/switch）、局部函数、out 变量、丢弃符、二进制字面量、数字分隔符、throw 表达式 | 编译为标准 IL 模式 |
| C# 7.1 | 异步 Main、默认字面量、推断元组元素名 | 无代理影响 |
| C# 7.3 | 元组相等、属性上的 field: 特性、改进的重载解析 | 无代理影响 |
| C# 8 | switch 表达式、using 声明、静态局部函数、??=、非托管构造类型 | 无代理影响 |
| C# 9 | 顶层语句、目标类型 new、模式匹配增强、静态匿名函数、lambda 丢弃参数 | 无代理影响 |
| C# 10 | 全局 using、lambda 改进、扩展属性模式、#line 改进 | 无代理影响 |
| C# 11 | 原始字符串字面量、列表模式、文件局部类型、UTF-8 字符串、无符号右移、宽松移位 | 无代理影响 |
| C# 12 | 任意类型 using 别名、内联数组、lambda 默认参数 | 无代理影响 |
| C# 13 | 新 lock 对象、后置 null 检查、索引器参数、新字符串转义 | 无代理影响 |

## 5. 适配优先级总览

```
紧急度 ──────────────────────────────────────────────────────────►

P0（已全部完成）          P1（功能缺失）            P2（注解/低优先）
┌─────────────────┐      ┌─────────────────┐      ┌─────────────────┐
│ ✅ record 类型    │      │                 │      │ ✅ NRT 注解完善   │
│ ✅ ref return     │      │                 │      │ ✅ 泛型属性转发   │
│                  │      │                 │      │ ✅ CallerArgument │
│                  │      │                 │      │ ✅ 集合表达式       │
│                  │      │                 │      │ ✅ Experimental     │
│                  │      │                 │      │ ✅ 类型参数属性     │
│                  │      │                 │      │ ✅ 方法组自然类型   │
└─────────────────┘      └─────────────────┘      └─────────────────┘

✅ 已完成（P0）：异步流（IAsyncEnumerable<T>）和 IAsyncDisposable、init-only、required 成员、record 类型、ref/ref readonly 返回
✅ 已完成（P1）：scoped 修饰符、主构造函数、params 集合、部分属性、插值处理器、Index/Range
⚠️ 部分适配：ref struct（代理目标已拒绝；被拦截方法的 byref-like 参数/返回值仍不支持，见 3.1.5）
✅ 已完成（P2）：NRT、泛型属性、CallerArgumentExpression、集合表达式、Experimental、类型参数属性、方法组自然类型
```

## 6. 关键适配点详解

### 6.1 ✅ IAsyncEnumerable\<T\> / IAsyncDisposable — 已适配

**实现方式**：`IAsyncEnumerable<T>` 通过 `IAspectActivator.InvokeAsyncEnumerable<T>` 接入两套代理引擎。异步迭代器在调用方首次枚举时创建并执行拦截器链，随后转发目标流；无论正常完成、提前取消还是异常退出，都会在迭代器的 `finally` 中释放 `AspectContext`。`IAsyncDisposable.DisposeAsync()` 使用已有的 `ValueTask` 返回路径，因此同样可被拦截。

**实现位置**：

- DynamicProxy：`ReturnKind.AsyncEnumerable`、`ILEmitVisitor.EmitReturnValue`、`AspectActivator.InvokeAsyncEnumerable<T>`。
- Source Generator：`ReturnKindKind.AsyncEnumerable` 与 `ProxyEmitter.EmitProxyInvokeBody`。
- Windsor：`InterceptUtils` 根据 `IAsyncEnumerable<T>` 返回类型选择对应 activator。

**验证**：覆盖动态代理与源生成代理的流枚举，覆盖枚举期异常传播，以及 `IAsyncDisposable.DisposeAsync()` 的拦截。

### 6.2 ✅ init-only 设置器 — 已适配

**实现方式**：`init` 访问器通过 custom modifiers（modreq）保持其元数据。DynamicProxy 在 `DefineProperty`/`DefineMethod`/`DefineConstructor` 时传递 `RequiredCustomModifiers`/`OptionalCustomModifiers`；Source Generator 检测 `IsInitOnly` 生成 `init` 关键字，并在非 override 场景使用反射直接调用避免编译限制。

**实现位置**：

- DynamicProxy：`PropertyNode.RequiredCustomModifiers`/`OptionalCustomModifiers`、`ILEmitVisitor.VisitProperty`/`VisitMethod`/`EmitClassProxyCtor`。
- Source Generator：`ProxyEmitter.EmitPropertyDeclaration`/`EmitAccessorBody`（`IsInitOnly` 检查）、`forceReflectiveDirectCall`。

**验证**：覆盖类代理和接口代理（有/无 target）的 init-only 属性元数据保持和拦截器执行。

### 6.3 ✅ required 成员 — 已适配

**实现方式**：`required` 属性通过 `IsRequired` 检测并在 SG 中生成 `required` 关键字；构造函数通过 `EmitConstructorAttributes` 转发 `[SetsRequiredMembers]` 属性，使对象初始化器可设置 `required` 成员。DynamicProxy 通过 custom modifiers 保留 required 元数据。

**实现位置**：

- DynamicProxy：`PropertyNode` custom modifiers、`ILEmitVisitor`。
- Source Generator：`ProxyEmitter`（`IsRequired` 检查、`EmitConstructorAttributes`、`EmitAttributes`）。

**验证**：覆盖代理属性 `required` 元数据保持、代理构造函数 `[SetsRequiredMembers]` 转发、对象初始化器设置 `required` 属性。

### 6.4 record 类型

**核心挑战**：Record 默认为 sealed，合成成员（`<>Copy`、`Equals`、`GetHashCode`、`Deconstruct`）需要正确代理。

**实现要点**：

- DynamicProxy 增加 sealed 类型检查，提前报错
- 非 sealed record class：转发合成成员，`with` 表达式需要 `<>Copy` 被代理
- record struct：值类型代理需处理装箱/拆箱
- record interface：可作为接口代理目标

### 6.5 ref struct（部分适配）

**现状**：ref struct 不能装箱、不能作为类字段。分两种场景：

- **代理目标类型**：✅ 已拒绝——入口检查 `type.IsByRefLike`（DynamicProxy）/ 报 ACSG008（SG），提前报错。
- **被拦截方法的 byref-like 参数/返回值**：❌ 仍不支持——拦截路径把参数装进 `object[]`，`Span<T>` 等无法装箱，运行时抛 `InvalidProgramException`；仅未拦截的直连路径可透传。详见 3.1.5。

### 6.6 ✅ ref 返回方法 — 已适配

**核心挑战**：`ref` 返回方法返回变量引用而非值，但 AspectCore 拦截器管线是值语义的（`AspectContext.ReturnValue` 为 `object`），无法在拦截后保留对目标原始存储的真别名。

**实现方式**：新增 `ReturnKind.RefSync`。命中拦截时正常跑管线拿到值，物化到 `StrongBox<T>`，返回 `ref box.Value`（堆槽地址在方法返回后有效）；未命中拦截走直连路径保持真 ref 别名。`MethodReflector.EmitReturn` 对 byref 返回先解引用（`ldind`/`ldobj`）再装箱。

**实现位置**：

- DynamicProxy：`MethodBodyFactory.DetermineReturnKind`（`IsByRef`）、`ILEmitVisitor.VisitAspectActivatorBody`/`EmitReturnValue`、`MethodUtils.StrongBox*`、`AspectContext.Runtime.Break()`。
- Source Generator：`ReturnKind.Determine`（`RefKind`）、`ProxyEmitter.EmitProxyMethod`（签名 `ref`/`ref readonly`）、`EmitProxyInvokeBody`（`StrongBox<T>` + `return ref`）、`EmitStubMethod`（ref stub）。
- Reflection：`MethodReflector`/`.Call`/`.Static` 的 `EmitReturn`。

**行为边界**：读值与拦截器替换正常；返回的 ref 可写但被拦截时写入不回流到目标（与 ref/out 参数拷回一致）；`ref readonly` 按只读处理。

**验证**：`RefReturnParityTests`（双引擎）、`RefReturnScenarios`（E2E）。

## 7. 总结

| 类别 | 数量 | 关键项 |
|------|------|--------|
| ✅ 已适配 | 26 类 | 协变返回、async/await、IAsyncEnumerable、IAsyncDisposable、init-only、required、部分属性、插值处理器、Index/Range、泛型、Nullable\<T\>、ref/out/in 参数、ValueTuple、NRT、泛型属性、CallerArgumentExpression、集合表达式、Experimental、类型参数属性、方法组自然类型、主构造函数、params 集合、scoped 修饰符、record 类型、ref return |
| ⚠️ 部分适配 | 1 项 | ref struct：代理目标已拒绝；被拦截方法的 byref-like 参数/返回值仍不支持（见 3.1.5） |
| 🔴 P0 需适配 | 0 项 | — |
| 🟡 P1 需适配 | 0 项 | — |
| 🟢 P2 已适配 | 7 项 | ✅ NRT、泛型属性、CallerArgumentExpression、集合表达式、Experimental、类型参数属性、方法组自然类型 |
| ⚪ 无需适配 | 30+ 项 | 纯语法糖，编译为标准 IL |

**当前状态**：C# 6 ~ C# 13 需适配特性基本完成。`record` 类型（含派生 record 在 DynamicProxy 下的已知限制，详见 [Record 类型支持](./record-support.md)）与 `ref`/`ref readonly` 返回方法两项 P0 已适配。唯一遗留的已知限制是 **ref struct 作为被拦截方法的 byref-like 参数/返回值**：代理目标类型已被安全拒绝，但拦截路径的 `object[]` 装箱使 `Span<T>` 等无法通过（见 3.1.5）。

> **更新记录**：P2 全部 7 项已在 commit `2733202` 中适配完成；P1 主构造函数和 params 集合已在 commit `11ad20b` 中适配完成；P0 ref struct/scoped 已在 commit `3f8669d` 中适配完成；P0 init-only 和 required 成员已在 commit `4bf323d` 中适配完成；P0 异步流（IAsyncEnumerable/IAsyncDisposable）已在 commit `5770dbb` 中适配完成；P1 部分属性已在 commit `a748efc` 中适配完成；P1 插值处理器和 Index/Range 已在 commit `fe319f9` 中适配完成；P0 record 类型已在 commit `ea68a9c` 中适配完成；P0 ref/ref readonly 返回已在本次适配完成（`StrongBox<T>` 值语义方案 + 双引擎测试覆盖）。
