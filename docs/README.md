# AspectCore 文档

> AspectCore 是面向 .NET Core 与 .NET Framework 的跨平台 AOP（面向切面编程）框架，提供动态代理拦截、依赖注入集成、Web 应用支持、数据校验等能力。

本目录是 AspectCore 的完整文档。中文为主，英文版本见 [`en/`](./en/README.md)（English documentation lives under [`en/`](./en/README.md)）。

## 快速导航

### 🚀 入门（getting-started）
从零开始使用 AspectCore。

- [安装](./getting-started/installation.md) — NuGet 包与目标框架
- [快速上手](./getting-started/quick-start.md) — 五分钟跑通第一个拦截器
- [核心概念](./getting-started/concepts.md) — 拦截器、代理、切面上下文等术语

### 📖 使用指南（guide）
面向日常开发的功能说明。

- [拦截器基础](./guide/interceptor.md) — 定义拦截器、特性拦截器、全局拦截器
- [拦截器配置](./guide/interceptor-configuration.md) — 三种注册方式与作用范围谓词
- [异步拦截](./guide/async-interception.md) — Task / ValueTask / IAsyncEnumerable
- [条件拦截](./guide/conditional-interception.md) — 按命名空间/服务/方法匹配
- [依赖注入集成](./guide/dependency-injection.md) — 内置容器与 Microsoft.Extensions.DependencyInjection
- [第三方容器](./guide/third-party-containers.md) — Autofac / Windsor / LightInject
- [配置注入](./guide/configuration-injection.md) — 从 IConfiguration 绑定值
- [数据校验](./guide/data-validation.md) — DataAnnotations 校验拦截
- [反射扩展](./guide/reflection-extensions.md) — AspectCore.Extensions.Reflection 高性能反射
- [常见场景](./guide/common-scenarios.md) — 日志、缓存、重试、性能监控等

### 🏛 架构设计（architecture）
面向贡献者与深度使用者的设计文档。

- [总体架构](./architecture/overview.md) — 分层与运行流程
- [模块与包结构设计](./architecture/module-design.md) — 14 个包的职责边界与依赖方向
- [DynamicProxy 运行时引擎](./architecture/dynamic-proxy.md) — 基于 Reflection.Emit 的运行时代理
- [Source Generator 编译时引擎](./architecture/source-generator.md) — 基于 Roslyn 的编译时代理
- [两套引擎对比与选型](./architecture/engine-comparison.md) — DynamicProxy vs SourceGenerator vs Auto
- [C# 语言特性适配](./architecture/language-features.md) — C# 6 ~ C# 13 特性在 AOP Emit 中的适配
- [Record 类型支持](./architecture/record-support.md) — C# 9 record 代理的两引擎差异

### 🔧 开发指南（development）
面向本仓库贡献者。

- [本地构建](./development/building.md) — 还原、编译、目标框架
- [项目结构](./development/project-structure.md) — 源码、测试、示例、基准布局
- [贡献指南](./development/contributing.md) — 分支、提交、PR 流程
- [开发规范](./development/development-guidelines.md) — 工程结构识别、命令粒度、测试、性能、设计原则
- [Code Review 规范](./development/code-review-guidelines.md) — Review 维度、BLOCKING 问题、自查清单

### ✅ 测试（testing）
- [测试策略](./testing/testing-strategy.md) — 单元、双引擎一致性、E2E、覆盖率门槛
- [运行测试](./testing/running-tests.md) — 如何运行与筛选测试

## 版本说明

本文档基于源码扫描生成，力求与代码一致。文中区分「已实现能力」与「设计/提案」，未实现的内容会显式标注。产品版本见 `build/version.props`。
