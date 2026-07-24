# AspectCore-Framework 发展路线图

> 版本：2026-07-24
> 维护者：AspectCore 核心团队
> 本文档描述 AspectCore-Framework 的生态定位、已落地基线、下一阶段优先级与验收标准。

---

## 一、生态定位

AspectCore 不再把增长重点放在“更大的 IoC 容器”或“更复杂的 AOP 语法”上。项目的生态位是：

> 在 Microsoft.Extensions 主航道上，提供 Castle 做不了、MSDI 不做、NativeAOT 时代又确实需要的 interception infrastructure。

这意味着后续投入优先服务四件事：

1. **NativeAOT + Source Generator 体验扎实**：可发布、可运行、可诊断，失败尽量发生在编译期。
2. **Castle / Windsor 迁移承接**：让存量 Castle 用户有低风险迁移路径，而不是只给一篇概念文档。
3. **官方横切能力包**：围绕 OpenTelemetry、HttpClient、validation、cache、retry 等基础设施场景提供小而可靠的 interceptor 包。
4. **工程化与信任资产**：benchmark、兼容矩阵、诊断目录、sample gallery、升级指南，让用户敢引入。

---

## 二、已落地基线

以下能力已从 roadmap 中移出，不再作为未来待办重复追踪：

- **Keyed 服务解析与拦截**：`IServiceResolver` 在 .NET 8+ 接入 `IKeyedServiceProvider`，内置容器、MSDI、Autofac、Windsor、LightInject 均已有 keyed 解析实现；keyed singleton/transient、多 key、缺失 required、keyed + non-keyed 混用、DynamicProxy + Source Generator 双引擎测试已覆盖。
- **Source Generator 引擎选择基础设施**：`ProxyEngine.DynamicProxy` / `SourceGenerator` / `Auto`、`AllowRuntimeFallback`、`Strict` 已存在。默认仍保持 `DynamicProxy`，避免破坏现有用户升级行为。
- **Source Generator NativeAOT 主路径**：已有 `IAspectInvokeDelegate`、`SourceGeneratedAspectContext`、NativeAOT E2E 工程和 `nativeaot-verify` workflow。当前承诺范围是 Source Generator 路径，不承诺 DynamicProxy 在 NativeAOT 下可用。
- **Castle 兼容垫片与迁移文档雏形**：已有 `AspectCore.Extensions.CastleCompat`、迁移指南、功能对照和 checklist。仍缺自动化迁移工具。
- **竞品 benchmark 雏形**：已有 `benchmarks/AspectCore.Benchmarks.Competitive`，覆盖 Castle 对比的首次调用、稳态、内存、NativeAOT 兼容性说明。仍缺结果发布与门禁策略。

---

## 三、优先级路线

### P0：NativeAOT + Source Generator 体验扎实

**目标**：让用户能用 CLI / template / sample 快速跑起 Source Generator NativeAOT，并在不支持的签名、缺失生成物或 fallback 场景下得到明确诊断。

**当前边界**：

- 支持目标是 **Source Generator NativeAOT**。
- DynamicProxy 仍依赖 `Reflection.Emit` / `DynamicMethod`，在 NativeAOT 下不可用。
- 开放泛型、byref-like、反射 metadata、manual registry 等边界必须以诊断和文档明确呈现。

**具体行动**：

1. 完善 Source Generator 编译期诊断：
   - byref-like 代理目标继续报 ACSG008。
   - byref-like `params` 参数继续报 ACSG009。
   - 非 `params` byref-like 参数新增专门诊断，避免生成后在 `object[]` 参数管道中失败。
   - byref-like 返回值新增专门诊断，避免生成后在 `object ReturnValue` 管道中失败。
   - 开放泛型方法的 NativeAOT fallback 继续提示 `[AspectCoreGenericHint]`。
2. 增强 NativeAOT 可运行体验：
   - 保持 `tests/AspectCore.NativeAot.E2E` 作为最小可发布证明。
   - 提供用户可复制的最小配置片段：analyzer 引用、`ProxyEngine.SourceGenerator` / `Strict`、manual registry、`PublishAot`。
   - 给 `dotnet publish` 失败场景补 troubleshooting：缺 registry、缺 metadata、命中 DynamicProxy fallback、byref-like 签名。
3. 建立诊断目录：
   - 每个 `ACSGxxx` 诊断都有原因、触发示例、修复建议、是否影响 NativeAOT 的说明。
   - 中英文文档保持一致。
4. 保持 CI 证明：
   - NativeAOT workflow 必须 publish + run。
   - Source Generator 诊断测试必须在 `AspectCore.Core.Tests` 中覆盖。
   - NativeAOT 专用覆盖率门禁独立于全仓普通覆盖率：unit scope 不低于 100%，E2E scope 不低于 95%。

**验收标准**：

- NativeAOT E2E publish/run 稳定通过。
- 常见不可 AOT/不可 SG 的签名在编译期给出 `ACSGxxx` 诊断，而不是运行时崩溃。
- NativeAOT unit coverage 达到 100%，NativeAOT E2E coverage 达到 95%。
- 用户文档能从零配置到可运行 NativeAOT 示例。
- 文档明确区分 Source Generator NativeAOT 与 DynamicProxy 非 AOT。

### P1：Castle / Windsor 迁移承接

**目标**：承接 Castle DynamicProxy / Windsor 存量用户，把 AspectCore 做成现代 C# 与 AOT 迁移路径，而不是另一个并列框架。

**具体行动**：

1. 补齐 `AspectCore.Extensions.CastleCompat` 的 solution 体验、包说明和示例。
2. 提供 Roslyn analyzer 或 CLI 工具，扫描：
   - `Castle.DynamicProxy.IInterceptor` / `IInvocation` 使用点。
   - Windsor `Component.For().ImplementedBy().Interceptors()` 注册链。
   - Castle-specific lifecycle、selector、mixin、`IChangeProxyTarget` 等不可迁移点。
3. 输出机器生成的迁移 checklist：
   - 可直接迁移项。
   - 需要人工判断项。
   - 不支持项与替代建议。
4. 提供一组可运行 before/after sample。

**验收标准**：

- 示例 Windsor 项目能生成可操作迁移报告。
- 迁移报告覆盖 interceptor API、容器注册、生命周期、selector、unsupported feature。
- Castle 兼容垫片有完整测试，并被 CI build/test 覆盖。

### P2：官方横切能力包

**目标**：降低用户第一次使用成本，但不把 AspectCore 变成业务框架。

**建议包范围**：

- `AspectCore.Extensions.OpenTelemetry`：method span、exception tags、duration metrics。
- `AspectCore.Extensions.HttpClient`：出站调用 trace context / audit hooks。
- `AspectCore.Extensions.Validation`：DataAnnotations / FluentValidation 接入。
- `AspectCore.Extensions.Caching`：`IMemoryCache` / `IDistributedCache` 接入。
- `AspectCore.Extensions.Resilience`：基于 Polly 的 retry / timeout / circuit breaker。

**约束**：

- 优先复用 OpenTelemetry、Polly、Microsoft.Extensions.*，不自造标准。
- 每个包必须有最小 sample、行为测试、性能边界说明。
- 不把 transaction / ORM 等高风险语义作为第一批官方包。

### P3：工程化与信任资产

**目标**：让用户能判断 AspectCore 是否适合生产引入。

**具体行动**：

1. Benchmark dashboard：
   - AspectCore DynamicProxy / Source Generator / Castle 长期对比。
   - 首次调用、稳态、分配、NativeAOT 体积和启动耗时。
   - 结果发布到 docs，CI 至少保证 benchmark 能编译。
2. Compatibility matrix：
   - TFM、C# 特性、容器、Source Generator、NativeAOT、trim 的支持表。
3. Diagnostic catalog：
   - `ACSGxxx` 诊断目录与修复建议。
4. Sample gallery：
   - Web API、Worker、Aspire、NativeAOT CLI、OpenTelemetry、Castle migration。
5. Upgrade guide：
   - 2.x 到 3.x 的行为边界、Source Generator opt-in、AOT 限制。

---

## 四、风险评估

| 风险 | 等级 | 说明 | 缓解策略 |
|------|------|------|----------|
| NativeAOT 体验不可信 | 最高 | 只要用户遇到运行时崩溃或不可解释的 publish failure，就很难相信 Source Generator 路径 | 把失败前移到 `ACSGxxx` 诊断；NativeAOT E2E 必须 publish + run；明确 DynamicProxy 非 AOT |
| 迁移成本过高 | 高 | Castle/Windsor 用户不是因为框架名迁移，而是因为 AOT、性能和现代 C# 需求迁移 | analyzer/CLI 输出迁移报告；保留兼容垫片；提供 before/after sample |
| 横切包范围失控 | 中 | 官方包过多会变成维护负担，并把项目拖向应用框架 | 第一批只做基础设施包；复用成熟生态；高风险语义延后 |
| benchmark 不可复现 | 中 | 宣传性能但无可复现结果会损害信任 | BenchmarkDotNet 工程、环境说明、结果归档、CI 编译门禁 |

---

## 五、决策原则

1. **平台方向优先**：NativeAOT / trimming / Source Generator 是主线。
2. **迁移价值优先**：优先承接已有 Castle/Windsor 用户的真实迁移成本。
3. **生态复用优先**：OpenTelemetry、Polly、Microsoft.Extensions.* 这类成熟标准优先，不自造协议。
4. **可诊断优先**：无法支持的场景必须有清晰诊断、文档和替代路径。
5. **兼容优先**：默认运行时行为不轻易改变；Source Generator / NativeAOT 以显式 opt-in 和 `Strict` 模式推进。

---

*本文档将根据项目进展与市场变化定期更新。建议每季度回顾一次优先级与风险评估。*
