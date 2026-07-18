# Common Scenarios

This page gathers several common cross-cutting-logic interceptors: logging, caching, parameter validation, retry, and performance monitoring. They are all based on `AbstractInterceptorAttribute` and can be applied as attributes or registered as global interceptors (see [Interceptor Configuration](./interceptor-configuration.md)). In the examples, reading async return values uniformly uses `context.UnwrapAsyncReturnValue()`; for the reason, see [Async Interception](./async-interception.md).

## Logging

Record method entry, elapsed time, and result, and on exception log the error and rethrow:

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

> `ILogger` is obtained through the service locator (`context.ServiceProvider`). You could also switch to property injection (`[FromServiceContext]`), provided the interceptor is registered with `AddTyped<T>()`, see [Interceptor Configuration](./interceptor-configuration.md).

## Caching

Short-circuit the original method on a cache hit, and on a miss execute and backfill. Note that for async methods you must re-wrap the return value according to the declared return type:

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
            // cache hit, short-circuit the original method; async methods must re-wrap into Task<T>
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

> For the rules on short-circuiting and re-wrapping return values (`Task.FromResult` / `ValueTask`, etc.), see [Async Interception](./async-interception.md). The example above assumes the cached method returns `Task<T>`; for a synchronous method, simply assign `context.ReturnValue = cached`.

## Parameter Validation

Check parameters before method execution, throwing an exception directly when a condition is not met:

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

> When you need declarative validation based on `DataAnnotations` attributes, use `AspectCore.Extensions.DataAnnotations`, see [Data Validation](./data-validation.md).

## Retry

Automatically retry a method that may fail transiently, throwing the last exception after exceeding the limit:

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

## Performance Monitoring

Measure method elapsed time, recording it whether it succeeds or fails (placed in `finally`):

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
            // could also report to a monitoring system: Metrics.Record(methodName, stopwatch.ElapsedMilliseconds);
        }
    }
}
```

## Combining Multiple Interceptors

Multiple interceptors matching the same method form an interceptor chain, with `Order` controlling the order (smaller is more outer). You can both stack attributes and mix in global interceptors:

```csharp
public interface IOrderService
{
    [Logging]
    [Performance]
    Task<Order> GetOrderAsync(int id);
}
```

For the semantics of `Order` and `AllowMultiple`, see [Interceptor Basics](./interceptor.md).

## Next Steps

- [Async Interception](./async-interception.md) — correctly reading and modifying async return values.
- [Interceptor Configuration](./interceptor-configuration.md) — registering these interceptors as global interceptors and constraining their scope.
- [Dependency Injection Integration](./dependency-injection.md) — the several ways to obtain dependencies inside an interceptor.
