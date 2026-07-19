# 项目结构

本文说明 AspectCore 仓库的顶层目录布局与各目录的职责。包（package）级别的职责边界、公开入口与依赖方向不在此重复，详见 [模块与包结构设计](../architecture/module-design.md)。

## 1. 顶层目录一览

| 目录 | 用途 |
|------|------|
| `src/` | 14 个可发布的源码包（核心 + 扩展 + 编译时引擎） |
| `tests/` | xUnit 测试项目：单元、双引擎一致性、E2E、反射、各容器集成 |
| `sample/` | 可运行的示例项目 |
| `benchmark/` | 早期的 BenchmarkDotNet 基准项目（Core、Reflection） |
| `benchmarks/` | 新的统一基准项目 `AspectCore.Benchmarks` |
| `docs/` | 本文档（中文为主，英文见 `docs/en/`） |
| `build/` | 版本、签名、公共包属性与 Cake 构建脚本 |
| `.github/` | CI 工作流与覆盖率脚本 |

根目录还包含解决方案与工作区文件：`AspectCore-Framework.sln`、`NuGet.config`、`LICENSE`、`README.md`、`build.cake`/`build.ps1`。

## 2. `src/` — 源码包

`src/` 下共 14 个包（不含 `Directory.Build.props`）。按角色可分为三类；各包的职责与依赖方向见 [模块与包结构设计](../architecture/module-design.md)。

- 核心：`AspectCore.Abstractions`、`AspectCore.Core`、`AspectCore.Extensions.Reflection`
- 编译时引擎：`AspectCore.SourceGenerator`（`netstandard2.0`，供 Roslyn 加载）
- 扩展与集成：`AspectCore.Extensions.DependencyInjection`、`AspectCore.Extensions.Autofac`、`AspectCore.Extensions.Windsor`、`AspectCore.Extensions.LightInject`、`AspectCore.Extensions.Hosting`、`AspectCore.Extensions.AspNetCore`、`AspectCore.Extensions.Configuration`、`AspectCore.Extensions.DataValidation`、`AspectCore.Extensions.DataAnnotations`、`AspectCore.Extensions.AspectScope`

`src/Directory.Build.props` 对所有源码项目启用 .NET 分析器（提示性，不阻断构建）。

## 3. `tests/` — 测试项目

`tests/` 下每个项目对应一类测试目标；测试分类与覆盖率门槛见 [测试策略](../testing/testing-strategy.md)。

| 测试项目 | 覆盖对象 |
|----------|----------|
| `AspectCore.Core.Tests` | 核心单元测试。含 `EngineParity/` 子目录，验证 DynamicProxy 与 Source Generator 两套引擎行为一致（如 `RefReturnParityTests`、`InitRequiredMembersParityTests`、record 类型等）；另有 `DynamicProxy/`、`DependencyInjection/`、`Injector/`、`Configuration/`、`Integrate/`、`Issues/`、`Extensions/`、`Utils/` 等子目录 |
| `AspectCore.E2E.Tests` | 端到端场景测试，用例集中在 `Scenarios/`，公共支撑在 `Fixtures/`（`TestHost.cs`、`TestServices.cs`） |
| `AspectCore.Extensions.Reflection.Test` | 反射扩展测试 |
| `AspectCore.Extensions.Autofac.Test`、`AspectCore.Extensions.Windsor.Test`、`AspectCore.Extensions.LightInject.Test`、`AspectCore.Extensions.Hosting.Tests`、`AspectCore.Extensions.DependencyInjection.Test`、`AspectCore.Extensions.Configuration.Tests` | 各容器 / 宿主 / 配置的集成测试 |

`tests/Directory.Build.props` 为所有测试项目统一引入 `coverlet.msbuild` 覆盖率采集。

## 4. `sample/` — 示例

可运行的演示项目，用于展示典型用法：

- `AspectCore.Extensions.DependencyInjection.ConsoleSample`
- `AspectCore.Extensions.Autofac.Sample`
- `AspectCore.Extensions.DataAnnotations.Sample`
- `AspectCore.Extensions.AspectScope.Sample`

## 5. `benchmark/` 与 `benchmarks/`

仓库存在两个基准目录：

- `benchmark/` — 早期基准：`AspectCore.Core.Benchmark`、`AspectCore.Extensions.Reflection.Benchmark`
- `benchmarks/` — 新的统一基准项目 `AspectCore.Benchmarks`

## 6. `build/` — 构建配置

集中管理版本、签名与公共包属性：`version.props`（产品版本 2.7.0）、`common.props`（包元数据 + `LangVersion=10.0`）、`sign.props` + `aspectcore.snk`（强名称签名），以及 Cake 脚本（`index.cake`、`util.cake`、`version.cake`）。详见 [本地构建](./building.md)。

## 7. `.github/` — CI

- `workflows/build-ci.yml` — `master` 推送时的构建、打包与 MyGet 发布
- `workflows/build-pr-ci.yml` — PR 校验：lint、`build-and-test`（ubuntu/windows）、单元与 E2E 的执行与覆盖率门槛、CodeQL
- `workflows/release.yml` — 发布流程
- `scripts/check-coverage.sh` — 覆盖率采集与门槛断言脚本

CI 详情见 [测试策略](../testing/testing-strategy.md) 与 [贡献指南](./contributing.md)。

## 相关文档

- [模块与包结构设计](../architecture/module-design.md) — 14 个包的职责边界与依赖方向
- [本地构建](./building.md) — 还原、编译、目标框架与构建属性
- [测试策略](../testing/testing-strategy.md) — 测试分类与覆盖率门槛
- [文档首页](../README.md)
