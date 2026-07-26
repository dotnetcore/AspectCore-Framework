# 从 2.x 升级到 3.0

本页面向已经在使用 AspectCore 2.x 的项目，说明升级到 3.0 需要注意什么、怎么升、以及升级后遇到问题如何排查。核心结论先给出来：**目标框架在 `net6.0` 及以上的项目，绝大多数只需升级包版本，代码零改动**；真正的门槛只有一个——目标框架收窄。

> 本页说的是 **AspectCore 自身的大版本升级（2.x → 3.0）**。如果你要从 Castle DynamicProxy 迁移到 AspectCore，那是另一件事，见[Castle 迁移指南](./castle-migration/migration-guide.md)，不要和本页混淆。

## 先判断：你能不能升

3.0 最大的变化是目标框架（TFM）收窄。先对照下表确认你的项目是否满足升级前提：

| 你的项目目标框架 | 能否升级到 3.0 | 说明 |
|------------------|----------------|------|
| `net8.0` / `net9.0` / `net10.0` | ✅ 可以 | 推荐路径，功能完整 |
| `net6.0` | ✅ 可以 | 最低支持；核心库与大部分集成包保留了 `net6.0` |
| `net7.0` | ⚠️ 需先升框架 | 3.0 不再提供 `net7.0`，需先把项目升到 `net8.0` 及以上 |
| `.NET Framework`（`net461` 等） | ❌ 不能 | 3.0 移除了 `net461`，请停留在 2.x |
| 以 `netstandard2.0` / `netstandard2.1` 消费的库 | ❌ 不能 | 3.0 移除了 netstandard 目标，请停留在 2.x |

如果这一步就卡住了（.NET Framework 或 netstandard 消费方），先看[常见升级问题](#常见升级问题)里的替代方案，不必往下读升级步骤。

## Breaking Changes

### 1. 目标框架收窄（最主要的破坏性变更）

所有包的目标框架从 2.4.0 的多目标收窄为 3.0 的现代 .NET：

| 版本 | 目标框架 |
|------|----------|
| 2.4.0 | `net7.0;net6.0;netstandard2.1;netstandard2.0;net461` |
| 3.0 | `net10.0;net9.0;net8.0;net6.0` |

**被移除的框架**：`net461`（.NET Framework）、`netstandard2.0`、`netstandard2.1`、`net7.0`。

**为什么收窄**：

- NativeAOT 要求 .NET 7+，而 `System.Reflection.Emit` 在 AOT 下不可用；AspectCore 的编译期引擎要落地就必须放弃老框架。
- Default Interface Method（3.0 内部用来保证 DynamicProxy 路径兼容）要求 .NET Core 3.0+，netstandard2.0 无法编译。
- netstandard2.0/2.1 的实际使用场景基本是 .NET Framework 遗留项目，这类项目不会用到 SG 引擎和 NativeAOT，收窄对它们没有价值损失。
- `net7.0` 已 EOL，`net6.0` 虽也已 EOL，但作为 AOP 框架的最低门槛仍覆盖大量存量项目，故保留。

（依据：`docs/architecture/nativeaot-design.md` 的「TFM 变更」一节。）

**二进制兼容影响**：对 `netstandard2.0` 消费者而言，`AspectCore.Abstractions` 的 TFM 收窄是破坏性的（无法再被引用）；对 `net6.0` 及以上的消费者，`AspectCore.Abstractions` 新增了接口与默认接口方法，属于**非破坏性**变更。

### 2. 各包目标框架的两个特例

大部分包都是 `net10.0;net9.0;net8.0;net6.0`，但有两个包例外，升级时注意：

| 包 | 目标框架 | 注意点 |
|----|----------|--------|
| `AspectCore.Extensions.CastleCompat` | `net10.0;net9.0;net8.0`（**不含 `net6.0`**） | 如果你的项目停在 `net6.0` 又想用 Castle 兼容层，用不了；需升到 `net8.0` 及以上 |
| `AspectCore.SourceGenerator` | `netstandard2.0` | 这是 Roslyn 分析器的约定要求，以 analyzer 形式被编译器加载，不代表你的项目要支持 netstandard2.0 |

### 3. 新增的包

3.0 引入两个新包，都属于 opt-in，不装就不影响现有行为：

| 包 | 作用 |
|----|------|
| `AspectCore.SourceGenerator` | 编译期代理引擎，在编译时生成代理类型，是 NativeAOT 支持的基础。默认不启用。 |
| `AspectCore.Extensions.CastleCompat` | Castle DynamicProxy 兼容垫片，供存量 Castle 代码渐进迁移到 AspectCore。 |

包的完整清单与选包建议见[安装](../getting-started/installation.md)。

### 4. 默认行为保持不变

这是本次升级最需要明确的一点：**3.0 没有改变默认运行时行为**。

- 默认代理引擎仍然是 `DynamicProxy`。`ProxyEngineOptions.Engine` 的默认值就是 `ProxyEngine.DynamicProxy`，不显式配置就走原来的运行时织入路径。
- DynamicProxy 路径做了「零变更保证」：`AspectActivatorContext`（struct）、`IAspectContextFactory` 的原有方法、`RuntimeAspectContext`、`MethodReflector` 等均保持不变，不改签名、不改行为。
- 结论：**目标框架在 `net6.0` 及以上的项目，从 2.x 升到 3.0 通常代码零改动，只是把 NuGet 包版本升上去**。你现有的拦截器、配置方式、DI 注册都照旧工作。

（依据：`ProxyEngineOptions.cs` 中 `Engine` 的默认值；`docs/architecture/nativeaot-design.md` 的「DynamicProxy 路径兼容性保证」一节。）

### 5. 移除的 API

提交历史中可见的一处公开 API 移除：`ObjectExtensions.cs`（提交 `cbbaf24`，#347）。

> **诚实说明**：更细粒度的 public API 增删本指南没有逐条核对。如果你的代码依赖了某些不常用的公开类型，升级后出现编译错误，属正常范围。需要精确的 API 差异清单时，建议对 `v2.4.0..HEAD` 做一次符号级 diff（例如借助 API 对比工具），本指南不臆造一份删除清单。

## 升级步骤

### 第 1 步：前置检查（目标框架）

先确认目标框架落在 `net6.0` / `net8.0` / `net9.0` / `net10.0`。若当前是 `net7.0` 或更老，先改 `.csproj` 的 `TargetFramework(s)`：

```xml
<!-- 例如从 net7.0 升到 net8.0 -->
<TargetFramework>net8.0</TargetFramework>
```

若是 .NET Framework 或 netstandard 消费方，无法升级到 3.0，见[常见升级问题](#常见升级问题)。

### 第 2 步：更新 NuGet 包版本

把用到的 AspectCore 包统一升到 3.0：

```bash
# 最常见的入口包，会带上 AspectCore.Core / AspectCore.Abstractions
dotnet add package AspectCore.Extensions.DependencyInjection --version 3.0.0

# 按需升级其他用到的包，例如
dotnet add package AspectCore.Extensions.Autofac --version 3.0.0
dotnet add package AspectCore.Extensions.Configuration --version 3.0.0
```

> 3.0 目前为预览阶段（`3.0.0-rc.1`）。正式版发布后把版本号换成对应的稳定版即可；升级方式一致。

### 第 3 步：验证现有拦截行为

由于默认引擎不变，升级后**不需要改拦截器代码**。构建并运行你的测试，确认拦截行为与升级前一致即可。若拦截"突然不生效"，多半是目标框架或引擎配置问题，见[常见升级问题](#常见升级问题)。

### 第 4 步（可选）：启用 Source Generator 引擎

如果你想用编译期代理（例如为了 NativeAOT），显式切换引擎：

```csharp
using AspectCore.DynamicProxy;

services.AddDynamicProxy();
services.ConfigureDynamicProxyEngine(options =>
{
    options.Engine = ProxyEngine.SourceGenerator;
    // options.Strict = true; // 缺失生成物时直接抛异常，适合在 CI 强约束
});
```

`ProxyEngine` 有三个取值：`DynamicProxy`（默认，运行时）、`SourceGenerator`（编译期）、`Auto`（优先 SG，缺失时按 `AllowRuntimeFallback` 策略回退 DynamicProxy）。两套引擎的差异与选型见[两套引擎对比与选型](../architecture/engine-comparison.md)。

（依据：`ProxyEngine.cs` 枚举定义；`ServiceCollectionExtensions.ConfigureDynamicProxyEngine`;`docs/architecture/nativeaot-design.md` 的使用示例。）

### 第 5 步（按需）：NativeAOT

如果目标是发布 NativeAOT 应用，需切换到 Source Generator 引擎，并遵循 NativeAOT 的额外约束。设计与限制见[NativeAOT 设计文档](../architecture/nativeaot-design.md)；面向使用者的上手步骤见 [NativeAOT 上手指南](../getting-started/nativeaot.md)。

> 注意：NativeAOT 目前只覆盖 Source Generator 路径，DynamicProxy 路径在 AOT 下仍不可用。

### 第 6 步（按需）：从 Castle 迁移

如果你的项目同时还在用 Castle DynamicProxy，想借这次升级一并迁到 AspectCore，那是一条独立的迁移路径，见 [Castle 迁移指南](./castle-migration/migration-guide.md)、[功能对比](./castle-migration/feature-comparison.md)、[迁移检查清单](./castle-migration/checklist.md)。再次强调：Castle 迁移和本页的 2.x → 3.0 版本升级是两回事。

## 常见升级问题

### 我的项目是 .NET Framework / netstandard2.0，怎么办？

3.0 已移除 `net461`、`netstandard2.0`、`netstandard2.1`，这类项目无法升级。可选方案：

- **停留在 2.x**：2.4.0 仍支持这些框架，功能不变，继续可用。
- **升级运行时后再升 AspectCore**：如果条件允许把项目迁到 `net6.0`+，迁移完成后即可升级到 3.0。

### 升级后拦截"不生效"了？

按顺序排查：

1. **目标框架**：确认项目实际编译到的是 `net6.0`+，而不是意外落回了不受支持的框架。
2. **引擎配置**：如果显式设置了 `ProxyEngine.SourceGenerator` 或 `Auto`，确认生成物已正确产出；否则先去掉引擎配置回到默认 `DynamicProxy` 验证一遍，隔离问题。
3. **包版本一致**：确认所有 AspectCore 包都升到了同一大版本，避免新旧混用。

### 启用 Source Generator 后编译报诊断（ACSGxxx）？

Source Generator 引擎在编译期会产出以 `ACSG` 开头的诊断信息（提示哪些方法/类型无法被编译期代理等）。诊断编号的含义与处理办法见 [Source Generator 诊断参考](./source-generator-diagnostics.md)。

## 相关文档

- [安装](../getting-started/installation.md) — 各包用途、目标框架、选包建议
- [两套引擎对比与选型](../architecture/engine-comparison.md) — DynamicProxy vs SourceGenerator vs Auto
- [NativeAOT 设计文档](../architecture/nativeaot-design.md) — TFM 变更、兼容性保证、引擎选择矩阵
- [Castle 迁移指南](./castle-migration/migration-guide.md) — 从 Castle DynamicProxy 迁移到 AspectCore（区别于本页的版本升级）
