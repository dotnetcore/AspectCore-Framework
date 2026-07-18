# 总体架构

本文说明 AspectCore 的整体架构、分层关系与两条核心执行流程（代理生成、运行时拦截）。术语见 [核心概念](../getting-started/concepts.md)，包级职责见 [模块与包结构设计](./module-design.md)。

## 1. 定位

AspectCore 是一个 AOP 框架，核心能力是**为服务方法织入拦截器**。它提供两套等价的代理生成引擎：

| 引擎 | 织入时机 | 代理生成方式 | 关键包 |
|------|---------|-------------|--------|
| **DynamicProxy** | 运行时 | `System.Reflection.Emit` 动态生成 IL | `AspectCore.Core` |
| **Source Generator** | 编译时 | Roslyn 生成 C# 源码 | `AspectCore.SourceGenerator` |

两套引擎共享同一套抽象契约（`AspectCore.Abstractions`）和同一条拦截器管线语义，因此对使用者而言拦截行为一致。引擎选择见 [两套引擎对比与选型](./engine-comparison.md)。

## 2. 分层

AspectCore 的依赖方向自下而上、单向无环：

```
                 ┌─────────────────────────────────────────────┐
  集成/特性层     │ DI 适配: DependencyInjection / Autofac /      │
                 │          Windsor / LightInject / Hosting      │
                 │ Web:     AspNetCore                            │
                 │ 特性:    AspectScope / DataValidation /        │
                 │          DataAnnotations / Configuration       │
                 └───────────────────┬─────────────────────────┘
                                     │ 依赖
                 ┌───────────────────▼─────────────────────────┐
  运行时核心层    │ AspectCore.Core                              │
                 │  - DynamicProxy 运行时引擎（IL emit）         │
                 │  - IoC 容器（ServiceContext/ServiceResolver）│
                 │  - 拦截器管线与配置                          │
                 └──────────┬───────────────────┬──────────────┘
                            │ 依赖               │ 依赖
       ┌────────────────────▼──────┐   ┌────────▼───────────────────┐
  基础层 │ AspectCore.Abstractions   │   │ AspectCore.Extensions.       │
        │ （契约：接口/抽象/特性）  │   │ Reflection（高性能反射）     │
        └───────────────────────────┘   └──────────────────────────────┘

  编译时引擎（独立）: AspectCore.SourceGenerator（Roslyn 分析器，无项目依赖）
```

- **基础层**：`Abstractions` 只放契约（接口、抽象类、特性、枚举），不含实现；`Extensions.Reflection` 是独立的高性能反射库，不依赖其他 AspectCore 包，反被 `Core` 依赖。
- **运行时核心层**：`Core` 依赖 `Abstractions` + `Reflection`，实现 DynamicProxy 运行时引擎、IoC 容器与拦截器管线。
- **集成/特性层**：所有 DI 适配与特性扩展都依赖 `Core`（少数只依赖 `Abstractions`）。
- **编译时引擎**：`SourceGenerator` 是独立的 Roslyn 分析器（`netstandard2.0`），无项目引用；它生成的代理代码在运行时引用 `Core`/`Abstractions` 的类型。

## 3. 两个核心事实

理解 AspectCore 架构，抓住两个关键事实：

### 事实一：「是否拦截」在代理生成时决定，「拦截器选择/排序」在首次调用时决定

- **代理生成期**：`MethodBodyFactory.DecideBody`（`src/AspectCore.Core/DynamicProxy/ProxyBuilder/Builders/MethodBodyFactory.cs:15`）对每个方法决定生成「直接委托」还是「切面激活」方法体，依据是 `IAspectValidator.Validate`。
- **首次运行调用**：`InterceptorCollector`（`src/AspectCore.Core/DynamicProxy/InterceptorCollector.cs:12`）收集、排序、去重拦截器，`AspectBuilderFactory`（`src/AspectCore.Core/DynamicProxy/AspectBuilderFactory.cs:7`）构建并缓存拦截器管线。

这一拆分意味着：未被判定需要拦截的方法在代理里就是普通转发，零运行时开销；需要拦截的方法在首次调用时才组装管线，之后走缓存。

### 事实二：同一套 `IAspectValidator` 校验链服务于两处

`IAspectValidator` 既用于代理生成期（决定方法体类型），也用于容器注册期（`ServiceValidator` 在 `ServiceTable` 中决定服务是否需要包成代理）。校验链是有序的责任链（`AspectValidatorBuilder`，`src/AspectCore.Core/DynamicProxy/AspectValidatorBuilder.cs:9`）。

## 4. 运行流程一：代理类型生成（DynamicProxy）

```
IProxyTypeGenerator.CreateClassProxyType / CreateInterfaceProxyType
  → ProxyTypeCompiler（单一 ModuleBuilder，按名缓存类型）
    → AST 构建：ClassProxyBuilder / InterfaceProxyBuilder / InterfaceImplBuilder
        产出 ProxyTypeNode（字段/构造器/方法/属性节点树）
        每方法经 MethodBodyFactory.DecideBody 选择方法体节点：
          - DirectDelegationBody / ReflectorDelegationBody（不拦截）
          - AspectActivatorBody（拦截）
          - StubBody / RecordCloneBody / BackingFieldGet|SetBody
    → ILEmitVisitor 遍历 AST，通过 ILGenerator 发射 IL
    → typeBuilder.CreateTypeInfo() 得到代理 Type
```

关键文件：`ProxyTypeGenerator.cs:11`、`ProxyBuilder/ProxyTypeCompiler.cs:14`、`ProxyBuilder/Visitors/ILEmitVisitor.cs`。详见 [DynamicProxy 运行时引擎](./dynamic-proxy.md)。

## 5. 运行流程二：运行时拦截

代理方法体（`AspectActivatorBody` 生成的 IL）在被调用时：

```
代理方法被调用
  → 构造 AspectActivatorContext（服务方法/目标方法/代理方法/参数等）
  → IAspectActivatorFactory.Create() 得到 IAspectActivator
  → 按返回类型分发（ReturnKind）：
      Invoke<T> / InvokeTask<T> / InvokeValueTask<T> / InvokeAsyncEnumerable<T>
  → AspectActivator：
      IAspectContextFactory.CreateContext(...)   // 建 RuntimeAspectContext
      IAspectBuilderFactory.Create(ctx).Build()  // 组装并缓存拦截器管线
      执行管线：interceptor₁ → interceptor₂ → … → ctx.Complete()
        Complete() 通过 MethodReflector 调用真实目标方法，await 异步结果，写回 ReturnValue
      取 context.ReturnValue 作为返回；ref 返回经 StrongBox<T> 承载
  → finally: ReleaseContext
```

关键文件：`AspectActivator.cs:26`、`AspectContext.Runtime.cs:80`、`AspectBuilder.cs:8`。管线是标准的中间件式链：每个拦截器拿到 `AspectContext` 和 `next` 委托，可在调用 `next` 前后插入逻辑，或不调用 `next` 直接短路（`ctx.Break()`）。

## 6. Source Generator 引擎的位置

Source Generator 在**编译时**扫描带 `[AspectCoreGenerateProxy]` 的类型，直接生成 C# 代理源码（`ProxyEmitter`），并生成一个 `ISourceGeneratedProxyRegistry` 供运行时发现。运行时通过 `SourceGeneratedProxyTypeGenerator` 查表拿到编译期已生成的代理类型，跳过 `Reflection.Emit`。它复用运行时同一套 `IAspectActivator`/`IAspectContextFactory`/`IAspectBuilderFactory` 语义，因此拦截行为与 DynamicProxy 一致。详见 [Source Generator 编译时引擎](./source-generator.md)。

## 7. 延伸阅读

- [模块与包结构设计](./module-design.md)：逐层说明 14 个包的职责与依赖。
- [两套引擎对比与选型](./engine-comparison.md)：何时用哪套引擎，`Strict`/`AllowRuntimeFallback`/`Auto` 的行为。
- [C# 语言特性适配](./language-features.md)：各 C# 版本特性在 AOP Emit 中是否需要适配。
