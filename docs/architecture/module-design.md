# 模块与包结构设计

本文系统说明 AspectCore 的模块划分：每个包的职责边界、公开入口、依赖方向与分层原则。这是理解代码组织的权威文档。包内实现细节见对应的架构文档；面向使用的 API 见 [使用指南](../guide/interceptor.md)。

## 1. 分层原则

AspectCore 遵循单向依赖：**契约在最底层，实现居中，集成在最上层**。

- 契约（`Abstractions`）不依赖任何实现，供上层统一编程。
- 反射库（`Extensions.Reflection`）完全独立，可单独使用，被 `Core` 依赖。
- 运行时核心（`Core`）实现 DynamicProxy 引擎 + IoC 容器。
- 集成/特性包只依赖 `Core` 或 `Abstractions`，互不横向耦合（`AspNetCore`、`DataAnnotations` 是刻意的组合例外）。
- 编译时引擎（`SourceGenerator`）独立于运行时，仅在生成代码中引用运行时类型的全限定名。

版本与语言级别统一由 `build/common.props` 管理（`LangVersion=10.0`，产品版本见 `build/version.props`）；`SourceGenerator` 自身覆盖为 `LangVersion=latest`。

## 2. 依赖关系图

```
Abstractions ◄──────────────┐
    ▲                        │
    │                        │
Extensions.Reflection ◄──┐   │
    ▲                    │   │
    │                    │   │
   Core ─────────────────┴───┘   （Core → Abstractions + Reflection）
    ▲
    ├── Extensions.DependencyInjection ── (MS.DI)
    ├── Extensions.Autofac ────────────── (Autofac)
    ├── Extensions.Windsor ────────────── (Castle.Windsor)
    ├── Extensions.LightInject ────────── (LightInject)
    ├── Extensions.Hosting ────────────── (MS.Hosting)  → 也依赖 DependencyInjection
    ├── Extensions.AspectScope
    ├── Extensions.DataValidation ─────── (仅依赖 Abstractions + Reflection)
    ├── Extensions.DataAnnotations ────── → 也依赖 DataValidation
    ├── Extensions.Configuration ──────── (仅依赖 Abstractions + Reflection)
    └── Extensions.AspNetCore ─────────── → 组合 AspectScope + DataAnnotations + DependencyInjection

SourceGenerator（独立 Roslyn 分析器，无项目引用）
```

依赖来源：各包 `.csproj` 的 `ProjectReference`。

## 3. 基础层

### AspectCore.Abstractions —— 契约

只包含接口、抽象类、特性、枚举、委托，无业务实现。分三个命名空间：

- **`AspectCore.DynamicProxy`（拦截）**：
  - `AspectContext`（每次调用的上下文抽象，含 `ReturnValue`/`Parameters`/`ServiceMethod` 等及 `Invoke`/`Complete`/`Break`）— `DynamicProxy/AspectContext.cs:9`
  - `IInterceptor` / `AbstractInterceptor` / `AbstractInterceptorAttribute` / `ServiceInterceptorAttribute`（拦截器的四种形态）— `DynamicProxy/IInterceptor.cs:6`、`AbstractInterceptorAttribute.cs:11`
  - `AspectDelegate`（管线委托 `delegate Task AspectDelegate(AspectContext)`）— `DynamicProxy/AspectDelegate.cs:5`
  - 校验：`IAspectValidator` / `IAspectValidatorBuilder` / `IAspectValidationHandler` — `DynamicProxy/IAspectValidator.cs:6`
  - 激活：`IAspectActivator`（`Invoke`/`InvokeTask`/`InvokeValueTask`/`InvokeAsyncEnumerable`）/ `IAspectActivatorFactory` / `AspectActivatorContext` — `DynamicProxy/IAspectActivator.cs:7`
  - 管线：`IAspectBuilder` / `IAspectBuilderFactory` / `IInterceptorCollector` / `IInterceptorSelector` — `DynamicProxy/IAspectBuilderFactory.cs:7`
  - 代理生成：`IProxyGenerator` / `IProxyTypeGenerator` — `DynamicProxy/IProxyGenerator.cs:6`
  - 引擎选择与 SG：`ProxyEngine`（枚举）/ `ProxyEngineOptions` / `ISourceGeneratedProxyRegistry` / `AspectCoreSourceGeneratedProxyRegistryAttribute` / `AspectCoreGenerateProxyAttribute` — `DynamicProxy/ProxyEngine.cs:6`
  - 标记特性：`[NonAspect]`（opt-out）/ `[Dynamically]`（标记生成的代理类型）— `DynamicProxy/NonAspectAttribute.cs:6`
- **`AspectCore.Configuration`（配置）**：`IAspectConfiguration`（`Interceptors`/`ValidationHandlers`/`NonAspectPredicates`/`ThrowAspectException`）、`AspectPredicate`（委托）、`InterceptorCollection`/`InterceptorFactory` — `Configuration/IAspectConfiguration.cs:7`
- **`AspectCore.DependencyInjection`（DI 契约）**：`IServiceContext`、`IServiceResolver`、`ServiceDefinition`（及 `Type`/`Instance`/`Delegate` 子类型）、`Lifetime`（枚举）、`IPropertyInjector`、`FromServiceContextAttribute` 等 — `DependencyInjection/IServiceContext.cs:9`

### AspectCore.Extensions.Reflection —— 高性能反射

独立库，用 `DynamicMethod` + IL emit 编译出缓存的委托，替代慢速的 `System.Reflection` 调用。可脱离 AOP 单独使用。

- 统一入口：`ReflectorExtensions.GetReflector(...)` 扩展方法族，把 `MethodInfo`/`ConstructorInfo`/`FieldInfo`/`PropertyInfo`/`ParameterInfo`/`Type` 映射到对应反射器 — `ReflectorExtensions.cs:6`
- 反射器类：`MethodReflector`（含 `Static`/`Call`/`OpenGeneric` 变体）、`ConstructorReflector`、`FieldReflector`、`PropertyReflector`、`TypeReflector`、`ParameterReflector`、`CustomAttributeReflector`，均派生自 `MemberReflector<T>` — `MethodReflector.cs:10`、`MemberReflector.cs:7`
- 缓存：`ReflectorCacheUtils<TMember,TReflector>` 用 `ConcurrentDictionary.GetOrAdd` 保证每个反射器只编译一次 — `Internals/ReflectorUtils.cs:8`
- IL 辅助：`Emit/ILGeneratorExtensions.cs`（`EmitLoadArg`/`EmitLdRef`/`EmitStRef`/类型转换等）
- 依赖：现代 TFM 无外部依赖；仅 `netstandard2.0` 需 `System.Threading.Tasks.Extensions`、`System.Reflection.Emit.Lightweight`、`System.Runtime.CompilerServices.Unsafe`

## 4. 运行时核心层

### AspectCore.Core —— DynamicProxy 引擎 + IoC 容器

依赖 `Abstractions` + `Extensions.Reflection`。是运行时的心脏，内部分三块：

**(a) DynamicProxy 运行时引擎**（`DynamicProxy/` 与 `DynamicProxy/ProxyBuilder/`）
- 代理类型生成：`ProxyTypeGenerator`（`ProxyTypeGenerator.cs:11`）→ `ProxyTypeCompiler`（单一 `ModuleBuilder`，按名缓存）→ AST 构建器（`ClassProxyBuilder`/`InterfaceProxyBuilder`/`InterfaceImplBuilder`）→ `ILEmitVisitor` 发射 IL
- 运行时拦截：`AspectActivator`（`AspectActivator.cs:26`）、`RuntimeAspectContext`（`AspectContext.Runtime.cs:13`）、`AspectBuilder`/`AspectBuilderFactory`（管线组装与缓存）、`InterceptorCollector`（收集/排序/去重）
- 返回类型分发：`ReturnKind` 枚举（`Void`/`Sync`/`Task`/`TaskOfT`/`ValueTask`/`ValueTaskOfT`/`AsyncEnumerable`/`RefSync`）— `ProxyBuilder/ReturnKind.cs:3`

**(b) IoC 容器**（`DependencyInjection/`）
- `ServiceContext`（注册清单，`AddInternalServices` 自注册整个 DynamicProxy 服务图）— `DependencyInjection/ServiceContext.cs:11`
- `ServiceResolver`（按 `Lifetime` 解析：Singleton/Scoped 缓存、Transient 直建）— `DependencyInjection/ServiceResolver.cs:10`
- `ServiceTable`（注册索引；**在此决定代理引擎**并对需拦截的服务包成 `ProxyServiceDefinition`）— `DependencyInjection/ServiceTable.cs:11`
- `ServiceCallSiteResolver` / `ConstructorCallSiteResolver`（编译解析委托、构造器选择）、`ServiceValidator`（是否代理的门槛）

**(c) 配置与校验**（`Configuration/` 与 `DynamicProxy/ValidationHandlers/`）
- `AspectConfiguration`（默认配置）、`Predicates`（`ForNameSpace`/`ForService`/`ForMethod`/`Implement` 谓词工厂）— `Configuration/Predicates.cs:5`
- 校验责任链：`CacheAspectValidationHandler`(Order -101) → `OverwriteAspectValidationHandler`(Order 1) → `ConfigureAspectValidationHandler`(Order 11) → `AttributeAspectValidationHandler`(Order 13)
- 拦截器注册扩展：`AddTyped`/`AddServiced`/`AddDelegate` — `Configuration/Extensions/InterceptorCollectionExtensions.cs:9`

### AspectCore.SourceGenerator —— 编译时引擎

独立 Roslyn 增量分析器（`netstandard2.0`，`IsRoslynComponent`），无项目引用。编译时扫描 `[AspectCoreGenerateProxy]`，生成 C# 代理源码 + `ISourceGeneratedProxyRegistry`。

- `AspectCoreProxyGenerator`（`IIncrementalGenerator`，候选发现与校验，发 `ACSGxxx` 诊断）— `AspectCoreProxyGenerator.cs:10`
- `Emit/ProxyEmitter.cs`（生成代理源码，内联激活避免 `AspectActivator` 分配，支持 `ref` 返回/`ref` 属性/接口 stub）
- `Emit/RegistryEmitter.cs`（生成 registry + assembly 特性供运行时发现）
- `RecordTypeUtils`/`Naming`/`TypeNameExtensions`（record 识别、命名、全限定名）
- 依赖：`Microsoft.CodeAnalysis.CSharp`、`Microsoft.CodeAnalysis.Analyzers`

## 5. 集成层（DI 适配）

这些包把 AspectCore 的代理织入不同容器；均依赖 `Core`。共同点：都提供 `ConfigureDynamicProxyEngine(Action<ProxyEngineOptions>)` 以切换到 Source Generator 引擎。

| 包 | 目标容器 | 入口 API | 关键依赖 |
|----|---------|---------|---------|
| `Extensions.DependencyInjection` | Microsoft.Extensions.DependencyInjection | `IServiceCollection.ConfigureDynamicProxy(...)`、`BuildServiceContextProvider()`、`DynamicProxyServiceProviderFactory` | `Microsoft.Extensions.DependencyInjection` |
| `Extensions.Autofac` | Autofac | `ContainerBuilder.RegisterDynamicProxy(...)`（`PipelineBuilding` 中间件织入） | `Autofac [7.0,8.0)` |
| `Extensions.Windsor` | Castle Windsor | `IWindsorContainer.AddAspectCoreFacility(...)`（Facility 形式） | `Castle.Windsor 6.0.0` |
| `Extensions.LightInject` | LightInject | `IServiceContainer.RegisterDynamicProxy(...)`（`Decorate` 形式） | `LightInject 6.6.4` |
| `Extensions.Hosting` | 泛型主机 | `IHostBuilder.UseServiceContext()` / `UseDynamicProxy()` / `ConfigureDynamicProxy()` | `Microsoft.Extensions.Hosting`（并依赖 `DependencyInjection`） |

入口位置：`ServiceCollectionExtensions.cs:20`、`Autofac/ContainerBuilderExtensions.cs:16`、`Windsor/FacilityExtensions.cs:11`、`LightInject/ContainerBuilderExtensions.cs:32`、`Hosting/HostBuilderExtensions.cs:12`。

## 6. 特性/扩展层

| 包 | 职责 | 入口 API | 依赖 |
|----|------|---------|------|
| `Extensions.AspectScope` | 作用域内的切面上下文与调度（scoped aspect） | `IServiceContext.AddAspectScope()` | `Core` |
| `Extensions.DataValidation` | 数据校验框架/抽象 + 校验拦截器 `DataValidationInterceptorAttribute`(Order -999) | 由 DataAnnotations 装配 | `Abstractions` + `Reflection` + `System.ComponentModel.Annotations` |
| `Extensions.DataAnnotations` | 基于 `System.ComponentModel.DataAnnotations` 的具体校验实现 | `IServiceContext.AddDataAnnotations(...)` | `Core` + `DataValidation` |
| `Extensions.Configuration` | 从 `IConfiguration` 绑定值到解析出的服务字段 | `IServiceContext.AddConfigurationInject()` + `[ConfigurationValue]`/`[ConfigurationBinding]` | `Abstractions` + `Reflection` + `Microsoft.Extensions.Configuration.*` |
| `Extensions.AspNetCore` | ASP.NET Core 集成：作用域切面、DataAnnotations 校验、ModelState 适配 | `IServiceCollection.AddAspectScope()` / `AddDataAnnotations(...)` | 组合 `AspectScope` + `DataAnnotations` + `DependencyInjection`（仅 net6.0+） |

入口位置：`AspectScope/ServiceContainerExtensions.cs:9`、`DataAnnotations/ServiceContainerExtensions.cs:10`、`Configuration/ServiceContainerExtensions.cs:8`、`AspNetCore/Extensions/ServiceCollectionExtensions.cs:17`。

## 7. 测试、示例、基准（非发行）

- `tests/`：`AspectCore.Core.Tests`（含 `EngineParity/` 双引擎一致性测试）、`AspectCore.E2E.Tests`、各容器适配测试、`AspectCore.Extensions.Reflection.Test` 等。详见 [测试策略](../testing/testing-strategy.md)。
- `sample/`：AspectScope、Autofac、DataAnnotations、DependencyInjection 控制台示例。
- `benchmark/`、`benchmarks/`：Core 与 Reflection 的基准项目。

## 8. 边界与约束

- **契约不下沉实现**：新增拦截/DI 抽象放 `Abstractions`，实现放 `Core`。
- **反射库保持独立**：`Extensions.Reflection` 不得反向依赖 `Core`/`Abstractions`。
- **特性包不横向耦合**：新特性包应只依赖 `Core`/`Abstractions`；需要组合时（如 `AspNetCore`）显式声明。
- **两引擎语义对齐**：任何影响拦截语义的改动必须同时覆盖 DynamicProxy 与 SourceGenerator，并由 `EngineParity` 测试守护。
