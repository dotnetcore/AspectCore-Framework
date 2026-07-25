# Source Generator 编译时引擎

Source Generator 是 AspectCore 的编译时代理引擎，基于 Roslyn 增量生成器（`IIncrementalGenerator`）在**编译期**为标注类型生成 C# 代理源码，运行时直接使用这些已编译的代理类型，无需 `Reflection.Emit`。它与 [DynamicProxy 运行时引擎](./dynamic-proxy.md) 共享同一套拦截语义，二者对比见 [两套引擎对比与选型](./engine-comparison.md)。

代码位于 `src/AspectCore.SourceGenerator/`。该项目是 `netstandard2.0` 的 Roslyn 分析器（`IsRoslynComponent`，`LangVersion=latest`），无项目引用，DLL 打包进 `analyzers/dotnet/cs`；依赖 `Microsoft.CodeAnalysis.CSharp`、`Microsoft.CodeAnalysis.Analyzers`。

## 1. 触发方式

生成由 `[AspectCore.DynamicProxy.AspectCoreGenerateProxy]` 特性触发（`AspectCoreProxyGenerator.cs:13`），支持三种放置：

- **类型级**：在具体 class 或 interface 上标注。接口需给出实现类型（`[AspectCoreGenerateProxy(typeof(Impl))]`）。
- **程序集级（当前编译）**：`[assembly: AspectCoreGenerateProxy]` 会自动发现本程序集内符合条件的类型。
- **程序集级（引用程序集）**：引用的程序集若在装配级标注，会纳入其符合条件的类型。

## 2. 增量生成流程

`AspectCoreProxyGenerator`（`AspectCoreProxyGenerator.cs:10`）在 `Initialize` 里接两路候选源并合并（`:15`）：

1. **语法快路**：`CreateSyntaxProvider` 谓词匹配「带特性列表的类型声明」，再由 `GetCandidate` 用符号的特性全名确认（`:125`）。
2. **引用程序集发现**：`CompilationProvider.SelectMany(GetReferencedAssemblyCandidates)`（`:44`）。

两路合并后 `RegisterSourceOutput` 执行 `Execute`（`:151`）：对每个候选做校验、决定接口/类代理、调用 `ProxyEmitter` 生成 `{ProxyTypeName}.g.cs`；只要有生成，就额外生成 `AspectCoreSourceGeneratedProxyRegistry.g.cs`。

### 候选过滤

- `IsProxyableClassMethod`（`:460`）：`Ordinary && !static && virtual && !sealed`，可访问性 ∈ {public, protected, protected internal}，且非 record 合成成员。
- `IsProxyableClassProperty`（`:470`）：同上（属性版）。
- 自动发现（程序集级）额外跳过含事件成员的类型、以及已显式标注的类型。

## 3. 诊断（ACSGxxx）

生成器在遇到不支持的情况时报告诊断（`Emit/GeneratorDiagnostics.cs`，类别 `AspectCore.SourceGenerator`）：

| ID | 级别 | 含义 |
|----|------|------|
| ACSG002 | Warning | 嵌套类型不支持 |
| ACSG003 | Warning | 事件成员不支持 |
| ACSG005 | Error | 无法代理 `sealed` 类型 |
| ACSG006 | Error | 类型对生成代码不可见（需 public/internal） |
| ACSG007 | Error | 类代理缺少可访问构造函数 |
| ACSG008 | Error | 无法代理 `ref struct` |
| ACSG009 | Warning | `params` 的 byref-like 参数不支持 |
| ACSG010 | Warning | byref-like 参数不支持 |
| ACSG011 | Warning | byref-like 返回值不支持 |

（ACSG001/ACSG004 为历史保留的「开放泛型类型/方法不支持」描述符；当前泛型已支持，不再主动触发。诊断标题/消息为中文。）

## 4. 代理源码生成（ProxyEmitter）

`Emit/ProxyEmitter.cs` 有两个入口：`EmitInterfaceProxy`（`:14`）与 `EmitClassProxy`（`:112`）。生成的代理是 `sealed` 类型，打上 `[NonAspect]` + `[Dynamically]`，字段包含 `_activatorFactory`、`_aspectContextFactory`、`_aspectBuilderFactory`、`_aspectConfiguration`、`_serviceProvider`、`_implementation`、`_validator` 及仅异步路径用到的 `_cachedActivator`。

- **无目标接口代理**：额外生成内部 `{proxy}__Stub` 实现接口，并提供带/不带目标的两个构造器。
- **类代理**：通过 `EmitClassConstructors` 转发真实基类构造器（跳过 record 拷贝构造器）。
- **record**：以 `sealed record class` 生成（让编译器合成拷贝构造器 / `with` 支持），非 record 用 `sealed class`。两引擎在 record 上的差异见 [Record 类型支持](./record-support.md)。
- **`__Meta` 反射缓存**：每个代理内嵌 `private static class __Meta`，缓存 `Service_*`/`Impl_*`/`Proxy_*` 的 `MethodInfo`，并打上裁剪/AOT 抑制特性。

### 返回类型分发（ReturnKindKind）

`ReturnKind.Determine`（`ProxyEmitter.cs:1515`）映射到 `ReturnKindKind`（`Void`/`Sync`/`Task`/`TaskOfT`/`ValueTask`/`ValueTaskOfT`/`AsyncEnumerable`/`RefSync`），语义与 DynamicProxy 的 `ReturnKind` 对齐。

### 内联激活（性能关键）

`EmitProxyInvokeBody`（`:858`）生成的方法体：

1. 取缓存的服务/实现/代理方法；必要时按运行时实例签名重解析实现方法。
2. `if (!ShouldIntercept(...))` → **直接调用**目标（`ref`/`ref readonly` 会保留 `ref` 前缀，维持真别名）。
3. 构造 `object[] __args` 与 `AspectActivatorContext`。
4. **同步路径完全内联激活**：直接 `CreateContext` → `GetBuilder` 取缓存管线 → `Build()` → 执行，用 `ExceptionDispatchInfo` 传播故障、`NoSyncContextScope.Run` 跑未完成 task——**跳过 `AspectActivator` 分配**。
5. **异步路径**：复用 `_cachedActivator`（`AspectActivator` 无状态可复用），调 `InvokeTask<T>`/`InvokeValueTask<T>`/`InvokeAsyncEnumerable<T>`。
6. `ref`/`out` 参数在管线后从 `__args[i]` 写回。
7. **`ref`/`ref readonly` 返回**：值语义管线结果先存入 `StrongBox<T>`，再 `return ref __refBox.Value;`（详见 [C# 语言特性适配](./language-features.md)）。

### 接口 stub

`EmitInterfaceStubMembers`/`EmitStubMethod`（`:256`）为接口及其继承接口生成最小成员：只为抽象方法生成 stub（默认接口方法省略以走 DIM）；`out` 置默认、非 void 返回 `default(T)`；`ref` 返回的非泛型 stub 返回指向 `private static` 槽的 `ref`，泛型 `ref` 返回则抛 `NotSupportedException`。

## 5. 运行时发现（RegistryEmitter）

`Emit/RegistryEmitter.cs` 生成 `AspectCoreSourceGeneratedProxyRegistry.g.cs`：

- 一个 assembly 特性 `[assembly: AspectCoreSourceGeneratedProxyRegistryAttribute(typeof(AspectCoreSourceGeneratedProxyRegistry))]`（`:17`），供运行时扫描发现。
- 一个 `public sealed class AspectCoreSourceGeneratedProxyRegistry : ISourceGeneratedProxyRegistry`，实现 `TryGetProxyType(serviceType, implementationType, kind, out proxyType)`（`:27`），内部按 kind + 服务键（泛型归一化为开放定义）逐条匹配。

运行时侧，`SourceGeneratedProxyTypeGenerator`（在 `AspectCore.Core`）通过 `ProxyEngineOptions` 启用后，`ScanRegistries` 反射扫描 `AppDomain` 各程序集上的 registry 特性并实例化，从而查表拿到编译期生成的代理类型。引擎启用与选择见 [两套引擎对比与选型](./engine-comparison.md)。

## 6. 适用性与限制

- **降低代理生成期的动态代码依赖**：代理在编译期生成，运行时**代理生成**不需 `Reflection.Emit`；配合手动注册 registry（`AddSourceGeneratedProxyRegistry<T>()`）可在无程序集扫描的场景工作。注意这不等于端到端 NativeAOT——拦截时目标调用仍经 `MethodReflector`（`DynamicMethod`），代理构造也保留 `[RequiresDynamicCode]` 路径，边界详见 [两套引擎对比与选型](./engine-comparison.md)。
- **需要显式标注**：只有带 `[AspectCoreGenerateProxy]` 的类型（或程序集级自动发现）会生成代理。
- **不支持**：`sealed` 类、`ref struct`、嵌套类型、事件成员、`params` 的 byref-like 参数（对应上表诊断）。
- 与 DynamicProxy 的行为差异集中在 record（相等性/`init` setter）与无目标接口 stub 的边界，均在对应文档说明。
