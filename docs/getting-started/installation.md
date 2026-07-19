# 安装

AspectCore 按职责拆分成多个 NuGet 包：核心库负责 AOP 与容器，集成包负责对接各种宿主和第三方容器，编译期引擎单独发布。本页说明每个包的用途、支持的目标框架，以及如何按场景选包。

## 核心库

| 包 | 用途 | 何时需要 |
|----|------|----------|
| `AspectCore.Abstractions` | 契约层：`AspectContext`、`IInterceptor`、`AbstractInterceptorAttribute`、`IAspectConfiguration`、`IServiceContext` 等抽象与特性。 | 通常由其他包间接引入；只想引用契约做扩展时直接安装。 |
| `AspectCore.Core` | DynamicProxy 运行时引擎 + 内置 IoC 容器（`ServiceContext` / `IServiceResolver`）。 | 需要独立使用动态代理或内置容器时。 |
| `AspectCore.Extensions.Reflection` | 独立的高性能反射扩展（`GetReflector()`），不依赖 AOP。 | 只需要快速反射调用时可单独使用。 |

## 集成包

| 包 | 用途 |
|----|------|
| `AspectCore.Extensions.DependencyInjection` | 与 `Microsoft.Extensions.DependencyInjection` 集成：`ConfigureDynamicProxy(...)` 配置拦截器、`DynamicProxyServiceProviderFactory` 接管容器。 |
| `AspectCore.Extensions.Autofac` | 与 Autofac 集成：`ContainerBuilder.RegisterDynamicProxy(...)`。 |
| `AspectCore.Extensions.Windsor` | 与 Castle Windsor 集成：`container.AddAspectCoreFacility(...)`。 |
| `AspectCore.Extensions.LightInject` | 与 LightInject 集成：`container.RegisterDynamicProxy(...)`。 |
| `AspectCore.Extensions.Hosting` | 与泛型主机集成：`IHostBuilder.UseServiceContext()` / `UseDynamicProxy()`。 |
| `AspectCore.Extensions.AspNetCore` | ASP.NET Core 场景下的扩展支持。 |
| `AspectCore.Extensions.Configuration` | 从 `IConfiguration` 注入配置值：`AddConfigurationInject()`。 |
| `AspectCore.Extensions.DataValidation` | 数据校验基础设施（抽象与拦截器）。 |
| `AspectCore.Extensions.DataAnnotations` | 基于 `System.ComponentModel.DataAnnotations` 的校验实现：`AddDataAnnotations(...)`。 |

## 编译期引擎

| 包 | 用途 |
|----|------|
| `AspectCore.SourceGenerator` | 编译期 Source Generator AOP 引擎，以 analyzer 方式引用，在编译时生成代理类型。属于**显式 opt-in**，默认不改变运行时行为。 |

Source Generator 是运行时 DynamicProxy 之外的另一套引擎。两者的差异与选型见[两套引擎对比与选型](../architecture/engine-comparison.md)。

## 目标框架

| 包类别 | 目标框架 |
|--------|----------|
| 核心库与大部分集成包（`AspectCore.Abstractions`、`AspectCore.Core`、`AspectCore.Extensions.Reflection`、`AspectCore.Extensions.DependencyInjection`、Autofac/Windsor/LightInject/Hosting、Configuration、DataAnnotations 等） | `net9.0`、`net8.0`、`net7.0`、`net6.0`、`netstandard2.1`、`netstandard2.0` |
| `AspectCore.Extensions.AspNetCore` | `net9.0`、`net8.0`、`net7.0`、`net6.0`（不含 netstandard） |
| `AspectCore.SourceGenerator` | `netstandard2.0`（analyzer 约定） |

> 以上框架列表来自各包 `.csproj` 的 `TargetFrameworks`。核心库通过 `netstandard2.0`/`netstandard2.1` 覆盖较老的运行时；`AspectCore.Extensions.AspNetCore` 仅面向 `net6.0` 及以上。

## 安装命令

以 .NET CLI 为例（Package Manager 控制台把 `dotnet add package` 换成 `Install-Package` 即可）：

```bash
# 与 Microsoft.Extensions.DependencyInjection 集成（最常见的入口）
dotnet add package AspectCore.Extensions.DependencyInjection

# 仅使用动态代理或内置容器
dotnet add package AspectCore.Core

# 仅使用高性能反射扩展
dotnet add package AspectCore.Extensions.Reflection
```

`AspectCore.Extensions.DependencyInjection` 会通过依赖自动引入 `AspectCore.Core` 与 `AspectCore.Abstractions`，一般无需单独安装。

## 如何选包

- **ASP.NET Core / 泛型主机应用，使用内置容器**：装 `AspectCore.Extensions.DependencyInjection`。这是绝大多数场景的起点，直接看[快速上手](./quick-start.md)。
- **已经在用 Autofac / Windsor / LightInject**：装对应的集成包，见[第三方容器](../guide/third-party-containers.md)。
- **不需要 DI，只想手动创建代理对象**：装 `AspectCore.Core`，用 `ProxyGeneratorBuilder` 独立生成代理。
- **只想要比原生反射更快的反射调用**：单独装 `AspectCore.Extensions.Reflection`，见[反射扩展](../guide/reflection-extensions.md)。
- **需要编译期代理、降低代理生成期动态代码依赖**：额外引用 `AspectCore.SourceGenerator`，见[Source Generator 编译时引擎](../architecture/source-generator.md)。

## 下一步

- [快速上手](./quick-start.md) — 五分钟跑通第一个拦截器。
- [核心概念](./concepts.md) — 理解拦截器、代理、切面上下文等术语。
