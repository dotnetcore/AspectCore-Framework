# Graph: emit-ast-production-hardening

> Status: completed | Iteration: 3/6

## User Intent
- Original: 看一下当前做的 emit AST 的重构. 帮我继续优化到生产可用,以及完整的验证
- Acceptance: 盘清当前 emit AST 重构状态与风险点；补齐达到生产可用所需的实现优化；完成独立、完整、可复述的验证，并报告剩余风险。

## Understanding
- 用户要在当前仓库中继续推进 emit AST 重构，不是从零开始。
- 成功标准强调“生产可用”和“完整验证”，需要实现与独立验证分离。
- 当前 emit AST 已接管 DynamicProxy 主生成路径，边界是“代理类型/成员/元数据 AST 化 + ILEmitVisitor 输出 IL”，不是方法体逐指令 AST 化。
- 当前主要缺口不是主流程缺失，而是生产证据与硬化不足：interface proxy parity、open generic/generic method 行为覆盖、异步异常边角、proxy emit benchmark、netstandard/跨平台兼容性证据。

## Completed
### n1 @omc-explore — DONE_WITH_CONCERNS
- Summary: emit AST 重构主链路已落地，但还缺生产化验证闭环。
- Findings: 主流程由 ProxyTypeCompiler + *AstBuilder + Nodes + ILEmitVisitor 组成；无旧新实现并存切换；测试偏 class-heavy，interface/generic/异常/性能/兼容性证据不足。
- Deliverables: 核心代码地图、验证入口、最小后续执行清单。

### n2 @omc-deep-executor — DONE_WITH_CONCERNS
- Summary: 补齐 interface/generic/异常/性能/兼容性关键缺口，新增 benchmark，并做代表性本地验证。
- Findings: 修复返回参数位置与 generic parameter attribute 回放；补 interface-only open generic stub 的 generic method 定义；收紧异步返回值 await 语义；新增 proxy emit benchmark。
- Deliverables: 新增/修改 src、tests、benchmark 多处文件；本地验证通过（DynamicProxy 过滤测试、netstandard2.0/2.1 build、ProxyEmitBenchmarks 运行）。

### n3 @omc-verifier — DONE_WITH_CONCERNS
- Summary: 独立验证通过，本轮目标可认为完成，且显著接近生产可用。
- Findings: DynamicProxy 子集、核心测试工程全框架、整仓 solution build/test、netstandard build、ProxyEmitBenchmarks 均通过；未发现新的功能性回归。
- Deliverables: 独立验收结论为 PASS；明确两个残留边界——`ThrowInUncontinuedTasks` 未重验、`AspectCore.Core.csproj` 存在 `.net9.0` 单框架构建瑕疵。

## In Flight

## Pending


## Budget
- Iterations: 3 / 6
