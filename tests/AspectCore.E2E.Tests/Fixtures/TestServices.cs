using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;

namespace AspectCore.E2E.Tests.Fixtures;

// ============================================================================
// Basic service with a mix of method types and an interceptor attribute.
// ============================================================================

public interface ICalculatorService
{
    int Add(int a, int b);
    int Subtract(int a, int b);
    Task<int> MultiplyAsync(int a, int b);
    ValueTask<int> DivideAsync(int a, int b);
    string Concat(string left, string right = "!");
    int Sum(int[] values);
    T Echo<T>(T value);
    void GetOutput(int input, out int doubled);
    void Increment(ref int value);
    string? Greet(string? name);
}

public class CalculatorService : ICalculatorService
{
    public int Add(int a, int b) => a + b;

    public int Subtract(int a, int b) => a - b;

    public async Task<int> MultiplyAsync(int a, int b)
    {
        await Task.Delay(1);
        return a * b;
    }

    public async ValueTask<int> DivideAsync(int a, int b)
    {
        await Task.Delay(1);
        return a / b;
    }

    public string Concat(string left, string right = "!") => left + right;

    public int Sum(int[] values)
    {
        int total = 0;
        foreach (var v in values)
        {
            total += v;
        }
        return total;
    }

    public T Echo<T>(T value) => value;

    public void GetOutput(int input, out int doubled) => doubled = input * 2;

    public void Increment(ref int value) => value += 1;

    public string? Greet(string? name) => name == null ? "Hello, stranger" : $"Hello, {name}";
}

// ============================================================================
// Service where one method is intercepted and another is not — used to verify
// that the proxy preserves original behavior on non-intercepted methods.
// ============================================================================

public interface IOrderService
{
    [OrderLoggingInterceptor]
    string PlaceOrder(string item);

    string CancelOrder(string item);
}

public class OrderService : IOrderService
{
    public string PlaceOrder(string item) => $"Order placed: {item}";

    public string CancelOrder(string item) => $"Order cancelled: {item}";
}

// ============================================================================
// Internal implementation — regression surface for the MethodAccessException fix
// (issue #274). The non-proxied method on an internal class must be callable.
// ============================================================================

public interface IInternalService
{
    [InternalInterceptor]
    string Intercepted();

    string NotIntercepted();
}

internal class InternalService : IInternalService
{
    public string Intercepted() => "original-intercepted";

    public string NotIntercepted() => "original-not-intercepted";
}

public class PublicInternalService : IInternalService
{
    public string Intercepted() => "original-intercepted";

    public string NotIntercepted() => "original-not-intercepted";
}

// ============================================================================
// Covariant return type scenario — a base class with a virtual method returning
// BaseResult, overridden in a derived class returning DerivedResult.
// ============================================================================

public class BaseResult
{
    public string Name { get; set; } = "base";
}

public class DerivedResult : BaseResult
{
    public int Extra { get; set; }
}

public class CovariantServiceBase
{
    public virtual BaseResult Get() => new() { Name = "base" };
}

public class CovariantService : CovariantServiceBase
{
    public override DerivedResult Get() => new() { Name = "derived", Extra = 42 };
}

// ============================================================================
// Async service that throws — used to verify exception propagation through the
// proxy pipeline.
// ============================================================================

public interface IAsyncService
{
    Task<string> GetNameAsync();
    Task<int> ThrowAsync();
    ValueTask<string> GetLabelAsync();
    Task ChainAsync();
}

public class AsyncService : IAsyncService
{
    public async Task<string> GetNameAsync()
    {
        await Task.Delay(1);
        return "async-name";
    }

    public Task<int> ThrowAsync() => throw new InvalidOperationException("boom");

    public async ValueTask<string> GetLabelAsync()
    {
        await Task.Delay(1);
        return "async-label";
    }

    public async Task ChainAsync()
    {
        await Task.Delay(1);
    }
}

// ============================================================================
// Interceptors — real interceptors, no mocks. They record execution order via
// a shared static list so tests can verify pipeline ordering.
// ============================================================================

public static class InterceptorLog
{
    public static readonly List<string> Entries = new();

    public static void Clear() => Entries.Clear();
}

public sealed class OrderLoggingInterceptorAttribute : AbstractInterceptorAttribute
{
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        InterceptorLog.Entries.Add("OrderLoggingInterceptor.Before");
        await context.Invoke(next);
        InterceptorLog.Entries.Add("OrderLoggingInterceptor.After");
    }
}

public sealed class InternalInterceptorAttribute : AbstractInterceptorAttribute
{
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        await context.Invoke(next);
        context.ReturnValue = "intercepted-internal";
    }
}

/// <summary>
/// Interceptor that records its execution and appends to the return value.
/// </summary>
public sealed class AppendInterceptorAttribute : AbstractInterceptorAttribute
{
    private readonly string _suffix;

    public AppendInterceptorAttribute(string suffix)
    {
        _suffix = suffix;
    }

    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        InterceptorLog.Entries.Add($"Append[{_suffix}].Before");
        await context.Invoke(next);
        context.ReturnValue = (context.ReturnValue as string ?? string.Empty) + _suffix;
        InterceptorLog.Entries.Add($"Append[{_suffix}].After");
    }
}

/// <summary>
/// Interceptor that short-circuits the pipeline by setting the return value
/// and calling Break() instead of next().
/// </summary>
public sealed class ShortCircuitInterceptorAttribute : AbstractInterceptorAttribute
{
    public override Task Invoke(AspectContext context, AspectDelegate next)
    {
        context.ReturnValue = "short-circuited";
        return context.Break();
    }
}

/// <summary>
/// First of two ordered interceptors — runs with a lower Order value (higher priority).
/// </summary>
public sealed class FirstInterceptorAttribute : AbstractInterceptorAttribute
{
    public FirstInterceptorAttribute()
    {
        Order = 1;
    }

    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        InterceptorLog.Entries.Add("First.Before");
        await context.Invoke(next);
        InterceptorLog.Entries.Add("First.After");
    }
}

/// <summary>
/// Second of two ordered interceptors — runs with a higher Order value (lower priority).
/// </summary>
public sealed class SecondInterceptorAttribute : AbstractInterceptorAttribute
{
    public SecondInterceptorAttribute()
    {
        Order = 2;
    }

    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        InterceptorLog.Entries.Add("Second.Before");
        await context.Invoke(next);
        InterceptorLog.Entries.Add("Second.After");
    }
}

/// <summary>
/// Interceptor used to verify AllowMultiple = false behavior — when applied via
/// configuration more than once, only one instance should execute.
/// AllowMultiple defaults to false on AbstractInterceptorAttribute.
/// </summary>
public sealed class OnceInterceptorAttribute : AbstractInterceptorAttribute
{
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        InterceptorLog.Entries.Add("Once");
        await context.Invoke(next);
    }
}

/// <summary>
/// Async-aware interceptor that captures and re-throws exceptions, recording them
/// in the log for verification.
/// </summary>
public sealed class AsyncExceptionInterceptorAttribute : AbstractInterceptorAttribute
{
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        try
        {
            await context.Invoke(next);
        }
        catch (Exception ex)
        {
            InterceptorLog.Entries.Add($"Caught:{ex.GetType().Name}");
            throw;
        }
    }
}
