# Async Interception

AspectCore's interceptor pipeline natively supports async methods. The same `Invoke` interceptor works whether the proxied method is synchronous or asynchronous, but **reading or modifying an async return value** requires a dedicated unwrapping API rather than reading `context.ReturnValue` directly. This page explains the supported return types and the correct way to handle async.

## Supported Return Types

The interceptor pipeline dispatches based on the method's return shape, covering the following categories:

- `void` and synchronous return values
- `Task` and `Task<T>`
- `ValueTask` and `ValueTask<T>`
- `IAsyncEnumerable<T>`
- `ref` / `ref readonly` returns

In other words, synchronous methods, `async` methods, and async-stream methods can all be intercepted. For the implementation details of return-type dispatch, see the "Return-type dispatch" section of [DynamicProxy Runtime Engine](../architecture/dynamic-proxy.md).

## Intercepting Async Methods

The interceptor simply does `await next(context)` as usual (or the equivalent `await context.Invoke(next)`). If the original method is asynchronous, `await` will wait for it to actually complete:

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

The corresponding async service:

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

## Reading Async Return Values: UnwrapAsyncReturnValue

For an async method, after `await next(context)`, `context.ReturnValue` holds a wrapper object such as `Task<T>` / `ValueTask<T>`, not the final result. To get the unwrapped value, use `context.UnwrapAsyncReturnValue()`:

```csharp
public class ResultLoggingInterceptorAttribute : AbstractInterceptorAttribute
{
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        await next(context);

        // unwrap Task<T> / ValueTask<T> to get the actual return value
        var result = await context.UnwrapAsyncReturnValue();
        Console.WriteLine($"Result: {result}");
    }
}
```

`UnwrapAsyncReturnValue()` returns `Task<object>`; there is also a generic overload `UnwrapAsyncReturnValue<T>()` that returns `Task<T>` directly. For a synchronous method it returns the already-ready `ReturnValue`, so in a general-purpose interceptor you can use it uniformly to read the result.

> The usage is the same in a delegate interceptor, for example:
>
> ```csharp
> config.Interceptors.AddDelegate(async (context, next) =>
> {
>     await context.Invoke(next);
>     var value = await context.UnwrapAsyncReturnValue();
>     Console.WriteLine($"unwrapped = {value}");
> }, Predicates.Implement(typeof(IAsyncService)));
> ```

## Modifying Async Return Values

To change the return value of an async method, you need to re-assign `context.ReturnValue` to the corresponding wrapper type. For example, if the original method returns `Task<string>`, assign a `Task.FromResult(...)`:

```csharp
public class ModifyResultInterceptorAttribute : AbstractInterceptorAttribute
{
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        await next(context);

        var original = await context.UnwrapAsyncReturnValue();
        // when the return type is Task<string>, re-wrap into Task<T>
        context.ReturnValue = Task.FromResult($"{original}-modified");
    }
}
```

The wrapper type you assign must match the method's declared return type (`Task<T>` corresponds to `Task.FromResult`, `ValueTask<T>` corresponds to `new ValueTask<T>(...)`), otherwise the caller will get a type mismatch when receiving the return value.

## Short-Circuiting Async Methods

You can skip the original method by not calling `next`. In this case you still need to assign `context.ReturnValue` a valid wrapper value for the caller to `await`:

```csharp
public class CacheShortCircuitInterceptorAttribute : AbstractInterceptorAttribute
{
    public override Task Invoke(AspectContext context, AspectDelegate next)
    {
        if (TryGetCached(context, out var cached))
        {
            context.ReturnValue = Task.FromResult(cached);   // return directly, do not execute the original method
            return Task.CompletedTask;
        }
        return next(context);
    }

    private bool TryGetCached(AspectContext context, out string value) { /* ... */ value = null; return false; }
}
```

## Determining Whether It Is Async

When you need to handle the two cases differently, use `context.IsAsync()` to determine whether the current method has an async return, and use `context.AwaitIfAsync()` to wait for completion when needed:

```csharp
public override async Task Invoke(AspectContext context, AspectDelegate next)
{
    await next(context);
    if (context.IsAsync())
    {
        await context.AwaitIfAsync();   // ensure the async operation has completed
    }
}
```

## Next Steps

- [Common Scenarios](./common-scenarios.md) — full examples of logging, caching, retry, and other interceptors.
- [Interceptor Basics](./interceptor.md) — the definition and application locations of interceptors.
- [DynamicProxy Runtime Engine](../architecture/dynamic-proxy.md) — the implementation principle of async return-value unwrapping.
