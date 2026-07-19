# AspectCore-Framework 开发规范

> 本文档定义 AspectCore-Framework 项目的开发规范。所有代码变更、PR 提交都应遵循这些规范。
> 参考：[C# / .NET AI 工程师工程能力检查清单](https://bytedance.larkoffice.com/wiki/JqL7w44B9i5kezkinfKc1zDDnrf)

---

## 1. 先理解 MSBuild 工程拓扑，再动代码

AspectCore 是一个由 MSBuild、NuGet、TargetFramework 和宿主框架共同决定行为的系统，而不是"若干独立 C# 文件的集合"。

| 对象 | 作用 | 为什么重要 |
|------|------|-----------|
| `AspectCore-Framework.sln` | solution 级入口，决定项目集合与执行边界 | 命令应该跑在 solution 级还是 project 级，首先看解空间结构 |
| `src/*/*.csproj` | 项目类型、TargetFramework、包引用、生成设置 | 几乎所有编译行为都在这里定义 |
| `build/common.props` | 全局编译约定（LangVersion、TargetFrameworks、分析器） | 改了这里影响所有项目 |
| `tests/Directory.Build.props` | 测试项目共享配置（coverlet.msbuild 注入） | 测试覆盖率配置来源 |
| `.github/workflows/*.yml` | CI/CD 定义 | 本地命令应与 CI 行为对齐 |

**关键判断点：** 不要跳过这些识别步骤。

---

## 2. 能根据上下文选择修改层级

- 改 solution 级配置（`build/common.props`）→ 影响所有项目，需 solution 级 build/test
- 改 project 级配置（`csproj`）→ 影响该项目及其依赖者
- 改单个源码文件 → 优先 project 级 + 依赖它的测试项目
- 只补一条测试 → `--filter` 精确到那个测试类

**常见错误：**
- 仓库多目标框架（`net9.0;net8.0;net7.0;net6.0;netstandard2.1;netstandard2.0`），只按单一 TFM 写 API → 错误
- 改了 `build/common.props` 却只编一个项目 → 错误

---

## 3. 代码生成要符合 .NET 语义，而不是只像 C#

- 正确传播 `CancellationToken`，而不是只在最外层签名里"摆一个参数"
- 区分 `Task` 与 `ValueTask` 的适用边界
- 理解 nullable reference types 的契约意义（本项目 `src/` 未全局启用 nullable，但测试项目已启用）
- 在热路径中控制分配、避免隐藏装箱与枚举器开销
- 遇到性能或稳定性问题时，先建议 benchmark / profiling / tracing，而不是拍脑袋重写

---

## 4. 项目进入后的识别顺序

### 4.1 先看 solution / project 结构
- `src/` 下的核心库：`AspectCore.Abstractions`、`AspectCore.Core`、`AspectCore.SourceGenerator`
- `src/AspectCore.Extensions.*` 扩展库
- `tests/` 下的测试项目
- `sample/`、`benchmark/` 示例与基准项目

### 4.2 再看 SDK 与目标框架约定
- 无 `global.json`，SDK 未钉死
- `src/` 的 `LangVersion=10.0`（`build/common.props`）
- 测试项目 `LangVersion=13.0`
- 库目标框架：`net9.0;net8.0;net7.0;net6.0;netstandard2.1;netstandard2.0`

### 4.3 识别测试框架与质量工具链
- xUnit `2.9.2` + `Microsoft.NET.Test.Sdk 17.12.0`
- 覆盖率：`coverlet.msbuild 6.0.2`，阈值 unit 95% / E2E 80%
- 无 `.editorconfig`，使用 `dotnet format` 默认风格

---

## 5. 最佳命令实践

### 5.1 先决定执行粒度，再跑命令

| 场景 | 推荐粒度 | 典型命令 |
|------|---------|---------|
| 新 clone、切分支、依赖变化 | solution 级 | `dotnet restore AspectCore-Framework.sln` |
| 改了 `build/common.props` 或公共接口 | solution 级 | `dotnet build AspectCore-Framework.sln -c Release` |
| 只改某个类库 | project 级 | `dotnet build src/AspectCore.Core/AspectCore.Core.csproj -c Release` |
| 只改某个测试项目 | test project 级 | `dotnet test tests/AspectCore.Core.Tests/AspectCore.Core.Tests.csproj` |
| 只验证一个测试类 | test filter 级 | `dotnet test tests/AspectCore.Core.Tests/AspectCore.Core.Tests.csproj --filter "FullyQualifiedName~ServiceResolverTests"` |

**核心原则：先缩小影响面，再缩小命令范围。**

### 5.2 常用命令

```bash
# 还原与构建
dotnet restore AspectCore-Framework.sln
dotnet build src/AspectCore.Core/AspectCore.Core.csproj -c Release

# 测试（带覆盖率，匹配 CI）
dotnet test ./tests/AspectCore.Core.Tests/AspectCore.Core.Tests.csproj \
  --configuration Release -f net9.0 \
  /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura \
  /p:CoverletOutput=./TestResults/ "/p:Include=[AspectCore.Core]*"

# 格式检查
dotnet format src/AspectCore.Core/AspectCore.Core.csproj --verify-no-changes
```

### 5.3 测试过滤

```bash
# 按类名过滤
dotnet test tests/AspectCore.Core.Tests/AspectCore.Core.Tests.csproj \
  --filter "FullyQualifiedName~ServiceResolverTests"

# 按方法名过滤
dotnet test tests/AspectCore.Core.Tests/AspectCore.Core.Tests.csproj \
  --filter "Name~Should_resolve_keyed_service"
```

---

## 6. 测试与质量保障

### 6.1 单元测试：优先覆盖纯逻辑与边界条件
- null / empty / default 输入
- 边界值与异常路径
- async 方法的取消、超时、异常传播

### 6.2 集成测试：关注宿主、配置和外部依赖边界
- DI 装配是否正确
- 切面激活管道是否正常工作
- 双引擎（DynamicProxy + Source Generator）一致性

### 6.3 回归验证：先把问题固化成测试，再改实现
1. 先复现问题
2. 用最小测试固定住问题
3. 只运行相关测试确认失败
4. 修改实现
5. 先跑过滤后的回归测试，再跑受影响项目的完整测试
6. 涉及公共接口或共享组件时，再补 solution 级验证

---

## 7. 高性能实现要点

### 7.1 先看分配，再谈"快"
- 热路径里频繁创建短命对象
- 不必要的字符串拼接、切片、拷贝
- 闭包捕获导致的额外分配
- 装箱（尤其是接口、多态、值类型场景）

### 7.2 LINQ 用在该用的地方，不要默认上热路径
- 冷路径先保可读性，别机械手写循环
- 热路径若已被 benchmark / profiling 证实有问题，优先改成单次遍历、显式循环

### 7.3 async/await：正确性优先，吞吐量其次
- 不要在 async 流程里用 `.Result`、`.Wait()` 做同步阻塞
- `CancellationToken` 应沿调用链传播
- `ValueTask` 只在结果经常同步完成、且已证明值得减少分配时使用

---

## 8. 基于语言特性的设计原则

### 8.1 nullable reference types
- 测试项目已启用 nullable，`string` 和 `string?` 是契约差异
- 不要把 `!` 当"修编译"的万能胶
- 参数、返回值、属性的可空性要反映真实业务语义

### 8.2 DI / options / logging：遵守宿主框架语义
- 构造函数注入优先；不要到处注入 `IServiceProvider` 再手动拉服务
- 注意 singleton / scoped / transient 生命周期边界
- 本项目核心管道：`Abstractions ◄── Core ◄── Extensions`，永远不要反向依赖

### 8.3 公共 API 契约/实现拆分（CRITICAL）
- 接口和属性放 `AspectCore.Abstractions`（无实现依赖）
- 实现放 `AspectCore.Core` 或相关扩展包
- 命名空间匹配包/功能

---

## 9. 常见错误与反模式

### 9.1 把 .NET 仓库当成单文件语言任务
- 不看 `.sln` / `.csproj` / `build/common.props`
- 不看 TargetFrameworks
- 不看测试工程

### 9.2 命令范围过大或过小
- 只改一个测试类，却每次全量 `dotnet test` → 浪费时间
- 改了公共接口，却只跑单个测试项目 → 漏回归

### 9.3 忽略仓库的目标框架约定
- 项目同时面向 `net9.0` 和 `netstandard2.0`，却直接使用只存在于新 TFM 的 API
- 应使用 `#if NET9_0_OR_GREATER` 条件编译，或抽象接口做适配

### 9.4 在热路径里机械使用 LINQ、异常或字符串操作
- 热路径里的 LINQ 链式枚举 → 每次调用分配枚举器
- 用异常做正常分支控制 → 异常抛出和捕获是昂贵操作
- 循环内字符串拼接 → 每次 `+=` 都创建新字符串

---

## 10. 检查清单

### 项目识别
- [ ] 是否识别了 TargetFramework / TargetFrameworks
- [ ] 是否识别了 solution、项目边界、测试项目
- [ ] 是否检查了 `build/common.props`
- [ ] 是否识别了测试框架（xUnit）与覆盖率工具（coverlet）

### 命令选择
- [ ] 是否根据改动范围选择 solution 级、project 级还是 test filter 级命令
- [ ] 是否显式指定了 `.sln` / `.csproj` 路径
- [ ] 是否避免对每次改动默认全量 `dotnet test`

### 测试与质量
- [ ] 是否优先用最小失败测试复现 bug
- [ ] 是否在本地迭代时优先缩小测试范围，在提交前再扩大验证范围
- [ ] 是否验证了双引擎一致性（DynamicProxy + Source Generator）

### 设计与语言特性
- [ ] 是否遵守 `Abstractions ◄── Core ◄── Extensions` 依赖方向
- [ ] 是否把公共接口/属性放在 `AspectCore.Abstractions`
- [ ] 是否使用 block-scoped namespaces（而非 file-scoped）
- [ ] 私有字段是否用 `_` 前缀
