# 拦截器基础

拦截器是 AspectCore 承载切面逻辑的核心。本页说明如何定义拦截器、把它应用到接口/类/方法上，以及 `Order`、`AllowMultiple`、`Inherited` 三个行为开关的作用。

## 定义拦截器

拦截器实现 `IInterceptor` 接口，核心是一个 `Invoke` 方法。AspectCore 提供两个基类：

- `AbstractInterceptorAttribute`：本身是 `Attribute`，可直接标注到接口、类或方法上，也可用于全局注册。
- `AbstractInterceptor`：普通基类，不是特性，通常只用于全局注册。

`Invoke` 的签名固定为 `Task Invoke(AspectContext context, AspectDelegate next)`。在其中调用 `next(context)` 把控制权交给管线的下一环，前后即是切面逻辑：

```csharp
using System;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;

public class CustomInterceptorAttribute : AbstractInterceptorAttribute
{
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        try
        {
            Console.WriteLine("Before service call");
            await next(context);
        }
        catch (Exception)
        {
            Console.WriteLine("Service threw an exception!");
            throw;
        }
        finally
        {
            Console.WriteLine("After service call");
        }
    }
}
```

> `AbstractInterceptorAttribute` 上带有 `[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Interface, Inherited = false)]`，即它只能标注到方法、类或接口上，且默认不随继承传递。

## 特性拦截器 vs 全局拦截器

同一个拦截器有两种生效方式：

- **特性拦截器**：把拦截器特性标注到目标上，作用范围就是被标注的成员。适合按需、精确地织入。
- **全局拦截器**：通过 `ConfigureDynamicProxy(config => config.Interceptors.Add...)` 注册，作用于所有（或谓词匹配的）服务。适合日志、监控这类横切所有服务的场景。

本页聚焦特性拦截器的应用位置；全局注册的三种方式（`AddTyped` / `AddServiced` / `AddDelegate`）和作用范围谓词见[拦截器配置](./interceptor-configuration.md)。

## 应用到方法、类、接口

`AbstractInterceptorAttribute` 可以标注在三个层级，作用范围逐级放大：

**标注到方法** — 只拦截该方法：

```csharp
public interface ICustomService
{
    [CustomInterceptor]
    void Call();

    void Untouched();   // 不被拦截
}
```

**标注到接口或类** — 拦截其下所有可代理成员：

```csharp
[CustomInterceptor]
public interface ICustomService
{
    void Call();
    string Query();     // 两个方法都被拦截
}
```

对**类**而言只有 `virtual` 成员会被织入（接口成员始终可代理）。哪些成员可被代理见 [DynamicProxy 运行时引擎](../architecture/dynamic-proxy.md)。

## 读取方法信息

在拦截器中通过 `AspectContext` 获取方法元数据。注意用 `ServiceMethod` / `ImplementationMethod` / `ProxyMethod`，`AspectContext` 没有 `ServiceDescriptor` 成员：

```csharp
public class MethodInfoInterceptorAttribute : AbstractInterceptorAttribute
{
    public override Task Invoke(AspectContext context, AspectDelegate next)
    {
        // 服务（声明）类型与方法
        var serviceType = context.ServiceMethod.DeclaringType;
        Console.WriteLine($"Service: {serviceType.Name}.{context.ServiceMethod.Name}");

        // 实现类型上的方法
        Console.WriteLine($"Implementation: {context.ImplementationMethod.DeclaringType?.Name}");

        // 参数
        Console.WriteLine($"Parameters: {string.Join(", ", context.Parameters)}");

        return next(context);
    }
}
```

读取和修改参数、返回值的更多细节见[异步拦截](./async-interception.md)与[常见场景](./common-scenarios.md)。

## `Order`：控制执行顺序

多个拦截器命中同一方法时会组成拦截器链。`Order` 决定它们的排序，值越小越靠外层（越先进入、越后退出）。`AbstractInterceptorAttribute.Order` 默认为 `0`，可读写：

```csharp
public class LogInterceptorAttribute : AbstractInterceptorAttribute
{
    public LogInterceptorAttribute() => Order = 1;   // 外层

    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        Console.WriteLine("[Log] enter");
        await next(context);
        Console.WriteLine("[Log] exit");
    }
}
```

## `AllowMultiple`：是否允许重复应用

`AllowMultiple` 表示同一个拦截器类型能否在一个目标上生效多次。`AbstractInterceptorAttribute` 默认是 `false`（不允许重复）。若确需在多个层级叠加同类拦截器，重写该属性返回 `true`：

```csharp
public class TagInterceptorAttribute : AbstractInterceptorAttribute
{
    public override bool AllowMultiple => true;

    public override Task Invoke(AspectContext context, AspectDelegate next)
        => next(context);
}
```

> `ServiceInterceptorAttribute` 的 `AllowMultiple` 就是 `true`。

## `Inherited`：是否随继承传递

`Inherited` 决定标注在接口/基类成员上的拦截器，是否对派生实现继续生效。默认为 `false`。设为 `true` 后，实现类沿用基类型上声明的拦截器：

```csharp
public class AuditInterceptorAttribute : AbstractInterceptorAttribute
{
    public AuditInterceptorAttribute() => Inherited = true;

    public override Task Invoke(AspectContext context, AspectDelegate next)
        => next(context);
}
```

## 拦截器中的依赖注入

拦截器可以拿到需要的服务，方式取决于它是如何注册的：

- **属性注入**：拦截器中带 `public get/set` 的属性标注 `[FromServiceContext]` 即可自动注入。仅在用 `AddTyped<T>()` 注册时生效。
- **构造器注入**：需要拦截器作为服务被激活，用 `AddServiced<T>()` 或 `ServiceInterceptorAttribute`。
- **服务定位器**：任何时候都可通过 `context.ServiceProvider.GetService<T>()` 获取服务。

三者的适用条件与注册方式见[拦截器配置](./interceptor-configuration.md)与[依赖注入集成](./dependency-injection.md)。

## 下一步

- [拦截器配置](./interceptor-configuration.md) — 全局注册的三种方式与作用范围。
- [异步拦截](./async-interception.md) — 处理 `Task` / `ValueTask` / `IAsyncEnumerable`。
- [条件拦截](./conditional-interception.md) — 用谓词和 `[NonAspect]` 精确控制织入范围。
