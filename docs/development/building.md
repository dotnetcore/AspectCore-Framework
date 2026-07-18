# 本地构建

本文说明如何在本地还原、编译与测试 AspectCore，并解释多目标框架与构建属性（build props）的组织方式。所有命令与配置均以仓库根目录的 `AspectCore-Framework.sln` 及 `build/`、`src/`、`tests/` 下的实际文件为准。

## 1. 环境准备

仓库根目录没有 `global.json`，因此不锁定具体 SDK 版本，但目标框架决定了你需要安装的 SDK：

- 测试项目 `AspectCore.Core.Tests` 与 `AspectCore.E2E.Tests` 目标框架为 `net10.0;net9.0;net8.0;net6.0`。要编译并运行完整测试，需要安装 **.NET 10 SDK**。
- 源码包最低目标框架为 `net6.0`（部分包还包含 `netstandard2.0`/`netstandard2.1`），运行多目标构建时需要相应的 .NET 6/7/8/9 运行时。
- CI 通过 `actions/setup-dotnet` 显式安装以下 SDK：`6.0.x`、`8.0.x`、`9.0.x`、`10.0.x`（见 `.github/workflows/build-ci.yml` 与 `.github/workflows/build-pr-ci.yml`）。本地对齐这几个版本即可覆盖所有目标框架。

验证本地 SDK：

```bash
dotnet --list-sdks
dotnet --version
```

## 2. 还原、编译、测试（解决方案范围）

在仓库根目录对整个解决方案操作：

```bash
# 还原 NuGet 依赖
dotnet restore AspectCore-Framework.sln

# 编译（Release 与 CI 一致）
dotnet build AspectCore-Framework.sln -c Release

# 运行全部测试
dotnet test AspectCore-Framework.sln
```

> CI 并不直接对解决方案调用 `dotnet build`，而是遍历 `./src` 与 `./tests` 下的每个 `*.csproj` 逐个 `build`/`test`（见 `build-ci.yml` 的 `Build`/`Run Tests` 步骤）。本地用解决方案范围的命令更方便；如需精确复现 CI，可按项目粒度执行。

## 3. 按项目 / 按条件运行（更窄的范围）

调试单个模块时，直接指定项目文件，避免全量编译：

```bash
# 只编译核心包
dotnet build src/AspectCore.Core/AspectCore.Core.csproj -c Release

# 只测试核心单元测试项目
dotnet test tests/AspectCore.Core.Tests/AspectCore.Core.Tests.csproj

# 只跑双引擎一致性（EngineParity）相关用例
dotnet test tests/AspectCore.Core.Tests/AspectCore.Core.Tests.csproj \
  --filter "FullyQualifiedName~EngineParity"

# 只在单个目标框架上测试，缩短反馈时间
dotnet test tests/AspectCore.Core.Tests/AspectCore.Core.Tests.csproj -f net8.0
```

更多筛选与覆盖率采集示例见 [运行测试](../testing/running-tests.md)。

## 4. 目标框架说明

不同项目按用途选择目标框架，具体以各 `*.csproj` 为准：

| 项目 | 目标框架 | 说明 |
|------|----------|------|
| `AspectCore.Abstractions`、`AspectCore.Core`、`AspectCore.Extensions.Reflection` | `net9.0;net8.0;net7.0;net6.0;netstandard2.1;netstandard2.0` | 核心包多目标，兼容 .NET Framework（经 netstandard2.0） |
| `AspectCore.SourceGenerator` | `netstandard2.0` | 编译时引擎需以 `netstandard2.0` 供 Roslyn 加载；`LangVersion=latest` |
| `AspectCore.Extensions.AspNetCore` | `net9.0;net8.0;net7.0;net6.0` | 仅 `net6.0` 及以上 |
| 容器/宿主等扩展包 | 以各 `*.csproj` 为准（多为 `net6.0` 及以上，含 netstandard 目标） | 详见 [项目结构](./project-structure.md) 与 [模块与包结构设计](../architecture/module-design.md) |
| 测试项目 | 多为 `net10.0;net9.0;net8.0;net6.0` 或 `net9.0;net8.0;net6.0` | 具体差异见 [测试策略](../testing/testing-strategy.md) |

多目标构建会为每个目标框架各产出一份程序集；因此本地缺少某个运行时会导致该目标框架的编译或测试步骤失败。

## 5. 构建属性（build props）布局

构建配置集中在 `build/` 目录与两个 `Directory.Build.props`，各 `*.csproj` 通过 `Import` 引入：

- `build/version.props` — 产品版本。当前 `VersionMajor=2`、`VersionMinor=7`、`VersionPatch=0`，`VersionQuality` 为空，因此 `VersionPrefix=2.7.0`。CI 在无 Git tag 时追加 `-preview-<时间戳>`。
- `build/common.props` — 公共包元数据（`Authors=Lemon`、`Product=AspectCore Framework`、仓库地址等），并 `Import` 了 `sign.props` 与 `version.props`。其中设置 `LangVersion=10.0`；注释说明：10.0 是最低目标框架 `net6.0` 支持的最新稳定 C# 版本。核心包源码即在此约束下编写。
- `build/sign.props` 与 `build/aspectcore.snk` — 强名称签名配置与密钥。
- `src/Directory.Build.props` — 对 `src/` 下所有项目启用 .NET 分析器（`EnableNETAnalyzers=true`、`AnalysisLevel=latest`、`AnalysisMode=Default`、`EnforceExtendedAnalyzerRules=true`）。这些是提示性诊断，不作为硬性失败门槛。
- `tests/Directory.Build.props` — 为所有测试项目统一引入 `coverlet.msbuild`（版本 `6.0.2`，`PrivateAssets=all`），用于覆盖率采集。

> 仓库根目录没有 `Directory.Build.props`；`src/` 与 `tests/` 各自的 props 仅作用于其子目录。`AspectCore.SourceGenerator` 单独把 `LangVersion` 覆盖为 `latest`。

## 相关文档

- [项目结构](./project-structure.md) — 源码、测试、示例、基准目录布局
- [贡献指南](./contributing.md) — 分支、提交、PR 流程
- [运行测试](../testing/running-tests.md) — 测试筛选与覆盖率采集
- [模块与包结构设计](../architecture/module-design.md) — 14 个包的职责与依赖方向
- [文档首页](../README.md)
