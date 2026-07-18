# 核心概念

本页解释 AspectCore 中反复出现的术语。理解它们有助于阅读后续的使用指南和架构文档。

## 切面（Aspect）

面向切面编程（AOP）中的「切面」，指从多个业务对象中抽取出来的横切关注点，例如日志、缓存、事务、校验。AspectCore 让你把这些逻辑写在拦截器里，再声明式地织入目标方法，而不必在每个业务方法里重复调用。

## 拦截器（Interceptor）

拦截器是承载切面逻辑的单元，实现 `IInterceptor` 接口，核心是一个 `Invoke` 方法：

```csharp
Task Invoke(AspectContext context, AspectDelegate next);
```

在 `Invoke` 中，`next(context)` 之前是「进入方法前」的逻辑，之后是「方法返回后」的逻辑；不调用 `next` 就等于短路（跳过原方法）。AspectCore 提供几种定义拦截器的形式：

- `AbstractInterceptorAttribute`：本身是特性，可直接标注到接口/类/方法上。
- `AbstractInterceptor`：普通基类，通常用于全局注册（不作为特性）。
- `ServiceInterceptorAttribute`：从容器解析真正的拦截器实例，适合需要构造器注入的拦截器。
- 委托拦截器：用 `AddDelegate((ctx, next) => ...)` 直接注册一段委托，无需定义类型。

各形式的用法见[拦截器基础](../guide/interceptor.md)与[拦截器配置](../guide/interceptor-configuration.md)。

## 代理（Proxy）

AspectCore 不会修改你的原始类，而是生成一个**代理类型**包住目标对象。调用代理的方法时，先经过拦截器管线，再转发给真实实现。你从容器解析出来的实例，其实就是这个代理。

代理只能织入可重写成员：接口成员，或非 `sealed` 类的 `virtual` 成员。生成代理有两套引擎：

- **DynamicProxy（运行时）**：默认引擎，运行时用 `Reflection.Emit` 生成代理，无需改动构建流程。见 [DynamicProxy 运行时引擎](../architecture/dynamic-proxy.md)。
- **SourceGenerator（编译期）**：显式 opt-in，编译时由 Roslyn 生成代理类型，适合 AOT / 裁剪场景。见 [Source Generator 编译时引擎](../architecture/source-generator.md)。

两者的取舍见[两套引擎对比与选型](../architecture/engine-comparison.md)。

## 切面上下文（AspectContext）

`AspectContext` 是拦截器 `Invoke` 收到的上下文对象，贯穿一次方法调用。常用成员：

| 成员 | 含义 |
|------|------|
| `ServiceMethod` | 服务（接口/声明）上的方法信息。 |
| `ImplementationMethod` | 实现类型上的方法信息。 |
| `ProxyMethod` | 代理类型上的方法信息。 |
| `Parameters` | 方法参数数组，可读可改。 |
| `ReturnValue` | 方法返回值，可读可改（`next` 之后才有值）。 |
| `Implementation` | 被代理的实现实例。 |
| `Proxy` | 代理实例。 |
| `ServiceProvider` | 当前作用域的服务提供者，可用作服务定位器。 |
| `AdditionalData` | 跨拦截器传递数据的字典。 |
| `Invoke(next)` | 继续执行管线的下一环。 |
| `Break()` | 短路管线。 |

> 注意：`AspectContext` 没有 `ServiceDescriptor` 成员；要拿服务类型请用 `context.ServiceMethod.DeclaringType`。异步方法的返回值需要用 `context.UnwrapAsyncReturnValue()` 解包，见[异步拦截](../guide/async-interception.md)。

## 切入点谓词（AspectPredicate）

谓词决定「哪些方法被拦截」。它本质是一个 `delegate bool AspectPredicate(MethodInfo method)`。`Predicates` 工厂提供常用构造方式，且支持通配符 `*`：

- `Predicates.ForNameSpace("App1")` — 按命名空间匹配。
- `Predicates.ForService("*Service")` — 按服务类型名匹配。
- `Predicates.ForMethod("Query*")` — 按方法名匹配。
- `Predicates.Implement(typeof(IFoo))` — 匹配实现了某接口/基类的类型。

全局注册拦截器时可传入谓词限定作用范围，见[条件拦截](../guide/conditional-interception.md)。

## 两套引擎

AspectCore 有两套生成代理的引擎，共享同一套拦截器配置 API（`ConfigureDynamicProxy(...)`、`IAspectConfiguration`、`Predicates`）：

- 不做任何新配置时，使用运行时 **DynamicProxy**（默认，与旧版本行为一致）。
- 显式启用 **SourceGenerator** 后，可在编译期生成代理，运行时查表直接拿到代理类型。

引擎选择通过 `ProxyEngineOptions.Engine`（`DynamicProxy` / `SourceGenerator` / `Auto`）完成，详见[两套引擎对比与选型](../architecture/engine-comparison.md)。

## `[NonAspect]`

`[NonAspect]` 用来显式排除某个接口、类或方法，使其**不被代理**。它是精确到类型/方法的开关：

```csharp
[NonAspect]
public interface ICustomService
{
    void Call();
}
```

除了特性，还可以在全局配置中通过 `NonAspectPredicates` 批量排除（支持通配符），见[条件拦截](../guide/conditional-interception.md)。

## 延伸阅读

- [总体架构](../architecture/overview.md) — 分层与运行流程。
- [模块与包结构设计](../architecture/module-design.md) — 各包职责边界。
- [拦截器基础](../guide/interceptor.md) — 开始定义你自己的拦截器。
