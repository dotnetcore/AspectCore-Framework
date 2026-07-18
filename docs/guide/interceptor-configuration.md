# 拦截器配置

全局拦截器通过 `ConfigureDynamicProxy(Action<IAspectConfiguration>)` 注册，其中 `IAspectConfiguration.Interceptors` 提供三种注册方式：`AddTyped`、`AddServiced`、`AddDelegate`。三者都可附带 `AspectPredicate[]` 限定作用范围。本页说明它们的差异与选用场景。

## 特性拦截器 vs 全局拦截器

- **特性拦截器**：把拦截器特性标注在接口/类/方法上，作用范围就是被标注处。见[拦截器基础](./interceptor.md)。
- **全局拦截器**：在配置中注册一次，作用于所有服务，或用谓词限定到部分服务。适合日志、监控、事务等横切逻辑。

全局注册的入口是 `ConfigureDynamicProxy`：

```csharp
using AspectCore.Configuration;
using AspectCore.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

services.ConfigureDynamicProxy(config =>
{
    config.Interceptors.AddTyped<CustomInterceptorAttribute>();
});
```

## AddTyped：按类型注册

`AddTyped<TInterceptor>()` 让 AspectCore 自己创建拦截器实例。这是最常用的方式，且**只有它支持拦截器内的属性注入**（`[FromServiceContext]`）：

```csharp
services.ConfigureDynamicProxy(config =>
{
    config.Interceptors.AddTyped<CustomInterceptorAttribute>();
});
```

如果拦截器有构造器参数，用 `args` 重载传入：

```csharp
public class CustomInterceptorAttribute : AbstractInterceptorAttribute
{
    private readonly string _name;
    public CustomInterceptorAttribute(string name) => _name = name;

    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        Console.WriteLine($"Before {_name}");
        await next(context);
        Console.WriteLine($"After {_name}");
    }
}

services.ConfigureDynamicProxy(config =>
{
    config.Interceptors.AddTyped<CustomInterceptorAttribute>(args: new object[] { "custom" });
});
```

> `AddTyped` 也有 `AddTyped(Type interceptorType, ...)` 的非泛型重载，用于运行期动态指定类型。

## AddServiced：从容器解析

`AddServiced<TInterceptor>()` 不自己创建实例，而是**从 DI 容器解析**拦截器。这适合拦截器需要构造器注入的场景——先把拦截器注册为服务，再用 `AddServiced` 引用它：

```csharp
// 1. 把拦截器注册为服务（可带构造器依赖）
services.AddTransient<CustomInterceptorAttribute>(
    provider => new CustomInterceptorAttribute("custom"));

// 2. 以「已注册服务」的形式加入全局拦截器
services.ConfigureDynamicProxy(config =>
{
    config.Interceptors.AddServiced<CustomInterceptorAttribute>();
});
```

`AddTyped` 与 `AddServiced` 对依赖注入的支持不同：

| 注册方式 | 属性注入 `[FromServiceContext]` | 构造器注入 |
|----------|-------------------------------|------------|
| `AddTyped<T>()` | 生效 | 需手动传 `args` |
| `AddServiced<T>()`（配合 `services.Add...<T>()`） | 不适用 | 由容器完成 |

## AddDelegate：注册委托

`AddDelegate` 直接注册一段委托拦截逻辑，无需定义拦截器类型，适合轻量、一次性的切面：

```csharp
services.ConfigureDynamicProxy(config =>
{
    config.Interceptors.AddDelegate(async (context, next) =>
    {
        Console.WriteLine("before");
        await next(context);
        Console.WriteLine("after");
    });
});
```

`AddDelegate` 还有带 `order` 的重载，用于控制它在拦截器链中的顺序：

```csharp
config.Interceptors.AddDelegate(async (context, next) => await next(context), order: 1);
```

## 用谓词限定作用范围

三种注册方式都接受 `params AspectPredicate[]`，用来把全局拦截器限定到部分服务/方法。`Predicates` 工厂支持通配符 `*`：

```csharp
services.ConfigureDynamicProxy(config =>
{
    // 只作用于类型名以 Service 结尾的服务
    config.Interceptors.AddTyped<CustomInterceptorAttribute>(Predicates.ForService("*Service"));
});
```

也可以直接传入一个 `AspectPredicate` 委托（`MethodInfo -> bool`），做更细的判断：

```csharp
services.ConfigureDynamicProxy(config =>
{
    config.Interceptors.AddTyped<CustomInterceptorAttribute>(
        method => method.Name.EndsWith("Async"));
});
```

谓词的完整用法（`ForNameSpace` / `ForService` / `ForMethod` / `Implement`）以及如何用 `[NonAspect]` 排除，见[条件拦截](./conditional-interception.md)。

## 在其他宿主中配置

上面的例子基于 `Microsoft.Extensions.DependencyInjection`。同样的 `IAspectConfiguration.Interceptors` API 也用于：

- 内置容器 `IServiceContext`：`serviceContext.Configure(config => config.Interceptors.AddTyped<T>())`。
- 第三方容器：Autofac / Windsor / LightInject 的注册入口都接受同样的 `Action<IAspectConfiguration>`，见[第三方容器](./third-party-containers.md)。

## 下一步

- [条件拦截](./conditional-interception.md) — 谓词与 `[NonAspect]` 的完整说明。
- [依赖注入集成](./dependency-injection.md) — 拦截器中的三种注入方式细节。
- [异步拦截](./async-interception.md) — 处理异步返回值。
