# C# 9 Record 类型代理支持

> 本文档说明 AspectCore 两套 AOP 引擎在代理 C# 9 `record` 类型时的行为差异和设计取舍。

## 1. 概述

AspectCore 拥有两套 AOP 引擎：

| 引擎 | IL 生成方式 | 关键文件 |
|------|------------|---------|
| **DynamicProxy**（运行时） | `System.Reflection.Emit` + AST 架构 | `ILEmitVisitor.cs`、`ClassProxyAstBuilder.cs` |
| **Source Generator**（编译时） | Roslyn 生成 C# 源码字符串 | `ProxyEmitter.cs`、`AspectCoreProxyGenerator.cs` |

两套引擎均支持对 C# 9 `record` 类型生成类代理，并支持 `with` 表达式（record 的不可变拷贝语义）。但由于两套引擎的生成机制不同，在以下两个方面存在行为差异。

## 2. 相等性语义差异（Equality Semantics）

### 2.1 现象

- **Source Generator**：代理类生成为 `record class`（继承自服务 record 类型）。C# 编译器会自动合成基于值相等的 `Equals`/`GetHashCode`/`ToString`。
- **DynamicProxy（IL emit）**：代理类生成为普通 `class`（非 record），使用引用相等。

因此，对同一个 record 服务类型的两个代理实例 `proxyA` 和 `proxyB`：

```csharp
// Source Generator 引擎：值相等
proxyA.Equals(proxyB)  // true（当各属性值相等时）

// DynamicProxy 引擎：引用相等
proxyA.Equals(proxyB)  // false（始终为引用比较）
```

### 2.2 设计取舍

Source Generator 将代理生成为 `record class` 是必要的：只有这样 C# 编译器才会合成拷贝构造函数和 `with` 支持，使得 `with` 表达式能正确工作。其副作用是编译器同时生成了值相等的 `Equals`/`GetHashCode`/`ToString`。

DynamicProxy 引擎通过 IL emit 直接生成普通类，record 的 `with` 支持通过手动实现 `<Clone>$`/`<>Copy` 方法（使用 `MemberwiseClone`）达成，不依赖编译器合成，因此代理类保持引用相等语义。

### 2.3 建议

- 如果业务代码依赖代理实例的相等性比较，请留意两套引擎的差异。
- 建议在 record 代理场景中避免直接比较代理实例的相等性，或在使用前明确了解当前引擎的行为。
- 如需统一行为，可考虑在 Source Generator 生成的代理中显式覆盖 `Equals`/`GetHashCode` 以排除 `_activatorFactory` 等基础设施字段，但这会增加生成代码的复杂度，当前版本未采用此方案。

## 3. init-only 属性 setter 差异（IL emit init setter）

### 3.1 现象

record 类型的属性通常使用 `init` 访问器（init-only setter），以保证不可变语义。

- **Source Generator**：生成的代理属性保留 `init` 访问器（`init` 关键字直接写入生成的源码）。
- **DynamicProxy（IL emit）**：生成的代理属性 setter 为普通 `set`（而非 `init`）。

### 3.2 原因

DynamicProxy 引擎在复制属性特性时，`AttributeNodeFactory.cs` 将 `System.Runtime.CompilerServices.IsExternalInitAttribute` 列入了跳过列表。`init` 访问器的 IL 表示依赖 `modreq([IsExternalInit])` 修饰符，跳过该特性后，代理属性的 setter 从 `init` 变为 `set`。

### 3.3 影响

- `with` 表达式在编译时基于静态类型生成调用代码，运行时不强制检查 `modreq([IsExternalInit])`，因此 `with` 仍能正常工作。
- 但代理属性的 setter 从 `init` 变为 `set` 意味着外部代码可以直接修改代理属性，绕过了 record 的不可变语义。

### 3.4 建议

- 如果业务代码依赖 record 代理属性的 init-only 不可变语义，请留意 DynamicProxy 引擎下 setter 变为 `set` 的差异。
- Source Generator 引擎下 init 语义保持不变。
- 当前版本选择在文档中说明此差异，而非在 IL emit 中保留 `modreq([IsExternalInit])`，因为后者需要在 `DefineProperty`/`SetSetMethod` 之外额外处理 required modifier，增加了 IL emit 的复杂度，且 `with` 表达式功能不受影响。

## 4. override 方法代理（Override Method Proxying）

### 4.1 现象

对于派生 record 类型中声明的 `override` 方法（例如覆盖基类 `Label()` 方法），两套引擎的代理行为：

- **Source Generator**：修复后正确生成 `override` 方法的代理。
- **DynamicProxy（IL emit）**：正确生成 `override` 方法的代理。

### 4.2 原因

在 Roslyn 的 `IMethodSymbol` API 中，`override` 方法的 `IsVirtual` 属性为 `false`，`IsOverride` 属性为 `true`。Source Generator 的 `IsOverridable` 方法此前仅检查 `IsVirtual`，导致 `override` 方法被错误地过滤掉，未生成代理。

修复方案：`IsOverridable` 同时检查 `IsVirtual` 和 `IsOverride`，确保 `override` 方法被正确识别为可代理方法。

### 4.3 影响

修复后，派生 record 中的 `override` 方法（如 `Label()`）将被正确代理，拦截器可以正常工作。

## 5. 派生 record 的 `<Clone>$ 方法（DynamicProxy 限制）

### 5.1 现象

对于派生 record 类型（例如 `DerivedRecordService : RecordClassService`），DynamicProxy 引擎在生成代理时可能抛出 `TypeLoadException`：

```
Return type in method '...<Clone>$()' on type '...' is not compatible with base type method '...<Clone>$()'
```

### 5.2 原因

派生 record 的 `<Clone>$` 方法使用了协变返回类型（covariant return type），返回派生类型而非基类类型。DynamicProxy 在 `DefineMethodOverride` 时，运行时需要验证返回类型的兼容性。对于动态生成的类型，运行时对协变返回类型的处理存在限制，导致 `TypeLoadException`。

### 5.3 影响

- DynamicProxy 引擎当前不支持对派生 record 类型生成类代理（涉及 `<Clone>$` 方法的协变返回类型）。
- Source Generator 引擎不受此限制影响，可正确代理派生 record 类型。

### 5.4 建议

- 如需代理派生 record 类型，建议使用 Source Generator 引擎。
- DynamicProxy 引擎的此限制将在后续版本中研究解决方案。

## 6. 相关文件

| 文件 | 说明 |
|------|------|
| `src/AspectCore.SourceGenerator/RecordTypeUtils.cs` | record 类型检测工具方法（`IsRecord`、`IsRecordCopyMethod`、`IsRecordSynthesizedMember`） |
| `src/AspectCore.SourceGenerator/AspectCoreProxyGenerator.cs` | Source Generator 候选类型发现与过滤 |
| `src/AspectCore.SourceGenerator/Emit/ProxyEmitter.cs` | Source Generator 代理代码生成（含 `IsOverridable` override 修复） |
| `src/AspectCore.Core/DynamicProxy/ProxyBuilder/Builders/ClassProxyAstBuilder.cs` | DynamicProxy record 拷贝方法 AST 构建（含派生 record `<Clone>$` 跳过逻辑） |
| `src/AspectCore.Core/DynamicProxy/ProxyBuilder/Visitors/ILEmitVisitor.cs` | DynamicProxy IL 发射（含 `VisitRecordCloneBody`） |
| `src/AspectCore.Core/DynamicProxy/ProxyBuilder/Builders/AttributeNodeFactory.cs` | 属性特性过滤（含 `IsExternalInit` 跳过逻辑） |
| `src/AspectCore.Core/Utils/ReflectionUtils.cs` | `IsRecordCopyMethod` 扩展方法（DynamicProxy 端） |
| `tests/AspectCore.Core.Tests/EngineParity/SourceGeneratorDynamicProxyParityTests.cs` | record 代理测试（含基类、派生、泛型、init-only 属性场景） |
