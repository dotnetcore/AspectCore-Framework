# NativeAOT 上手指南

本页带你把一个使用 AspectCore 的应用发布成 **NativeAOT 原生二进制**，从空项目一路走到可运行。NativeAOT 下 AspectCore 的能力有明确边界，配置也和普通运行时不同，因此请先读完「支持边界」再动手，避免走上默认路径后在发布或运行阶段才发现不兼容。

> 本页面向使用者。如果你想理解 NativeAOT 支持背后的引擎改造与设计取舍，见[NativeAOT AOP 设计方案](../architecture/nativeaot-design.md)（面向贡献者）。

## 支持边界（先读这一节）

AspectCore 有两套生成代理的引擎，它们对 NativeAOT 的支持完全不同：

- **DynamicProxy（默认引擎）在 NativeAOT 下不可用。** 它在运行时用 `Reflection.Emit` / `DynamicMethod` 生成代理，而这类动态代码生成正是 NativeAOT 所禁止的，发布后会在运行阶段失败。
- **NativeAOT 支持仅覆盖 Source Generator 路径。** Source Generator 在编译期生成代理类型与调度委托，不依赖运行时 Emit，因此可以随 NativeAOT 一起发布并运行。

引擎与 NativeAOT 的兼容关系：

| 引擎 | NativeAOT 兼容 | 是否默认 | 说明 |
|------|----------------|----------|------|
| DynamicProxy | 否 | 是 | 运行时织入，功能最完整；依赖 Emit，AOT 下不可用。 |
| SourceGenerator | 是 | 否 | 编译期生成代理，需显式 opt-in。 |
| Auto | 条件兼容 | 否 | 优先用 SG，缺生成物时回退 DynamicProxy；在 NativeAOT 下该回退不可用，会抛异常。 |

**结论：要用 NativeAOT，必须显式把引擎切换到 Source Generator。** 保持默认（DynamicProxy）或依赖 `Auto` 的运行时回退，都会在 AOT 环境下失败。下面的步骤会完成这一切换。

## 前置条件

- **.NET 7 及以上**（NativeAOT 的最低要求）。本页示例统一用 `net9.0`，与仓库内的官方 E2E 工程一致。
- 已安装 NativeAOT 工具链所需的本机构建环境（编译器/链接器），具体见 [.NET NativeAOT 官方部署文档](https://learn.microsoft.com/dotnet/core/deploying/native-aot/)。
- 了解 AspectCore 的基本用法（拦截器、代理、容器接管）。如果还不熟悉，先看[快速上手](./quick-start.md)。

## 从零到可运行

下面以一个控制台程序为例，给出一套可直接复制的完整配置。

### 1. 目标框架

在 `.csproj` 中把目标框架设为 .NET 7 及以上，示例用 `net9.0`：

```xml
<TargetFramework>net9.0</TargetFramework>
```

### 2. 开启 NativeAOT 相关的项目属性

在 `.csproj` 的 `<PropertyGroup>` 中加入以下属性（取自官方 E2E 工程 `tests/AspectCore.NativeAot.E2E/AspectCore.NativeAot.E2E.csproj`）：

```xml
<PropertyGroup>
  <OutputType>Exe</OutputType>
  <TargetFramework>net9.0</TargetFramework>
  <Nullable>enable</Nullable>

  <!-- 开启 NativeAOT 发布 -->
  <PublishAot>true</PublishAot>
  <!-- 声明本项目按 AOT 兼容方式编译，启用裁剪/AOT 分析器 -->
  <IsAotCompatible>true</IsAotCompatible>

  <!-- 抑制来自 AspectCore 运行时中“有意不做 AOT 安全”代码的裁剪告警 -->
  <SuppressTrimAnalysisWarnings>true</SuppressTrimAnalysisWarnings>
  <!-- 保留元数据,供代理构造函数的反射解析使用 -->
  <IlcTrimMetadata>false</IlcTrimMetadata>
</PropertyGroup>
```

- `PublishAot` 和 `IsAotCompatible` 是必需项。
- `SuppressTrimAnalysisWarnings` 与 `IlcTrimMetadata` 是「视需要」的配套项：Source Generator 路径仍保留了少量基于标准反射的解析（如代理构造函数查找），关闭元数据裁剪能避免运行期找不到构造函数；若你的场景不涉及这类反射解析，可按需收紧。

### 3. 引用运行时包与 Source Generator

运行时依赖照常安装（与[安装](./installation.md)一致）：

```bash
dotnet add package AspectCore.Extensions.DependencyInjection
```

Source Generator 需要以 **analyzer** 方式引用，不能当普通库引用。手动在 `.csproj` 中加入：

```xml
<ItemGroup>
  <PackageReference Include="AspectCore.SourceGenerator" Version="<与其它 AspectCore 包一致的版本>"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

`OutputItemType="Analyzer"` 让编译器在编译期加载它生成代理代码，`ReferenceOutputAssembly="false"` 表示不把它作为运行时程序集引用。如果你在本仓库内直接引用源码，则改用等价的 `ProjectReference`（E2E 工程即如此）：

```xml
<ProjectReference Include="..\..\src\AspectCore.SourceGenerator\AspectCore.SourceGenerator.csproj"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false" />
```

### 4. 切换到 Source Generator 引擎

在注册服务时，除了照常调用 `ConfigureDynamicProxy()`，再调用 `ConfigureDynamicProxyEngine(...)` 把引擎切到 Source Generator：

```csharp
using AspectCore.DynamicProxy;

services.ConfigureDynamicProxy();
services.ConfigureDynamicProxyEngine(o =>
{
    o.Engine = ProxyEngine.SourceGenerator;
    o.Strict = true; // 可选：严格模式下,遇到无法 AOT 的路径直接抛异常,而不是回退反射
});
```

`ProxyEngineOptions`（`src/AspectCore.Abstractions/DynamicProxy/ProxyEngineOptions.cs`）上有三个开关：

| 选项 | 含义 |
|------|------|
| `Engine` | 引擎选择：`DynamicProxy`（默认）/ `SourceGenerator` / `Auto`。NativeAOT 必须设为 `SourceGenerator`。 |
| `AllowRuntimeFallback` | 缺失生成物时是否回退到运行时 DynamicProxy。`Engine=Auto` 时默认 `true`，`Engine=SourceGenerator` 时默认 `false`。NativeAOT 下回退不可用，应保持关闭。 |
| `Strict` | 为 `true` 时缺失生成物直接抛异常，适合用来在 CI/开发期强约束覆盖率，尽早暴露不兼容路径。 |

`Strict` 与 `AllowRuntimeFallback` 的取舍见下文[已知限制与诊断](#已知限制与诊断)。

### 5. 手动注册 Source Generator 代理注册表

NativeAOT 开启裁剪后，靠程序集扫描自动发现生成的代理**可能不可靠**。为确保原生二进制能稳定找到代理，用工厂方式手动注册代理注册表（取自 E2E 工程 `Program.cs`）：

```csharp
services.AddSingleton<AspectCore.DynamicProxy.ISourceGeneratedProxyRegistry>(
    _ => new AspectCore.SourceGenerated.AspectCoreSourceGeneratedProxyRegistry());
```

`AspectCoreSourceGeneratedProxyRegistry` 由 Source Generator 在编译期生成，用工厂注册可以绕开 AOT 下的 DI 构造函数解析问题。这一步在 NativeAOT 场景下是**推荐做法**；在非 AOT 环境下可省略。

把第 4、5 步合到一起，一个最小可运行的控制台入口如下：

```csharp
using AspectCore.DynamicProxy;
using AspectCore.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddTransient<ICustomService, CustomService>();

services.ConfigureDynamicProxy();
services.ConfigureDynamicProxyEngine(o =>
{
    o.Engine = ProxyEngine.SourceGenerator;
    o.Strict = true;
});
services.AddSingleton<AspectCore.DynamicProxy.ISourceGeneratedProxyRegistry>(
    _ => new AspectCore.SourceGenerated.AspectCoreSourceGeneratedProxyRegistry());

var provider = services.BuildDynamicProxyProvider();
provider.GetRequiredService<ICustomService>().Call();
```

拦截器与服务的写法和普通场景完全一样（见[快速上手](./quick-start.md)），Source Generator 会在编译期为它们生成代理。

### 6. （按需）用 rd.xml 保留反射元数据

如果你的代码涉及需要运行期反射的场景（例如动态类型、部分泛型路径），可以用 `rd.xml` 显式声明要保留元数据的程序集，避免被裁剪。参考 E2E 工程的 `tests/AspectCore.NativeAot.E2E/rd.xml`：

```xml
<Directives xmlns="http://schemas.microsoft.com/netfx/2013/01/metadata">
  <Application>
    <Assembly Name="YourApp" Dynamic="Required All" />
    <Assembly Name="AspectCore.Core" Dynamic="Required All" />
    <Assembly Name="AspectCore.Abstractions" Dynamic="Required All" />
    <Assembly Name="AspectCore.Extensions.DependencyInjection" Dynamic="Required All" />
  </Application>
</Directives>
```

在 `.csproj` 中通过 `RdXmlFile` 引入：

```xml
<ItemGroup>
  <RdXmlFile Include="rd.xml" />
</ItemGroup>
```

`Dynamic="Required All"` 会保留对应程序集的完整反射元数据。把 `YourApp` 换成你自己的程序集名。如果你的场景不涉及运行期反射，这一步可以跳过。

### 7. 发布并运行

用 `dotnet publish` 发布成对应平台（RID）的原生二进制，然后直接运行验证：

```bash
dotnet publish -c Release -r linux-x64
```

把 `linux-x64` 换成你的目标 RID（如 `win-x64`、`osx-arm64`）。发布产物是一个自包含的原生可执行文件，不依赖 .NET 运行时，直接运行即可看到拦截器生效。

## 已知限制与诊断

即使切到 Source Generator，NativeAOT 下仍有一些编译期无法完全静态化的签名。Source Generator 会在编译期发出 `ACSG` 系列诊断提示这些情况。

### 开放泛型方法可能回退反射

对开放泛型方法（如 `T Process<T>(T input)`），Source Generator 无法为所有可能的 `T` 预生成强类型委托，只能覆盖它在编译期能发现的具体类型实参。对未覆盖到的类型：

- `Strict = false`：回退到 `MethodInfo.Invoke()`（标准反射，NativeAOT 兼容但更慢，且依赖元数据被保留）。
- `Strict = true`：直接抛 `InvalidOperationException`，提示你补充类型提示。

Source Generator 会对这类方法发出诊断 **ACSG0101**。要让某个开放泛型方法获得完整的 AOT 委托覆盖，用 `[AspectCoreGenericHint]` 在方法上声明已知的闭合类型（`src/AspectCore.Abstractions/DynamicProxy/AspectCoreGenericHintAttribute.cs`）：

```csharp
[AspectCoreGenericHint(typeof(int), typeof(string))]
T Process<T>(T input);
```

上面告诉 Source Generator 为 `Process<int>` 和 `Process<string>` 生成强类型委托。该特性可在同一方法上多次标注，每次提供一组类型实参。

### ref struct / byref-like 参数与返回值不支持代理

代理管道以 `object[]` 传参、以 `object` 承载返回值，因此 `ref struct` 等 byref-like 类型无法参与代理。Source Generator 会对这类签名在编译期报诊断，避免生成后在运行期失败：

| 诊断码 | 场景 |
|--------|------|
| `ACSG008` | 代理目标本身是 byref-like 类型。 |
| `ACSG009` | byref-like 的 `params` 参数。 |
| `ACSG010` | 非 `params` 的 byref-like 参数。 |
| `ACSG011` | byref-like 返回值。 |

遇到这些诊断时，通常需要调整签名，或用 `[NonAspect]` 显式排除该成员（见[核心概念](./concepts.md#nonaspect)）。

> `ACSG` 系列诊断的完整含义、触发条件与修复建议，见 [Source Generator 诊断目录](../guide/source-generator-diagnostics.md)。

### Strict 模式 vs 运行时回退的取舍

- 想要**尽早在编译/CI 阶段暴露所有不兼容路径**：设 `Strict = true`，任何缺失生成物或命中反射回退的地方都会直接失败，不会悄悄退化。推荐在 NativeAOT 目标工程和 CI 中开启。
- 想要**在非 AOT 环境保留兼容性、AOT 环境按需收紧**：可用 `Auto` + `AllowRuntimeFallback`，但要清楚在 NativeAOT 下运行时回退到 DynamicProxy 是不可用的——回退一旦被触发就会抛异常。因此 NativeAOT 目标工程应保持 `SourceGenerator` + 不回退。

## 验证与参考

### 官方 E2E 示例工程

仓库内的 `tests/AspectCore.NativeAot.E2E/` 是一个最小可发布、可运行的完整示例，建议直接参考：

- `AspectCore.NativeAot.E2E.csproj` — 本页所有 `.csproj` 配置的来源。
- `Program.cs` — 引擎切换、手动注册代理注册表，以及覆盖同步/异步/`ValueTask`/`IAsyncEnumerable`/`ref`·`out` 参数/多拦截器堆叠/keyed 服务/接口与类代理等场景的断言。
- `rd.xml` — 反射元数据保留声明。

### 在本地照做验证

仓库通过 `.github/workflows/nativeaot-verify.yml` 做**双重验证**：先 `dotnet publish` 发布原生二进制，再直接运行该二进制并以退出码判定成败。你可以在本地照同样的方式验证自己的工程：

```bash
# 1. 发布原生二进制
dotnet publish path/to/YourApp.csproj -c Release -r linux-x64 -o ./publish-aot

# 2. 直接运行原生二进制
./publish-aot/YourApp
```

发布成功且运行时拦截器行为符合预期，即说明你的 NativeAOT 配置可用。

## 下一步

- [Source Generator 编译时引擎](../architecture/source-generator.md) — 编译期代理的生成机制。
- [两套引擎对比与选型](../architecture/engine-comparison.md) — DynamicProxy / SourceGenerator / Auto 的整体取舍。
- [NativeAOT AOP 设计方案](../architecture/nativeaot-design.md) — NativeAOT 支持的设计背景与实现细节（面向贡献者）。
