# 测试策略

本文说明 AspectCore 的测试分类、各类测试的用途，以及 CI 中的覆盖率门槛与必需检查。测试项目均位于 `tests/`，基于 xUnit（`xunit` 2.9.2）。

## 1. 测试分类

### 单元测试（`AspectCore.Core.Tests`）

覆盖核心运行时的单元行为：动态代理生成、拦截器管线、依赖注入与注入器、配置等。目录按主题划分（`DynamicProxy/`、`DependencyInjection/`、`Injector/`、`Configuration/`、`Integrate/`、`Issues/`、`Extensions/`、`Utils/` 等）。目标框架为 `net10.0;net9.0;net8.0;net6.0`。

### 双引擎一致性测试（`AspectCore.Core.Tests/EngineParity/`）

AspectCore 提供两套等价的代理生成引擎：运行时的 DynamicProxy 与编译时的 Source Generator。二者共享同一套拦截语义，因此**必须表现一致**——同样的类型与方法，无论由哪套引擎生成代理，拦截结果都应相同。`EngineParity/` 就是为守住这条不变量而存在，覆盖容易在两套实现间产生分歧的语言特性，例如：

- `RefReturnParityTests` — `ref` / `ref readonly` 返回值
- `InitRequiredMembersParityTests` — `init` 与 `required` 成员
- `RefStructAndScopedParityTests` — `ref struct` 与 `scoped`
- `PrimaryConstructorAndParamsCollectionParityTests` — 主构造函数与 `params` 集合
- `InterpolatedStringHandlerAndIndexRangeParityTests` — 插值字符串处理器与 `Index`/`Range`
- record 类型相关一致性
- `SourceGeneratorDynamicProxyParityTests`、`SourceGeneratorEdgeCaseTests`、`SourceGeneratorDiagnosticTests` 等

两套引擎的差异与选型见 [两套引擎对比与选型](../architecture/engine-comparison.md)。

### 端到端测试（`AspectCore.E2E.Tests/Scenarios`）

从用户视角组织的完整场景测试，用例集中在 `Scenarios/`（如基础代理、异步 `Task`/`ValueTask`、DI 集成、配置驱动、错误处理、泛型服务、属性注入、record 类型、`ref` 返回等），公共宿主与服务在 `Fixtures/`（`TestHost.cs`、`TestServices.cs`）。目标框架为 `net10.0;net9.0;net8.0;net6.0`。

### 反射测试（`AspectCore.Extensions.Reflection.Test`）

覆盖 `AspectCore.Extensions.Reflection` 高性能反射扩展。目标框架为 `net9.0;net8.0;net6.0`。

### 各容器 / 宿主集成测试

验证 AspectCore 与第三方容器及宿主的集成：

- `AspectCore.Extensions.Autofac.Test`、`AspectCore.Extensions.Windsor.Test`、`AspectCore.Extensions.LightInject.Test`、`AspectCore.Extensions.Hosting.Tests`、`AspectCore.Extensions.Configuration.Tests` — `net9.0;net8.0;net6.0`
- `AspectCore.Extensions.DependencyInjection.Test` — `net9.0;net6.0`

## 2. 覆盖率工具

所有测试项目通过 `tests/Directory.Build.props` 统一引入 `coverlet.msbuild`（`6.0.2`）采集覆盖率。CI 用 `.github/scripts/check-coverage.sh` 在 `net9.0` 上采集 cobertura 格式的行覆盖率并断言门槛。

采集口径（见脚本 `check-coverage.sh`）：

- 单元覆盖率：遍历 `./tests` 下的 `*.csproj`，排除名称含 `E2E` 的项目；每个测试项目按其对应源程序集（如 `AspectCore.Extensions.Windsor.Test` → `[AspectCore.Extensions.Windsor]*`）过滤后求各项目覆盖率的算术平均。
- E2E 覆盖率：仅 `*E2E*.csproj`，且刻意只统计 `[AspectCore.Core]` 与 `[AspectCore.Abstractions]`（扩展程序集由各自的单元测试项目单独度量）。

## 3. 覆盖率门槛与 CI 必需检查

PR 由 `.github/workflows/build-pr-ci.yml` 校验，覆盖率门槛在 `check-coverage.sh` 中定义：

- 单元测试覆盖率门槛：**95%**（`UT_THRESHOLD=95`）
- E2E 测试覆盖率门槛：**80%**（`E2E_THRESHOLD=80`）

分支保护要求以下状态检查全部通过：

- `lint`
- `build-and-test (ubuntu-latest)`
- `build-and-test (windows-latest)`
- `Unit Test Execution`
- `Unit Test Coverage Result`
- `E2E Test Execution`
- `E2E Test Coverage Result`

覆盖率被拆成「执行」与「门槛断言」两个独立 job：`*-test-execution` 负责跑测试并采集结果，`*-test-coverage-result` 负责断言是否达标。此外 PR 还会运行 CodeQL（C#）分析。合并策略与评审要求见 [贡献指南](../development/contributing.md)。

## 相关文档

- [运行测试](./running-tests.md) — 如何运行与筛选测试、采集覆盖率
- [两套引擎对比与选型](../architecture/engine-comparison.md) — 双引擎一致性的背景
- [贡献指南](../development/contributing.md) — PR 流程与必需检查
- [项目结构](../development/project-structure.md) — 测试项目在仓库中的位置
- [文档首页](../README.md)
