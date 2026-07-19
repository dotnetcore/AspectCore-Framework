# 两套引擎对比与选型

AspectCore 提供两套等价的代理生成引擎：运行时的 [DynamicProxy](./dynamic-proxy.md) 与编译时的 [Source Generator](./source-generator.md)。二者共享同一套拦截语义（同样的 `IAspectActivator`/`AspectContext`/拦截器管线），因此**拦截行为一致**，区别只在代理"何时、如何"生成。本文帮你选型，并说明 `ProxyEngineOptions` 的三个开关行为。

## 1. 对比一览

| 维度 | DynamicProxy | Source Generator |
|------|-------------|------------------|
| 代理生成时机 | 运行时 | 编译时 |
| 生成方式 | `System.Reflection.Emit` 发射 IL | Roslyn 生成 C# 源码 |
| 触发条件 | 服务被容器管理 / 经 `ProxyGenerator` 创建 | 类型标注 `[AspectCoreGenerateProxy]`（或程序集级自动发现） |
| 是否需改构建 | 否 | 需引用 `AspectCore.SourceGenerator`（analyzer） |
| AOT / 裁剪 | 代理生成依赖 `Reflection.Emit`（标 `[RequiresDynamicCode]`） | 代理**生成**不需 `Reflection.Emit`；但拦截时目标调用仍经 `MethodReflector`（`DynamicMethod`），并非端到端 NativeAOT |
| 首次调用开销 | 运行时生成 + 管线组装 | 仅管线组装（同步路径还内联激活、省去 `AspectActivator` 分配） |
| record 相等性 | 引用相等（生成普通类） | 值相等（生成 `record class`） |
| 默认启用 | 是 | 否（显式 opt-in） |

## 2. 引擎选择（ProxyEngine）

通过 `ProxyEngineOptions.Engine`（`AspectCore.Abstractions/DynamicProxy/ProxyEngine.cs:6`）选择：

- `ProxyEngine.DynamicProxy`（默认）：始终运行时生成。
- `ProxyEngine.SourceGenerator`：只用编译期生成物，缺失时报错。
- `ProxyEngine.Auto`：优先 SG，缺失时按策略回退 DynamicProxy（适合渐进迁移）。

**默认行为不变**：不显式配置时，即使引用了 `AspectCore.SourceGenerator`，仍走 `DynamicProxy`。只有调用 `ConfigureDynamicProxyEngine(...)` 才启用可选引擎逻辑。

## 3. 三个开关的行为

`ProxyEngineOptions` 提供 `Engine`、`AllowRuntimeFallback`（可空 bool）、`Strict`：

```csharp
services.ConfigureDynamicProxyEngine(o =>
{
    o.Engine = ProxyEngine.Auto;
    o.AllowRuntimeFallback = true; // 缺失生成物时是否回退 DynamicProxy
    o.Strict = false;              // 严格模式：缺失即抛异常
});
```

按当前实现（`SourceGeneratedProxyTypeGenerator.CreateCore`，`AspectCore.Core/DynamicProxy/SourceGeneratedProxyTypeGenerator.cs:72`）：

- `Engine = DynamicProxy`：始终运行时 DynamicProxy。
- `Engine = SourceGenerator`：只用 SG 生成物；**缺失时抛异常**（即使 `AllowRuntimeFallback=true` 也不回退）。
- `Engine = Auto`：优先 SG；缺失时**默认允许**回退 DynamicProxy。
  - 显式 `AllowRuntimeFallback=false` → 缺失抛异常。
  - `Strict=true` → 缺失抛异常（常用于 CI 强约束"该生成的都生成了"）。

回退开关的判定：显式的 `AllowRuntimeFallback` 优先，否则只有 `Auto` 默认为 `true`（`GetAllowRuntimeFallback`，`:115`）。

## 4. 启用 Source Generator 的三件事

必须同时满足：

1. **引用 analyzer**：以 analyzer 方式引用 `AspectCore.SourceGenerator`（NuGet `PrivateAssets="all"`，或 `ProjectReference` + `OutputItemType="Analyzer"`）。
2. **触发生成**：在类型上标 `[AspectCoreGenerateProxy]`（接口需给实现类型）。
3. **运行时选引擎**：`ConfigureDynamicProxyEngine(o => o.Engine = ProxyEngine.SourceGenerator | Auto)`。

MS.DI 与 ServiceContext 都支持 `ConfigureDynamicProxyEngine`；该 API 在 `Extensions.DependencyInjection`、`Extensions.Autofac`、`Extensions.LightInject` 适配中一致提供。

## 5. AOT / 裁剪下的手动 registry

运行时发现 SG 生成物依赖两步：扫描程序集上的 `[AspectCoreSourceGeneratedProxyRegistryAttribute]`，再反射无参构造 registry 实例。裁剪/AOT 下扫描与反射可能不可用，导致"生成了但运行时找不到"。此时手动注册：

```csharp
// MS.DI（泛型）
services.AddSourceGeneratedProxyRegistry<AspectCore.SourceGenerated.AspectCoreSourceGeneratedProxyRegistry>();

// ServiceContext（实例）
serviceContext.AddSourceGeneratedProxyRegistry(new AspectCore.SourceGenerated.AspectCoreSourceGeneratedProxyRegistry());
```

> registry 类型名/命名空间以项目实际生成结果为准；当前生成器默认输出 `AspectCore.SourceGenerated.AspectCoreSourceGeneratedProxyRegistry`。

> **关于 NativeAOT 的边界（重要）**：手动注册 registry 只解决"运行时发现代理类型"的问题，**不代表**端到端 NativeAOT 已被支持：
> - 代理**构造**仍保留反射路径（相关 API 标注 `[RequiresDynamicCode]`）。
> - 拦截时目标方法调用经 `RuntimeAspectContext.Complete()` → `MethodReflector`，其构造会创建 `DynamicMethod`（`AspectCore.Extensions.Reflection/MethodReflector.cs:26`）。
>
> 因此 Source Generator 的价值是**降低代理生成阶段对 `Reflection.Emit`/动态代码的依赖**，而非已验证的 NativeAOT 支持。仓库当前没有 NativeAOT publish/run 测试，若你的目标是完整 NativeAOT，请自行验证并留意上述运行时约束。

## 6. 选型建议

- **渐进迁移 / 本地开发**：`Auto`（默认允许回退），先跑起来。
- **CI 强约束**：`Auto + Strict=true`（或 `Auto + AllowRuntimeFallback=false`），确保该生成的都生成了。
- **裁剪 / 减少代理生成期动态代码**：`SourceGenerator` + 手动注册 registry（注意仍有上述运行时反射/动态代码约束，并非已验证的端到端 NativeAOT）。
- **无法改类型 / 不想标注**：留在默认 `DynamicProxy`。

## 7. 一致性保障

任何影响拦截语义的改动必须同时覆盖两套引擎，并由 `tests/AspectCore.Core.Tests/EngineParity/` 下的双引擎一致性测试守护。已知的行为差异（record 相等性、`init` setter、无目标接口 stub 的 `ref` 返回边界）在 [Record 类型支持](./record-support.md) 与 [C# 语言特性适配](./language-features.md) 中说明。
