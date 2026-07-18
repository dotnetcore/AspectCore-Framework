# 条件拦截

条件拦截解决「哪些方法该被织入、哪些不该」的问题。AspectCore 提供两条互补的手段：用 `Predicates` 谓词**限定**全局拦截器的作用范围（正向匹配），用 `[NonAspect]` 或 `NonAspectPredicates`**排除**不希望被代理的类型/方法（反向排除）。

## 切入点谓词 AspectPredicate

`AspectPredicate` 本质是 `delegate bool AspectPredicate(MethodInfo method)`：对每个候选方法返回 `true` 表示命中。`Predicates` 工厂提供四种常用构造方式，字符串匹配都支持通配符 `*`。

### Predicates.ForNameSpace

按方法声明类型的命名空间匹配：

```csharp
using AspectCore.Configuration;

// App1 命名空间
config.Interceptors.AddTyped<LogInterceptorAttribute>(Predicates.ForNameSpace("App1"));

// 通配符：任意以 .App1 结尾的命名空间
config.Interceptors.AddTyped<LogInterceptorAttribute>(Predicates.ForNameSpace("*.App1"));
```

### Predicates.ForService

按服务类型名匹配（对泛型类型会去掉 <code>\`n</code> 部分，也会尝试匹配全名）：

```csharp
// 类型名以 Service 结尾的服务
config.Interceptors.AddTyped<LogInterceptorAttribute>(Predicates.ForService("*Service"));
```

### Predicates.ForMethod

按方法名匹配，另有 `ForMethod(service, method)` 同时限定服务与方法：

```csharp
// 所有名为 Query 或以 Query 结尾的方法
config.Interceptors.AddTyped<LogInterceptorAttribute>(Predicates.ForMethod("*Query"));

// 限定：*Service 服务上的 Get* 方法
config.Interceptors.AddTyped<LogInterceptorAttribute>(Predicates.ForMethod("*Service", "Get*"));
```

### Predicates.Implement

匹配实现了指定接口或继承自指定基类的类型：

```csharp
// 所有实现 IRepository 的类型
config.Interceptors.AddTyped<LogInterceptorAttribute>(Predicates.Implement(typeof(IRepository)));
```

> `Implement` 要求传入的类型是 class 或 interface，且不能是 `sealed`，否则会抛 `ArgumentException`。

### 自定义谓词

需要更复杂的判断时，直接传一个 `MethodInfo -> bool` 委托：

```csharp
config.Interceptors.AddTyped<LogInterceptorAttribute>(
    method => method.Name.StartsWith("Get") && method.ReturnType != typeof(void));
```

### 组合多个谓词

`AddTyped` / `AddServiced` / `AddDelegate` 都接受 `params AspectPredicate[]`。传入多个谓词时，只要**任一**命中即生效：

```csharp
config.Interceptors.AddTyped<LogInterceptorAttribute>(
    Predicates.ForService("*Service"),
    Predicates.ForService("*Repository"));   // Service 或 Repository 都命中
```

## 用 [NonAspect] 排除

`[NonAspect]` 显式地把某个接口、类或方法标记为**不代理**，优先级高于全局拦截器的匹配：

```csharp
using AspectCore.DynamicProxy;

[NonAspect]
public interface ICustomService
{
    void Call();
}
```

也可只排除单个方法：

```csharp
public interface ICustomService
{
    void Call();

    [NonAspect]
    void Diagnostics();   // 该方法不会被代理
}
```

## 用 NonAspectPredicates 全局排除

除了逐个标注，还能在配置里通过 `NonAspectPredicates` 批量排除，同样支持通配符。这适合排除整个命名空间或一批命名约定：

```csharp
services.ConfigureDynamicProxy(config =>
{
    // App1 命名空间下的服务不被代理
    config.NonAspectPredicates.AddNamespace("App1");

    // 最后一级为 App1 的命名空间下的服务不被代理
    config.NonAspectPredicates.AddNamespace("*.App1");

    // ICustomService 接口不被代理
    config.NonAspectPredicates.AddService("ICustomService");

    // 后缀为 Service 的接口和类不被代理
    config.NonAspectPredicates.AddService("*Service");

    // 名为 Query 的方法不被代理
    config.NonAspectPredicates.AddMethod("Query");

    // 后缀为 Query 的方法不被代理
    config.NonAspectPredicates.AddMethod("*Query");
});
```

`NonAspectPredicates` 提供的方法与 `Predicates` 对应：`AddNamespace`、`AddService`、`AddMethod`（含 `AddMethod(service, method)` 重载）。

## 正向匹配 vs 反向排除

| 手段 | 作用 | 用在哪里 |
|------|------|----------|
| `Predicates.*` | 限定全局拦截器**命中**哪些方法 | 注册拦截器时作为 `AspectPredicate[]` 传入 |
| `[NonAspect]` | 精确排除某个类型/方法**不被代理** | 标注在接口/类/方法上 |
| `NonAspectPredicates.*` | 按命名空间/服务/方法批量**排除代理** | `IAspectConfiguration` 配置中 |

两类手段可以叠加：先用谓词圈定拦截范围，再用 `[NonAspect]` / `NonAspectPredicates` 挖掉个别例外。

## 下一步

- [拦截器配置](./interceptor-configuration.md) — 谓词如何配合三种注册方式。
- [核心概念](../getting-started/concepts.md) — 谓词与 `[NonAspect]` 的概念背景。
- [总体架构](../architecture/overview.md) — 「是否拦截」在何时被决定。
