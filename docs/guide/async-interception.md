# 异步拦截

AspectCore 的拦截器管线原生支持异步方法。同一个 `Invoke` 拦截器无论被代理的方法是同步还是异步都能工作，但**读取或修改异步返回值**时需要用专门的解包 API，而不是直接读 `context.ReturnValue`。本页说明支持的返回类型以及正确的异步处理写法。

## 支持的返回类型

拦截器管线按方法的返回形态分发，覆盖以下几类：

- `void` 与同步返回值
- `Task` 与 `Task<T>`
- `ValueTask` 与 `ValueTask<T>`
- `IAsyncEnumerable<T>`
- `ref` / `ref readonly` 返回

也就是说，同步方法、`async` 方法、异步流方法都可以被拦截。返回类型分发的实现细节见 [DynamicProxy 运行时引擎](../architecture/dynamic-proxy.md) 的「返回类型分发」小节。

## 拦截异步方法

拦截器只需照常 `await next(context)`（或等价的 `await context.Invoke(next)`）。原方法是异步的，`await` 就会等它真正完成：

```csharp
using System;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;

public class AsyncInterceptorAttribute : AbstractInterceptorAttribute
{
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        Console.WriteLine("Before async call");
        await next(context);
        Console.WriteLine("After async call");
    }
}
```

对应的异步服务：

```csharp
public interface IAsyncService
{
    [AsyncInterceptor]
    Task<string> GetDataAsync();
}

public class AsyncService : IAsyncService
{
    public async Task<string> GetDataAsync()
    {
        await Task.Delay(100);
        return "Hello Async";
    }
}
```

## 读取异步返回值：UnwrapAsyncReturnValue

对异步方法，`await next(context)` 之后 `context.ReturnValue` 里放的是 `Task<T>` / `ValueTask<T>` 这类包装对象，而不是最终结果。要拿到解包后的值，用 `context.UnwrapAsyncReturnValue()`：

```csharp
public class ResultLoggingInterceptorAttribute : AbstractInterceptorAttribute
{
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        await next(context);

        // 解包 Task<T> / ValueTask<T>，得到真正的返回值
        var result = await context.UnwrapAsyncReturnValue();
        Console.WriteLine($"Result: {result}");
    }
}
```

`UnwrapAsyncReturnValue()` 返回 `Task<object>`；也有泛型重载 `UnwrapAsyncReturnValue<T>()` 直接返回 `Task<T>`。对同步方法它会返回已经就绪的 `ReturnValue`，所以在通用拦截器里可以统一用它读取结果。

> 委托拦截器里用法相同，例如：
>
> ```csharp
> config.Interceptors.AddDelegate(async (context, next) =>
> {
>     await context.Invoke(next);
>     var value = await context.UnwrapAsyncReturnValue();
>     Console.WriteLine($"unwrapped = {value}");
> }, Predicates.Implement(typeof(IAsyncService)));
> ```

## 修改异步返回值

要改异步方法的返回值，需要把 `context.ReturnValue` 重新赋成对应的包装类型。例如原方法返回 `Task<string>`，就赋一个 `Task.FromResult(...)`：

```csharp
public class ModifyResultInterceptorAttribute : AbstractInterceptorAttribute
{
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        await next(context);

        var original = await context.UnwrapAsyncReturnValue();
        // 返回类型为 Task<string> 时，重新包装成 Task<T>
        context.ReturnValue = Task.FromResult($"{original}-modified");
    }
}
```

赋值时的包装类型要与方法声明的返回类型匹配（`Task<T>` 对应 `Task.FromResult`，`ValueTask<T>` 对应 `new ValueTask<T>(...)`），否则调用方拿到返回值时会类型不符。

## 短路异步方法

不调用 `next` 就能跳过原方法。此时同样要给 `context.ReturnValue` 赋一个合法的包装值，供调用方 `await`：

```csharp
public class CacheShortCircuitInterceptorAttribute : AbstractInterceptorAttribute
{
    public override Task Invoke(AspectContext context, AspectDelegate next)
    {
        if (TryGetCached(context, out var cached))
        {
            context.ReturnValue = Task.FromResult(cached);   // 直接返回，不执行原方法
            return Task.CompletedTask;
        }
        return next(context);
    }

    private bool TryGetCached(AspectContext context, out string value) { /* ... */ value = null; return false; }
}
```

## 判断是否异步

需要区分处理时，可用 `context.IsAsync()` 判断当前方法是否为异步返回，用 `context.AwaitIfAsync()` 在需要时等待完成：

```csharp
public override async Task Invoke(AspectContext context, AspectDelegate next)
{
    await next(context);
    if (context.IsAsync())
    {
        await context.AwaitIfAsync();   // 确保异步操作已完成
    }
}
```

## 下一步

- [常见场景](./common-scenarios.md) — 日志、缓存、重试等拦截器的完整示例。
- [拦截器基础](./interceptor.md) — 拦截器的定义与应用位置。
- [DynamicProxy 运行时引擎](../architecture/dynamic-proxy.md) — 异步返回值解包的实现原理。
