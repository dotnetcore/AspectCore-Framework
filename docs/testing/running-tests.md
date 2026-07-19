# 运行测试

本文给出在本地运行与筛选 AspectCore 测试、并采集覆盖率的具体命令。测试分类与覆盖率门槛见 [测试策略](./testing-strategy.md)。

## 1. 运行完整测试套件

在仓库根目录对整个解决方案运行：

```bash
dotnet test AspectCore-Framework.sln
```

> CI 实际是遍历 `./tests` 下每个 `*.csproj` 逐个 `dotnet test --configuration Release`（见 `.github/workflows/build-pr-ci.yml`）。本地用解决方案范围命令更方便；要贴近 CI 可加 `-c Release`。

## 2. 只运行单个测试项目

```bash
# 核心单元测试
dotnet test tests/AspectCore.Core.Tests/AspectCore.Core.Tests.csproj

# 端到端测试
dotnet test tests/AspectCore.E2E.Tests/AspectCore.E2E.Tests.csproj

# 某个容器集成测试
dotnet test tests/AspectCore.Extensions.Autofac.Test/AspectCore.Extensions.Autofac.Test.csproj
```

## 3. 按名称筛选用例（`--filter`）

用 `FullyQualifiedName~` 做子串匹配，只跑关心的用例：

```bash
# 只跑双引擎一致性（EngineParity）相关测试
dotnet test tests/AspectCore.Core.Tests/AspectCore.Core.Tests.csproj \
  --filter "FullyQualifiedName~EngineParity"

# 只跑 ref 返回值一致性测试
dotnet test tests/AspectCore.Core.Tests/AspectCore.Core.Tests.csproj \
  --filter "FullyQualifiedName~RefReturnParityTests"

# 只跑某个 E2E 场景
dotnet test tests/AspectCore.E2E.Tests/AspectCore.E2E.Tests.csproj \
  --filter "FullyQualifiedName~AsyncScenarios"
```

## 4. 指定目标框架（`-f`）

测试项目多目标，默认会在全部目标框架上各跑一遍。加 `-f` 只在单个框架上运行，缩短反馈时间：

```bash
# 仅 net8.0（Core.Tests 目标为 net10.0;net9.0;net8.0;net6.0）
dotnet test tests/AspectCore.Core.Tests/AspectCore.Core.Tests.csproj -f net8.0

# 仅 net9.0
dotnet test tests/AspectCore.E2E.Tests/AspectCore.E2E.Tests.csproj -f net9.0
```

> 只有目标框架列表中声明、且本地已安装对应运行时的框架才能运行。各测试项目的目标框架见 [测试策略](./testing-strategy.md)。

## 5. 采集覆盖率

测试项目已通过 `tests/Directory.Build.props` 引入 `coverlet.msbuild`，可直接用 MSBuild 属性采集 cobertura 覆盖率：

```bash
dotnet test tests/AspectCore.Core.Tests/AspectCore.Core.Tests.csproj -f net9.0 \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=cobertura \
  /p:CoverletOutput=./TestResults/
```

CI 用 `.github/scripts/check-coverage.sh` 在 `net9.0` 上采集并断言门槛（单元 95% / E2E 80%）。本地可直接复用该脚本预演：

```bash
# 采集单元测试覆盖率结果
./.github/scripts/check-coverage.sh collect unit --output coverage-results/unit.env
# 断言是否达标
./.github/scripts/check-coverage.sh assert unit --input coverage-results/unit.env

# E2E 同理
./.github/scripts/check-coverage.sh collect e2e --output coverage-results/e2e.env
./.github/scripts/check-coverage.sh assert e2e --input coverage-results/e2e.env
```

> 脚本会在测试项目目录下生成 `TestResults/*.cobertura.xml` 并取 `line-rate`。它按源程序集过滤覆盖率，E2E 仅统计 `AspectCore.Core` 与 `AspectCore.Abstractions`（详见 [测试策略](./testing-strategy.md)）。

## 相关文档

- [测试策略](./testing-strategy.md) — 测试分类与覆盖率门槛
- [本地构建](../development/building.md) — 还原、编译、目标框架
- [贡献指南](../development/contributing.md) — 提交前验证与 PR 必需检查
- [文档首页](../README.md)
