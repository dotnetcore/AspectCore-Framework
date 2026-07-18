# 贡献指南

本文说明向 AspectCore 提交代码的流程与规范，所有规则均以本仓库的实际配置（分支保护、CI 工作流、提交历史与 Git 身份）为准。

## 1. 分支与工作流

- 默认分支为 `master`，也是 PR 的合并目标。
- 从最新的 `origin/master` 拉出特性分支进行开发：

```bash
git fetch origin
git switch -c feat/<简短描述> origin/master
```

- 除非明确需要 stacked PR，不要基于其他特性分支开发。

## 2. 提交规范（Conventional Commits）

提交历史遵循 [Conventional Commits](https://www.conventionalcommits.org/)，常用前缀：`feat:`、`fix:`、`docs:`、`test:`、`ci:`。参考近期提交：

```text
feat: support ref and ref readonly return proxy generation (#385)
feat: support C# 9 record type proxy generation (#384)
ci: split coverage checks by test type (#380)
docs(architecture): complete architecture section
```

- PR 合并为 squash 方式，合并提交标题中带上 PR 编号 `(#number)`。
- 提交信息用一行简明描述改动意图，必要时在正文补充上下文。

## 3. Git 身份

本仓库使用固定的提交身份，提交前先确认本地 `git config`：

```bash
git config user.name    # Haoyang Liu
git config user.email   # liuhaoyang1221@hotmail.com
```

- 提交身份必须匹配仓库配置，不要用命令行 author 参数或环境变量覆盖。
- **不要**添加任何 `Co-Authored-By` trailer。

## 4. 提交前的本地验证

在发起 PR 前，至少在改动所属项目上跑通编译与测试；跨核心引擎、公共契约或多包的改动应扩大验证范围：

```bash
# 解决方案范围
dotnet build AspectCore-Framework.sln -c Release
dotnet test AspectCore-Framework.sln

# 或按改动范围收窄
dotnet test tests/AspectCore.Core.Tests/AspectCore.Core.Tests.csproj
```

命令细节与筛选方式见 [本地构建](./building.md) 与 [运行测试](../testing/running-tests.md)。

> 涉及两套代理引擎的改动，务必让 `EngineParity/` 下的一致性测试通过，确保 DynamicProxy 与 Source Generator 行为一致（背景见 [两套引擎对比与选型](../architecture/engine-comparison.md)）。

## 5. PR 流程与必需检查

PR 面向 `master`，合并需满足分支保护规则：

- squash-only 合并；
- 至少 1 个 approving review；
- 所有 review 讨论线程（threads）已解决；
- 以下必需状态检查全部通过（由 `.github/workflows/build-pr-ci.yml` 产生）：
  - `lint`（`dotnet format --verify-no-changes`）
  - `build-and-test (ubuntu-latest)`
  - `build-and-test (windows-latest)`
  - `Unit Test Execution`
  - `Unit Test Coverage Result`（单元覆盖率门槛 95%）
  - `E2E Test Execution`
  - `E2E Test Coverage Result`（E2E 覆盖率门槛 80%）

覆盖率门槛与检查含义见 [测试策略](../testing/testing-strategy.md)。lint 使用 `dotnet format`，本地可先自查：

```bash
dotnet format AspectCore-Framework.sln --verify-no-changes
# 如有格式问题，运行 dotnet format 自动修复
dotnet format AspectCore-Framework.sln
```

## 相关文档

- [本地构建](./building.md) — 还原、编译、目标框架
- [项目结构](./project-structure.md) — 源码、测试、示例、基准布局
- [测试策略](../testing/testing-strategy.md) — 测试分类与覆盖率门槛
- [运行测试](../testing/running-tests.md) — 测试筛选与覆盖率采集
- [文档首页](../README.md)
