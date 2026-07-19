# AspectCore-Framework Code Review 规范

> 本文档定义 AspectCore-Framework 项目的 Code Review 规范。所有 PR 在合并前都应通过 Review。
> 参考：[C# / .NET AI 工程师工程能力检查清单](https://bytedance.larkoffice.com/wiki/JqL7w44B9i5kezkinfKc1zDDnrf) (internal reference, not publicly accessible)

---

## 1. Review 流程

1. **作者自查**：提交 PR 前，对照本文档检查清单逐项自查
2. **自动化检查**：CI 运行 build、test、coverage、format 检查
3. **人工 Review**：至少 1 名维护者 Review，重点关注架构、设计和正确性
4. **修复反馈**：作者根据 Review 评论修复，重新提交
5. **合并**：Review 通过后 squash-merge 到 master

---

## 2. Review 维度

### 2.1 工程结构与命令

| 检查项 | 说明 |
|--------|------|
| 改动范围是否合理 | 改公共接口 → solution 级验证；改单个文件 → project 级验证 |
| 是否影响多目标框架 | 代码是否在所有 TFM（net9.0/8.0/7.0/6.0/netstandard2.1/2.0）下都能编译 |
| 是否修改了 `build/common.props` | 如有，是否所有项目都受影响并验证 |
| 命令是否与 CI 对齐 | 本地 build/test 命令是否匹配 `.github/workflows/` 中的 CI 行为 |

### 2.2 架构与设计

| 检查项 | 说明 |
|--------|------|
| 依赖方向是否正确 | `Abstractions ◄── Core ◄── Extensions`，永不反向 |
| 公共 API 是否在 Abstractions | 接口/属性放 `AspectCore.Abstractions`，实现放 Core/Extensions |
| 是否破坏双引擎一致性 | DynamicProxy 和 Source Generator 行为是否保持一致 |
| 切面激活管道是否被绕过 | 服务解析是否经过 `IAspectActivatorFactory` |
| 命名是否符合约定 | 接口 `I` 前缀、拦截器 `Interceptor`/`InterceptorAttribute` 后缀 |

### 2.3 代码质量

| 检查项 | 说明 |
|--------|------|
| nullable 语义是否正确 | 测试项目启用 nullable，不要用 `!` 强压警告 |
| async/await 是否正确 | 不用 `.Result`/`.Wait()`，`CancellationToken` 沿调用链传播 |
| 热路径是否有不必要分配 | 避免 LINQ 链式枚举、字符串拼接、闭包捕获 |
| 异常是否用于正常分支 | 用 `TryParse` 而非 `try/catch` 做正常流程控制 |
| 格式是否符合 `dotnet format` | 提交前运行 `dotnet format` |

### 2.4 测试与覆盖率

| 检查项 | 说明 |
|--------|------|
| 是否有对应测试 | 新功能/修复必须有测试覆盖 |
| 覆盖率是否达标 | unit 95%、E2E 80%（CI 门禁） |
| 测试是否最小化 | 优先用 `--filter` 定位，而非全量跑 |
| 双引擎是否都测试 | 关键路径需同时覆盖 DynamicProxy 和 Source Generator |
| 回归测试是否先于修复 | 先写失败测试，再改实现 |

### 2.5 性能与稳定性

| 检查项 | 说明 |
|--------|------|
| 是否有 benchmark 证据 | 声称性能优化需提供 BenchmarkDotNet 数据 |
| 是否考虑 GC 压力 | 热路径分配是否可控 |
| 是否考虑线程安全 | 单例/缓存是否线程安全 |
| 是否考虑 NativeAOT | 是否引入反射/动态代码生成依赖 |

---

## 3. BLOCKING 问题（必须修复才能合并）

以下问题在 Review 中标记为 BLOCKING，必须修复：

1. **破坏依赖方向**：`Abstractions ◄── Core ◄── Extensions` 被反向
2. **公共 API 不在 Abstractions**：接口/属性错误地放在实现项目中
3. **绕过切面激活管道**：服务解析未经过拦截器管道
4. **破坏双引擎一致性**：DynamicProxy 和 Source Generator 行为不一致
5. **覆盖率不达标**：unit < 95% 或 E2E < 80%
6. **多目标框架编译失败**：任一 TFM 编译不过
7. **CI 失败**：build/test/format 任一检查不过
8. **测试缺失**：核心功能变更无对应测试

---

## 4. 建议改进项（非阻塞，但应考虑）

- 热路径中的 LINQ 链式枚举可改为单次遍历
- `ValueTask` 仅在结果经常同步完成时使用
- 配置对象与领域对象分离，不混用
- 日志使用结构化模板，避免热路径字符串插值
- 公共 API 有 XML 文档注释（`/// <summary>`）

---

## 5. Review 评论格式

Review 评论应包含：

- **严重程度**：`BLOCKING` / `MAJOR` / `MINOR` / `NIT`
- **具体位置**：文件路径 + 行号
- **问题描述**：清楚说明问题是什么
- **建议方案**：给出可行的修复建议

示例：
```
[BLOCKING] src/AspectCore.Core/DependencyInjection/ServiceTable.cs:45
keyed 查找失败时直接 return null，未 fall through 到 generic 分支。
建议：当 keyed 匹配失败时，继续执行 generic 查找逻辑。
```

---

## 6. 自查清单（提交 PR 前）

### 工程结构
- [ ] 改动范围与验证范围匹配（改公共接口 → solution 级）
- [ ] 所有 TFM 编译通过
- [ ] 本地命令与 CI 对齐

### 架构
- [ ] 依赖方向 `Abstractions ◄── Core ◄── Extensions` 未被破坏
- [ ] 公共 API 在 `AspectCore.Abstractions`
- [ ] 双引擎行为一致
- [ ] 切面激活管道未被绕过

### 代码
- [ ] 无 `!` 强压 nullable 警告
- [ ] 无 `.Result`/`.Wait()` 同步阻塞
- [ ] 热路径无不必要分配
- [ ] `dotnet format` 通过

### 测试
- [ ] 新功能/修复有测试
- [ ] 覆盖率达标（unit 95% / E2E 80%）
- [ ] 双引擎都有覆盖
- [ ] 先写失败测试再改实现

### 性能
- [ ] 性能优化有 benchmark 证据
- [ ] 无热路径分配问题
