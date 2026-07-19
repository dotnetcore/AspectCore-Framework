# 常见场景

本页汇集几个常见的横切逻辑拦截器：日志、缓存、参数校验、重试、性能监控。它们都基于 `AbstractInterceptorAttribute`，可以作为特性标注，也可以作为全局拦截器注册（见[拦截器配置](./interceptor-configuration.md)）。示例中读取异步返回值统一用 `context.UnwrapAsyncReturnValue()`，原因见[异步拦截](./async-interception.md)。

## 日志记录

记录方法进入、耗时与结果，异常时记录错误并重新抛出：

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class LoggingInterceptorAttribute : AbstractInterceptorAttribute
{
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        var logger = context.ServiceProvider.GetService<ILogger<LoggingInterceptorAttribute>>();
        var methodName = $"{context.ServiceMethod.DeclaringType.Name}.{context.ServiceMethod.Name}";
        var parameters = string.Join(", ", context.Parameters.Select(p => p?.ToString() ?? "null"));

        logger.LogInformation("[START] {Method}({Params})", methodName, parameters);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            await next(context);
            stopwatch.Stop();
            var result = await context.UnwrapAsyncReturnValue();
            logger.LogInformation("[END] {Method} in {Elapsed}ms. Result: {Result}",
                methodName, stopwatch.ElapsedMilliseconds, result);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "[ERROR] {Method} failed after {Elapsed}ms",
                methodName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
```

> `ILogger` 通过服务定位器（`context.ServiceProvider`）获取。也可以改用属性注入（`[FromServiceContext]`），前提是拦截器用 `AddTyped<T>()` 注册，见[拦截器配置](./interceptor-configuration.md)。

## 缓存

命中缓存时短路原方法，未命中则执行并回填。注意异步方法要按声明的返回类型重新包装返回值：

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Microsoft.Extensions.Caching.Memory;

public class CacheInterceptorAttribute : AbstractInterceptorAttribute
{
    private readonly int _durationSeconds;

    public CacheInterceptorAttribute(int durationSeconds = 60) => _durationSeconds = durationSeconds;

    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        var cache = context.ServiceProvider.GetService(typeof(IMemoryCache)) as IMemoryCache;
        var cacheKey = GetCacheKey(context);

        if (cache.TryGetValue(cacheKey, out var cached))
        {
            // 命中缓存，短路原方法；异步方法需重新包装成 Task<T>
            context.ReturnValue = Task.FromResult(cached);
            return;
        }

        await next(context);
        var result = await context.UnwrapAsyncReturnValue();
        cache.Set(cacheKey, result, TimeSpan.FromSeconds(_durationSeconds));
    }

    private static string GetCacheKey(AspectContext context)
        => $"{context.ServiceMethod.DeclaringType.Name}.{context.ServiceMethod.Name}_{string.Join("_", context.Parameters)}";
}
```

> 短路和重新包装返回值的规则（`Task.FromResult` / `ValueTask` 等）见[异步拦截](./async-interception.md)。上例假设被缓存的方法返回 `Task<T>`；同步方法直接赋 `context.ReturnValue = cached` 即可。

## 参数校验

在方法执行前检查参数，不满足条件直接抛异常：

```csharp
using System;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;

public class ValidationInterceptorAttribute : AbstractInterceptorAttribute
{
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        foreach (var param in context.Parameters)
        {
            if (param == null)
            {
                throw new ArgumentException("Parameters cannot be null");
            }
        }
        await next(context);
    }
}
```

> 需要基于 `DataAnnotations` 特性的声明式校验时，用 `AspectCore.Extensions.DataAnnotations`，见[数据校验](./data-validation.md)。

## 重试

对可能瞬时失败的方法自动重试，超过上限后抛出最后一次异常：

```csharp
using System;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;

public class RetryInterceptorAttribute : AbstractInterceptorAttribute
{
    private readonly int _maxRetries;
    private readonly int _delayMs;

    public RetryInterceptorAttribute(int maxRetries = 3, int delayMs = 1000)
    {
        _maxRetries = maxRetries;
        _delayMs = delayMs;
    }

    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        var retryCount = 0;
        while (true)
        {
            try
            {
                await next(context);
                return;
            }
            catch (Exception ex) when (retryCount < _maxRetries)
            {
                retryCount++;
                Console.WriteLine($"Retry {retryCount}/{_maxRetries} after: {ex.Message}");
                await Task.Delay(_delayMs);
            }
        }
    }
}
```

## 性能监控

统计方法耗时，无论成功失败都记录（放在 `finally`）：

```csharp
using System;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;

public class PerformanceInterceptorAttribute : AbstractInterceptorAttribute
{
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            var methodName = $"{context.ServiceMethod.DeclaringType.Name}.{context.ServiceMethod.Name}";
            Console.WriteLine($"[PERF] {methodName} took {stopwatch.ElapsedMilliseconds}ms");
            // 也可上报到监控系统：Metrics.Record(methodName, stopwatch.ElapsedMilliseconds);
        }
    }
}
```

## 组合多个拦截器

多个拦截器命中同一方法会组成拦截器链，用 `Order` 控制顺序（越小越靠外）。既可以叠加特性，也可以混用全局拦截器：

```csharp
public interface IOrderService
{
    [Logging]
    [Performance]
    Task<Order> GetOrderAsync(int id);
}
```

`Order`、`AllowMultiple` 的语义见[拦截器基础](./interceptor.md)。

## 下一步

- [异步拦截](./async-interception.md) — 正确读取和修改异步返回值。
- [拦截器配置](./interceptor-configuration.md) — 把这些拦截器注册为全局拦截器并限定范围。
- [依赖注入集成](./dependency-injection.md) — 拦截器中获取依赖的几种方式。
