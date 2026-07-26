# Source Generator 诊断目录（ACSGxxx）

AspectCore 的 [Source Generator 编译时引擎](../architecture/source-generator.md) 在编译期为标注了 `[AspectCoreGenerateProxy]` 的类型生成代理源码。当目标类型或其成员触及生成器无法支持的形态时，生成器会报告一条诊断，编译器输出中以 `ACSGxxx` 编号呈现。

- **诊断类别**：所有诊断的 `category` 均为 `AspectCore.SourceGenerator`，默认启用。
- **编号规则**：`ACSG001`–`ACSG011` 为通用生成限制；`ACSG0101` 为 NativeAOT 专项诊断。
- **两类严重级别**：
  - **Error**：直接阻断该类型的代理生成，编译报错。纯 NativeAOT / 裁剪场景下没有可用的编译期代理，因此这些错误会阻断该类型的 AOP 能力。
  - **Warning**：跳过该类型或成员的代理生成（不阻断编译），或提示运行时会发生降级（如反射回退）。
- **本文档诊断文案取自源码** `src/AspectCore.SourceGenerator/Emit/GeneratorDiagnostics.cs`，消息模板与源码逐字一致；标题为便于阅读省略了部分诊断的 `AspectCore SourceGenerator ` 前缀。消息模板中的 `{0}`、`{1}` 是运行时填入的类型名 / 成员名占位符。

## 总览

| ID | 标题 | 级别 | 触发形态 | 状态 | NativeAOT 影响 |
|----|------|------|----------|------|----------------|
| [ACSG001](#acsg001) | 暂不支持开放泛型类型 | Warning | 开放泛型类型 | **保留（当前未发出）** | 间接 |
| [ACSG002](#acsg002) | 暂不支持嵌套类型 | Warning | 嵌套类型 | 生效 | 间接 |
| [ACSG003](#acsg003) | 暂不支持事件成员 | Warning | 含 `event` 成员 | 生效 | 间接 |
| [ACSG004](#acsg004) | 暂不支持开放泛型方法 | Warning | 开放泛型方法 | **保留（当前未发出）** | 间接 |
| [ACSG005](#acsg005) | 无法为 sealed 类型生成代理 | **Error** | `sealed` 类 | 生效 | 是（阻断） |
| [ACSG006](#acsg006) | 类型对 Source Generator 不可见 | **Error** | 非 public/internal | 生效 | 是（阻断） |
| [ACSG007](#acsg007) | 类型没有可访问的构造函数 | **Error** | 缺可访问构造函数 | 生效 | 是（阻断） |
| [ACSG008](#acsg008) | 无法为 ref struct 类型生成代理 | **Error** | `ref struct` | 生效 | 是（阻断） |
| [ACSG009](#acsg009) | 暂不支持 byref-like params 参数 | Warning | `params` 的 byref-like 参数 | 生效 | 是 |
| [ACSG010](#acsg010) | 暂不支持 byref-like 参数 | Warning | byref-like 参数 | 生效 | 是 |
| [ACSG011](#acsg011) | 暂不支持 byref-like 返回值 | Warning | byref-like 返回值 | 生效 | 是 |
| [ACSG0101](#acsg0101) | 开放泛型方法在 NativeAOT 下回退反射 | Warning | 无提示的开放泛型方法 | 生效 | 是（直接） |

> **关于"保留（当前未发出）"**：`ACSG001` 与 `ACSG004` 在 `GeneratorDiagnostics.cs` 中有描述符与工厂方法，但全仓没有任何 `ReportDiagnostic` 调用点。当前版本**开放泛型类型与开放泛型方法均已受支持**（见 `AspectCoreProxyGenerator.cs:190` 与 `ProxyEmitter.cs` 的 "Generic types/methods are supported" 注释，代理会转发泛型参数）。开放泛型方法在 NativeAOT 下的降级由 [`ACSG0101`](#acsg0101) 负责提示。因此这两条诊断当前不会触发，仅作保留。

---

## ACSG001

**暂不支持开放泛型类型** · Warning · **保留（当前未发出）**

- **消息模板**：`类型 '{0}' 为开放泛型，当前版本的 Source Generator 暂不支持生成代理。`
- **状态说明**：保留项。当前版本的开放泛型类型**受支持**——生成器会把类型参数转发给代理类型（`AspectCoreProxyGenerator.cs:190` 注释 "Generic types are supported"）。该描述符已定义但没有发出点，编译时不会出现此诊断。
- **示例**：以下开放泛型类型可正常生成代理，**不会**触发本诊断：

  ```csharp
  [AspectCoreGenerateProxy]
  public class Repository<T> { public virtual T Get(int id) => default!; }
  ```

- **修复**：无需处理。若在旧版本或历史构建中遇到，升级到支持泛型的版本即可。
- **NativeAOT 影响**：间接。作为保留项当前不影响 AOT。

## ACSG002

**暂不支持嵌套类型** · Warning

- **消息模板**：`类型 '{0}' 为嵌套类型，当前版本的 Source Generator 暂不支持生成代理。`
- **说明**：目标类型定义在另一个类型内部（`type.ContainingType is not null`）。生成器检测到后跳过该类型（`AspectCoreProxyGenerator.cs:195`，随后 `continue`）。
- **示例**：

  ```csharp
  public class Outer
  {
      [AspectCoreGenerateProxy]
      public class Inner { public virtual void Do() { } }   // 触发 ACSG002
  }
  ```

- **修复**：将需要代理的类型提升为顶层（命名空间级）类型。
- **NativeAOT 影响**：间接。该类型不会生成编译期代理，纯 AOT 场景下将缺少可用代理。

## ACSG003

**暂不支持事件成员** · Warning

- **消息模板**：`类型 '{0}' 包含事件成员 '{1}'，当前版本的 Source Generator 暂不支持生成代理。`
- **说明**：目标类型（或其继承的接口）声明了 `event` 成员。接口路径检查全部继承接口的事件（`ProxyEmitter.cs:25`），类路径检查自身事件（`ProxyEmitter.cs:122`），命中即中止该类型代理生成（`return null`）。
- **示例**：

  ```csharp
  [AspectCoreGenerateProxy(typeof(NotifierImpl))]
  public interface INotifier
  {
      event EventHandler Changed;   // 触发 ACSG003
      void Notify();
  }
  ```

- **修复**：从需要代理的类型中移除事件成员，或将事件拆分到不参与代理的类型上。
- **NativeAOT 影响**：间接。该类型不生成编译期代理。

## ACSG004

**暂不支持开放泛型方法** · Warning · **保留（当前未发出）**

- **消息模板**：`类型 '{0}' 包含开放泛型方法 '{1}'，当前版本的 Source Generator 暂不支持生成代理。`
- **状态说明**：保留项。当前版本的**开放泛型方法受支持**——代理方法会保留泛型元数（generic arity）并在调用时 `MakeGenericMethod`（`ProxyEmitter.cs` 注释 "Generic methods are supported"）。该描述符已定义但没有发出点，编译时不会出现此诊断。开放泛型方法在 NativeAOT 下的委托降级改由 [`ACSG0101`](#acsg0101) 提示。
- **示例**：以下开放泛型方法可正常生成代理，**不会**触发本诊断（但在 NativeAOT 下可能触发 `ACSG0101`）：

  ```csharp
  [AspectCoreGenerateProxy(typeof(ConverterImpl))]
  public interface IConverter { T Process<T>(T input); }
  ```

- **修复**：无需处理。若关注 NativeAOT 下的反射回退，参见 [`ACSG0101`](#acsg0101)。
- **NativeAOT 影响**：间接。作为保留项当前不影响 AOT；实际的 AOT 提示见 `ACSG0101`。

## ACSG005

**无法为 sealed 类型生成代理** · **Error**

- **消息模板**：`无法为 sealed 类型 '{0}' 生成代理。请移除 sealed 修饰符或使用接口代理。`
- **说明**：类代理通过继承目标类实现，`sealed` 类无法被继承。生成器对非抽象的 `sealed class` 报错并跳过（`AspectCoreProxyGenerator.cs:202`）。
- **示例**：

  ```csharp
  [AspectCoreGenerateProxy]
  public sealed class OrderService   // 触发 ACSG005
  {
      public virtual void Submit() { }
  }
  ```

- **修复**：移除 `sealed` 修饰符；或抽出接口并改用接口代理（`[AspectCoreGenerateProxy(typeof(OrderService))]` 标注在接口上）。
- **NativeAOT 影响**：是（阻断）。该类型不会生成任何编译期代理，纯 AOT 场景下无法对其进行 AOP。

## ACSG006

**类型对 Source Generator 不可见** · **Error**

- **消息模板**：`类型 '{0}' 对 Source Generator 不可见。请确保类型具有 public 或 internal 可访问性。`
- **说明**：目标类型或其实现类型的可访问性不足，生成的代理代码无法引用它。生成器对目标类型（`AspectCoreProxyGenerator.cs:216`）与 `[AspectCoreGenerateProxy(typeof(Impl))]` 指定的实现类型（`:264`）分别校验可见性。
- **示例**：

  ```csharp
  public interface IFoo { void Run(); }

  // 实现类型可访问性低于 public/internal，代理无法引用它
  [AspectCoreGenerateProxy(typeof(FooImpl))]   // FooImpl 不可见时触发 ACSG006
  public class FooProxyMarker : IFoo { public virtual void Run() { } }
  ```

- **修复**：将目标类型与实现类型的可访问性提升到 `public` 或 `internal`。
- **NativeAOT 影响**：是（阻断）。该类型不生成编译期代理。

## ACSG007

**类型没有可访问的构造函数** · **Error**

- **消息模板**：`类型 '{0}' 没有可访问的构造函数。类代理要求目标类型具有 public 或 protected 构造函数。`
- **说明**：类代理需要转发目标类的构造器，若目标类只有 `private` 构造函数则无法继承调用。该检查仅在类代理路径进行（`AspectCoreProxyGenerator.cs:277`）。
- **示例**：

  ```csharp
  [AspectCoreGenerateProxy]
  public class Cache
  {
      private Cache() { }          // 只有私有构造函数 → 触发 ACSG007
      public virtual object? Get(string key) => null;
  }
  ```

- **修复**：为目标类型添加 `public` 或 `protected` 构造函数。
- **NativeAOT 影响**：是（阻断）。该类型不生成编译期代理。

## ACSG008

**无法为 ref struct 类型生成代理** · **Error**

- **消息模板**：`无法为 ref struct 类型 '{0}' 生成代理。ref struct（如 Span<T>、ReadOnlySpan<T>）不能装箱、不能实现接口、不能作为类字段，因此无法进行 AOP 代理。`
- **说明**：`ref struct` 存在生命周期约束，不能装箱、不能实现接口、不能作为类字段，无法承载 AOP 代理结构。生成器检测到 `type.IsRefLikeType` 后报错并跳过（`AspectCoreProxyGenerator.cs:209`）。
- **示例**：

  ```csharp
  [AspectCoreGenerateProxy]
  public ref struct SpanHolder   // 触发 ACSG008
  {
      public void Use() { }
  }
  ```

- **修复**：不要对 `ref struct` 应用 AOP。将需要拦截的逻辑迁移到普通类 / 接口上。
- **NativeAOT 影响**：是。属于 NativeAOT 相关限制，该类型无法生成代理。

## ACSG009

**暂不支持 byref-like params 参数** · Warning

- **消息模板**：`成员 '{0}' 包含 byref-like params 参数 '{1}'，当前版本的 Source Generator 暂不支持生成代理。`
- **说明**：成员含一个 `params` 且类型为 byref-like 的参数（如 C# 13 的 `params ReadOnlySpan<T>`）。由 `NativeAotSignatureDiagnosticRules.Analyze` 识别、经 `ProxyEmitter.cs` 分发（`TryReportUnsupportedByRefLikeMembers`），命中则跳过该类型代理生成。
- **示例**：

  ```csharp
  [AspectCoreGenerateProxy(typeof(WriterImpl))]
  public interface IWriter
  {
      void Write(params ReadOnlySpan<byte> data);   // 触发 ACSG009
  }
  ```

- **修复**：改用普通数组 `params`（如 `params byte[]`）或非 byref-like 的集合参数。
- **NativeAOT 影响**：是。byref-like 类型无法进入 AspectCore 的 `object[]` 参数管道；属 NativeAOT 相关限制。

## ACSG010

**暂不支持 byref-like 参数** · Warning

- **消息模板**：`成员 '{0}' 包含 byref-like 参数 '{1}'，当前版本的 Source Generator 暂不支持生成代理。byref-like 类型（如 Span<T>、ReadOnlySpan<T>）无法进入 AspectCore 的 object[] 参数管道。`
- **说明**：成员含一个 byref-like 类型的普通参数。AspectCore 的拦截管道把参数装箱进 `object[]`，而 byref-like 类型不能装箱。识别与分发路径同 `ACSG009`。
- **示例**：

  ```csharp
  [AspectCoreGenerateProxy(typeof(ParserImpl))]
  public interface IParser
  {
      int Parse(ReadOnlySpan<char> text);   // 触发 ACSG010
  }
  ```

- **修复**：将 byref-like 参数替换为可装箱的类型（如 `string`、`byte[]`、`Memory<T>`）。
- **NativeAOT 影响**：是。属 NativeAOT 相关限制，该类型不生成代理。

## ACSG011

**暂不支持 byref-like 返回值** · Warning

- **消息模板**：`成员 '{0}' 返回 byref-like 类型 '{1}'，当前版本的 Source Generator 暂不支持生成代理。byref-like 类型（如 Span<T>、ReadOnlySpan<T>）无法进入 AspectCore 的 object ReturnValue 管道。`
- **说明**：成员返回 byref-like 类型。拦截管道以 `object ReturnValue` 承载返回值，byref-like 类型不能装箱。返回值检查先于参数检查执行（`NativeAotSignatureDiagnostic.cs:36`）。
- **示例**：

  ```csharp
  [AspectCoreGenerateProxy(typeof(BufferImpl))]
  public interface IBuffer
  {
      Span<int> Rent(int size);   // 触发 ACSG011
  }
  ```

- **修复**：将返回类型替换为可装箱的类型（如 `int[]`、`Memory<int>`）。
- **NativeAOT 影响**：是。属 NativeAOT 相关限制，该类型不生成代理。

## ACSG0101

**开放泛型方法在 NativeAOT 下回退反射** · Warning · 直接 NativeAOT 诊断

- **标题（源码原文，英文）**：`Open generic method falls back to reflection for NativeAOT`
- **消息模板（源码原文，英文）**：`Method '{0}.{1}' is an open generic. The NativeAOT delegate falls back to reflection for unclosed type parameters. Add [AspectCoreGenericHint] to specify concrete type arguments for full NativeAOT safety.`
- **说明**：开放泛型方法**可以**正常生成代理，但为其生成的 NativeAOT 委托对未闭合的类型参数会回退到反射调用。生成器对每个 `IsGenericMethod` 的方法发出此提示（接口路径 `ProxyEmitter.cs:452`，类路径 `ProxyEmitter.cs:617`）。**这是提示而非阻断**，代理仍会生成。
- **示例**：

  ```csharp
  [AspectCoreGenerateProxy(typeof(ConverterImpl))]
  public interface IConverter
  {
      T Process<T>(T input);   // 未加提示 → 触发 ACSG0101（代理仍生成）
  }
  ```

- **修复**：在方法上添加 `[AspectCoreGenericHint]`（命名空间 `AspectCore.DynamicProxy`），为常用的类型参数组合声明具体类型，生成器会为这些组合产出完全类型化的委托，消除反射回退：

  ```csharp
  using AspectCore.DynamicProxy;

  public interface IConverter
  {
      [AspectCoreGenericHint(typeof(int), typeof(string))]
      T Process<T>(T input);
  }
  ```

  该特性 `AllowMultiple = true`，可叠加多组类型参数；每组生成一个类型化委托（如上例生成 `Process<int>` 与 `Process<string>` 的委托）。

- **NativeAOT 影响**：是（直接）。这是唯一一条直接针对 NativeAOT 安全性的诊断。不处理时，未提示的类型参数在 AOT 运行时走反射，可能与裁剪 / AOT 约束冲突。

---

## 相关文档

- [Source Generator 编译时引擎](../architecture/source-generator.md) — 触发方式、增量生成流程、候选过滤
- [两套引擎对比与选型](../architecture/engine-comparison.md) — DynamicProxy vs SourceGenerator vs Auto
- [C# 语言特性适配](../architecture/language-features.md) — 各 C# 特性在 AOP Emit 中的适配情况
