# AspectCore-Framework 发展路线图

> 版本：2026-07-19
> 维护者：AspectCore 核心团队
> 本文档描述 AspectCore-Framework 的战略定位、优先级事项、时间线与风险评估。所有事项均按优先级排序，并附具体可执行的验收标准。

---

## 一、战略定位

### 1.1 项目概述

AspectCore 是一个开源的 .NET AOP（面向切面编程）框架，采用双引擎架构（运行时 DynamicProxy + 编译时 Source Generator），全面适配 C# 6–13 语言特性，并与 MSDI（Microsoft.Extensions.DependencyInjection）深度集成。

### 1.2 核心竞争优势

- **双引擎一致性**：同一套 `IAspectActivator`、`AspectContext` 和拦截器管道同时服务于运行时引擎与编译时引擎，用户无需为两种引擎维护两套拦截逻辑。
- **激进的 C# 特性适配**：支持部分属性（partial properties）、主构造函数（primary constructors）、`params` 集合、`ref struct` 拒绝、记录类型（record types）、`ref`/`ref readonly` 返回、异步可枚举（async enumerable）等现代 C# 特性。
- **统一的拦截器模型**：无需像 Castle 那样为异步场景单独维护 `IAsyncInterceptor` 包装器，一套拦截器模型覆盖同步与异步。
- **深度 MSDI 集成 + Windsor 集成**：在 MSDI 无原生 AOP 的市场空白中占据有利位置，并提供 Windsor 集成以覆盖存量用户。

### 1.3 核心短板

- **缺乏端到端 NativeAOT 支持**：`MethodReflector` 仍在运行时依赖 `DynamicMethod`，无法通过 NativeAOT 编译裁剪。
- **Keyed 服务解析缺口**：`IServiceResolver` 的各实现对 `GetKeyedService`/`GetRequiredKeyedService` 抛出 `NotImplementedException`。
- **Source Generator 仅为可选项**：编译时引擎未成为默认引擎，用户需手动启用。

### 1.4 竞争格局

| 竞品 | 现状 | 对 AspectCore 的影响 |
|------|------|----------------------|
| **MSDI** | 无原生 AOP；.NET 9 编译时 Source Generator 稳定，.NET 10 完全对等，正从默认 DI 管道中消除运行时 Reflection.Emit；.NET 8+ 原生装饰器正在侵蚀低端价值主张 | MSDI 不做 AOP 是 AspectCore 的核心机会；但需跟进其编译时方向，避免在运行时反射路线上被边缘化 |
| **Castle DynamicProxy** | Castle.Core 仍在积极维护（v5.2.1，2025-03），Windsor 功能冻结；16 亿次 NuGet 下载（多数经 Moq/NSubstitute/EF Core 传递依赖）；技术局限：无 ref 返回、无 proper record 支持、无 NativeAOT、无 async streams、现代 C# 特性适配有限；Castle v6.0 Source Generator 已规划但无发布日期 | Castle 在存量市场仍占主导，但其技术天花板明显；AspectCore 可通过现代特性与 NativeAOT 形成差异化 |
| **Metalama** | 企业级编译时 AOP 最强竞品，附带 IDE 工具；免费增值模式，核心闭源 | 在高端企业市场构成直接竞争；AspectCore 以"免费 + 开源"差异化 |
| **AspectInjector** | 基于 Roslyn 的编译时织入器，MIT 协议，约 660 万下载；无官方 NativeAOT 支持 | 同属开源编译时方案，AspectCore 需以双引擎一致性和更广泛的特性适配拉开差距 |

---

## 二、短期优先级（1–2 周）

### P0-1：修复 Keyed 服务解析缺口

**背景**：`IServiceResolver` 的各实现对 `GetKeyedService`/`GetRequiredKeyedService` 抛出 `NotImplementedException`。该缺口已被作为"预期行为"写入测试，严重损害用户信任。.NET 8 起 MSDI 原生支持 keyed 服务，AspectCore 必须跟进。

**具体行动**：
1. 审计所有 `IServiceResolver` 实现类，列出缺失 `GetKeyedService`/`GetRequiredKeyedService` 的清单。
2. 逐一实现两个方法，确保与 MSDI 的 `IKeyedServiceProvider` 语义对齐：
   - `GetKeyedService(Type serviceType, object? serviceKey)` —— keyed 服务不存在时返回 `null`。
   - `GetRequiredKeyedService(Type serviceType, object? serviceKey)` —— keyed 服务不存在时抛出明确异常。
3. 确保 keyed 服务解析路径同样经过切面激活管道（不绕过拦截器）。

**验收标准**：
- 所有 `IServiceResolver` 实现不再包含 `NotImplementedException`。
- keyed 服务解析与普通服务解析走同一套切面激活逻辑。
- 现有"预期行为"测试更新为真实行为断言。

### P0-2：新增 Keyed 服务拦截集成测试

**背景**：P0-1 修复后，需要端到端测试验证 keyed 服务在被拦截时的完整行为，防止回归。

**具体行动**：
1. 新增集成测试项目（或扩充现有集成测试），覆盖以下场景：
   - keyed 单例服务的拦截器执行。
   - keyed 瞬态服务的拦截器执行。
   - 同一接口注册多个 keyed 实现时，按 key 精确解析并拦截。
   - `GetRequiredKeyedService` 在服务缺失时抛出预期异常。
   - keyed 服务与非 keyed 服务混合注册时的解析正确性。
2. 测试需同时覆盖运行时 DynamicProxy 引擎与编译时 Source Generator 引擎（双引擎一致性验证）。

**验收标准**：
- 上述场景均有对应测试用例并全部通过。
- 测试在 CI 中作为门禁，keyed 相关变更必须通过全部用例。

### P1-1：将 Source Generator 设为默认引擎并提供 Auto 回退

**背景**：Source Generator 目前仅为可选项，多数用户仍在使用运行时 DynamicProxy。将编译时引擎设为默认，可显著降低运行时开销、改善 NativeAOT 兼容性，并与 MSDI 的编译时方向保持一致。

**具体行动**：
1. 引入引擎选择策略：`Auto`（默认）、`Runtime`、`SourceGenerator`。
2. `Auto` 模式行为：
   - 优先使用 Source Generator（若目标项目已启用并成功生成代理）。
   - 当 Source Generator 不可用（如项目未启用、生成失败、或目标框架不支持）时，自动回退到运行时 DynamicProxy。
   - 回退时记录可诊断的警告信息，便于用户排查。
3. 更新文档与示例，将默认配置指向 `Auto`。
4. 提供显式选择 `Runtime`/`SourceGenerator` 的配置入口，满足需要确定性行为的场景。

**验收标准**：
- 新用户零配置即获得 Source Generator 优先的体验。
- `Auto` 回退路径有明确日志，不静默降级。
- 现有运行时用户升级后行为不变（回退到 Runtime）。

---

## 三、中期优先级（1–3 个月）

### P0-3：实现端到端 NativeAOT 支持

**背景**：`MethodReflector` 在运行时依赖 `DynamicMethod`（Reflection.Emit），这是 NativeAOT 的硬性障碍。若 12–18 个月内无法提供端到端 NativeAOT 支持，AspectCore 将与平台方向不兼容，面临被淘汰的最高风险。

**具体行动**：
1. 梳理 `MethodReflector` 中所有 `DynamicMethod` 使用点，明确其在拦截器管道中的职责（方法调用分派、参数构造、返回值提取等）。
2. 以编译时生成的分派代码（compile-time-generated dispatch）替代运行时 `DynamicMethod`：
   - Source Generator 为被拦截方法生成强类型分派逻辑，在编译期完成方法签名的绑定。
   - 运行时引擎在 NativeAOT 场景下走预生成的分派表，完全避免 `DynamicMethod`。
3. 标注所有仍依赖运行时反射的类型与成员，添加 `[RequiresDynamicCode]` / `[RequiresUnreferencedCode]` 注解，使 NativeAOT 编译警告可定位、可消除。
4. 建立 NativeAOT 验证工程：
   - 配置 `<PublishAot>true</PublishAot>` 的示例项目。
   - 在 CI 中增加 NativeAOT 发布 + 运行验证流水线。

**验收标准**：
- NativeAOT 发布的示例项目可编译、可运行，且拦截器行为与运行时版本一致。
- `MethodReflector` 不再在 NativeAOT 路径上调用 `DynamicMethod`。
- CI 中 NativeAOT 流水线稳定通过。

### P1-2：构建 Windsor 迁移指南与工具

**背景**：Windsor 功能冻结，Castle.Core 虽在维护但技术天花板明显。大量存量 Windsor/Castle 用户正在寻找迁移路径。AspectCore 提供兼容垫片与迁移工具，可有效承接这部分用户。

**具体行动**：
1. 编写 Castle `IInterceptor`/`IInvocation` API 到 AspectCore 拦截器模型的兼容性垫片（compatibility shim）：
   - 使 Castle 风格的拦截器可在 AspectCore 管道中运行，降低迁移初期改造成本。
   - 明确标注垫片的覆盖范围与不支持的 API（如 ref 返回、async streams 等 Castle 不支持的特性）。
2. 撰写《Windsor → AspectCore 迁移指南》：
   - 容器注册方式对照。
   - 拦截器 API 对照与自动转换建议。
   - 常见配置（命名约定、属性注入、生命周期）的迁移步骤。
   - FAQ：迁移后行为差异、性能影响、如何逐步迁移而非一次性切换。
3. 提供迁移辅助工具（脚本或 Roslyn 分析器）：
   - 识别 Windsor 特有 API 调用并给出 AspectCore 替代建议。
   - 生成迁移检查清单。

**验收标准**：
- 兼容垫片覆盖 Castle `IInterceptor`/`IInvocation` 的核心使用场景。
- 迁移指南包含可运行的前后对比示例。
- 迁移工具可在示例 Windsor 项目上输出可操作的迁移建议。

### P1-3：建立对标竞品的基准测试套件

**背景**：需要用数据说话，量化 AspectCore 相对于 Castle DynamicProxy、Metalama、AspectInjector 的性能表现，为技术决策与市场宣传提供依据。

**具体行动**：
1. 定义基准测试维度：
   - 首次调用延迟（first-invocation latency）：代理创建 + 首次拦截的端到端耗时。
   - 稳态开销（steady-state overhead）：拦截器在热路径上的额外耗时。
   - 内存分配（memory allocation）：每次拦截的分配字节数与 GC 压力。
   - NativeAOT 兼容性：是否可发布为 NativeAOT 及发布后体积。
2. 选定对标对象：Castle DynamicProxy（v5.x）、AspectInjector（Metalama 闭源，可做定性对比）。
3. 使用 BenchmarkDotNet 构建测试套件，确保：
   - 每个维度有明确的测试方法与对照组（无拦截基线）。
   - 测试环境与参数（框架版本、硬件、并发数）文档化。
   - 结果可复现。
4. 定期（如每季度）更新基准结果并公开发布。

**验收标准**：
- 基准套件覆盖上述四个维度，每个维度至少有一个 Castle 对标用例。
- 基准结果以表格 + 图表形式发布在 docs 目录。
- 基准测试纳入 CI，防止性能回归。

---

## 四、风险评估（1–2 年）

| 风险 | 等级 | 说明 | 缓解策略 |
|------|------|------|----------|
| **NativeAOT 过时风险** | 最高 | 若 12–18 个月内无法提供端到端 NativeAOT 支持，将与平台方向不兼容 | P0-3 为最高优先级；在 NativeAOT 路径上彻底移除 `DynamicMethod`；建立 NativeAOT CI 门禁 |
| **Metalama 免费层扩张** | 高 | Metalama 若扩大免费层功能，将侵蚀 AspectCore"免费 + 开源"的差异化优势 | 强化开源社区治理、双引擎一致性、与 MSDI 深度集成等 Metalama 不具备的优势；保持完全开源 |
| **Castle Source Generator 发布** | 中 | Castle v6.0 若发布 Source Generator，将缩小 AspectCore 在编译时方向的差距 | 加快 P1-1（Source Generator 默认化）与 P0-3（NativeAOT）落地，在 Castle 之前占据编译时 + NativeAOT 的生态位 |
| **社区可持续性** | 中 | 团队规模小，节奏激进，存在 burnout 风险 | 建立贡献者指南与自动化 CI/CD 降低维护成本；优先保障 P0 事项，P1 事项可吸纳社区贡献；避免在非核心方向过度消耗 |
| **Keyed 服务缺口损害信任** | 中 | `NotImplementedException` 被作为"预期行为"测试，传递出"功能不完整"的负面信号 | P0-1 + P0-2 立即修复；修复后发布公告，主动恢复用户信心 |

---

## 五、时间线总览

| 阶段 | 时间窗口 | 事项 | 优先级 |
|------|----------|------|--------|
| **短期** | 第 1–2 周 | P0-1：修复 keyed 服务解析缺口 | P0 |
| | | P0-2：keyed 服务拦截集成测试 | P0 |
| | | P1-1：Source Generator 默认引擎 + Auto 回退 | P1 |
| **中期** | 第 1–3 个月 | P0-3：端到端 NativeAOT 支持 | P0 |
| | | P1-2：Windsor 迁移指南与工具 | P1 |
| | | P1-3：对标竞品基准测试套件 | P1 |
| **长期** | 第 3–12 个月 | 持续跟进 C# 最新特性适配 | P2 |
| | | 扩展生态集成（更多 DI 容器、框架） | P2 |
| | | 社区治理与贡献者体系建设 | P2 |

> **优先级说明**：P0 = 必须立即完成，阻塞核心竞争力或损害用户信任；P1 = 应在中期内完成，强化差异化优势；P2 = 长期投入，视社区资源与市场反馈动态调整。

---

## 六、优先级决策原则

当资源有限时，按以下顺序取舍：

1. **用户信任优先**：已暴露的功能缺口（如 keyed 服务）必须先于新特性修复。
2. **平台方向优先**：NativeAOT 是 .NET 平台的确定性方向，与之兼容是生存前提。
3. **差异化优先**：选择能放大 AspectCore 独特优势（双引擎一致性、现代 C# 适配、开源免费）的投入。
4. **可验证优先**：每项工作必须有明确的验收标准与 CI 门禁，避免"完成了但无法证明"。

---

*本文档将根据项目进展与市场变化定期更新。建议每季度回顾一次优先级与风险评估。*
