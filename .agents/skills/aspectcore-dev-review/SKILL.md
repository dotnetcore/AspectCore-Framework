---
name: aspectcore-dev-review
description: AspectCore-Framework 开发与 Code Review 规范指导。当在 AspectCore 项目中进行开发、提交 PR 或进行 Code Review 时使用此 skill。
---

# AspectCore 开发与 Review 规范 Skill

## 何时使用

- 在 AspectCore-Framework 仓库中进行任何代码变更时
- 准备提交 PR 前进行自查时
- 对 PR 进行 Code Review 时
- 需要选择正确的 build/test 命令粒度时

## 核心原则

### 1. 先识别工程结构，再动代码

进入仓库后，按以下顺序识别：
1. `AspectCore-Framework.sln` — solution 入口
2. `src/*/*.csproj` — 项目类型与 TargetFramework
3. `build/common.props` — 全局编译约定（LangVersion=10.0、TargetFrameworks）
4. `tests/Directory.Build.props` — 测试覆盖率配置
5. `.github/workflows/*.yml` — CI/CD 行为

**关键：** 不要跳过识别步骤直接改代码。

### 2. 根据改动范围选择命令粒度

| 改动类型 | 命令粒度 |
|---------|---------|
| 改 `build/common.props` 或公共接口 | solution 级 `dotnet build/test` |
| 改单个类库 | project 级 `dotnet build` + 依赖它的测试项目 |
| 改单个测试类 | `--filter` 精确到那个类 |
| 新 clone / 切分支 | solution 级 `dotnet restore` |

### 3. 遵守架构约束（CRITICAL）

- **依赖方向：** `Abstractions ◄── Core ◄── Extensions`，永不反向
- **公共 API：** 接口/属性放 `AspectCore.Abstractions`，实现放 Core/Extensions
- **双引擎一致性：** DynamicProxy 和 Source Generator 行为必须一致
- **切面激活：** 服务解析必须经过 `IAspectActivatorFactory`，不绕过拦截器

### 4. 代码风格

- block-scoped namespaces（非 file-scoped）
- 私有字段 `_` 前缀
- 公共 API 必须有 `/// <summary>` XML 文档
- 提交前运行 `dotnet format`

## 开发流程

### 变更前
1. 识别工程结构与目标框架
2. 确认改动范围与影响面
3. 选择正确的命令粒度

### 变更中
1. 遵守依赖方向与公共 API 位置
2. 保持双引擎一致性
3. 热路径控制分配，避免 LINQ 链式枚举
4. 正确传播 `CancellationToken`

### 变更后
1. 先写最小失败测试复现问题（如适用）
2. 运行 `--filter` 精确测试
3. 扩大到 project 级测试
4. 涉及公共接口时跑 solution 级验证
5. 运行 `dotnet format` 检查风格

## Code Review 检查清单

### BLOCKING（必须修复）
- [ ] 依赖方向未被破坏
- [ ] 公共 API 在 `AspectCore.Abstractions`
- [ ] 双引擎行为一致
- [ ] 切面激活管道未被绕过
- [ ] 覆盖率达标（unit 95% / E2E 80%）
- [ ] 所有 TFM 编译通过
- [ ] CI 全部通过

### 建议改进
- [ ] 热路径无不必要分配
- [ ] 无 `!` 强压 nullable 警告
- [ ] 无 `.Result`/`.Wait()` 同步阻塞
- [ ] 性能优化有 benchmark 证据

## 常用命令

```bash
# Solution 级
dotnet restore AspectCore-Framework.sln
dotnet build AspectCore-Framework.sln -c Release

# Project 级
dotnet build src/AspectCore.Core/AspectCore.Core.csproj -c Release

# 测试（带覆盖率）
dotnet test ./tests/AspectCore.Core.Tests/AspectCore.Core.Tests.csproj \
  --configuration Release -f net9.0 \
  /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura \
  /p:CoverletOutput=./TestResults/ "/p:Include=[AspectCore.Core]*"

# 精确测试
dotnet test tests/AspectCore.Core.Tests/AspectCore.Core.Tests.csproj \
  --filter "FullyQualifiedName~ServiceResolverTests"

# 格式检查
dotnet format src/AspectCore.Core/AspectCore.Core.csproj --verify-no-changes
```

## 参考文档

- [开发规范](docs/development/development-guidelines.md)
- [Code Review 规范](docs/development/code-review-guidelines.md)
- [AGENTS.md](AGENTS.md)
- [C# / .NET 工程能力检查清单](https://bytedance.larkoffice.com/wiki/JqL7w44B9i5kezkinfKc1zDDnrf)
