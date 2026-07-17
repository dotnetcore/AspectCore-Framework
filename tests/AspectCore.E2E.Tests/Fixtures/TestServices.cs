using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.Parameters;

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
    IAsyncEnumerable<int> GetValuesAsync(CancellationToken cancellationToken = default);
    IAsyncEnumerable<int> GetValuesAndThrowAsync();
    IAsyncEnumerable<int> GetValuesWithFailingDisposeAsync();
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

    public async IAsyncEnumerable<int> GetValuesAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        yield return 1;
        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    public async IAsyncEnumerable<int> GetValuesWithFailingDisposeAsync()
    {
        try
        {
            yield return 1;
        }
        finally
        {
            await Task.Yield();
            throw new InvalidOperationException("async stream disposal failure");
        }
    }

    public async IAsyncEnumerable<int> GetValuesAndThrowAsync()
    {
        await Task.Yield();
        yield return 1;
        throw new InvalidOperationException("async stream failure");
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

// ============================================================================
// Generic service types — used by GenericServiceScenarios.
// ============================================================================

/// <summary>
/// Generic interface with a generic method and a regular method.
/// </summary>
public interface IGenericRepository<T>
{
    T GetById(int id);
    IEnumerable<T> GetAll();
    TResult Transform<TResult>(T input, Func<T, TResult> selector);
}

/// <summary>
/// Real implementation of IGenericRepository — no mocks.
/// </summary>
public class GenericRepository<T> : IGenericRepository<T>
    where T : class, new()
{
    public T GetById(int id)
    {
        var item = new T();
        return item;
    }

    public IEnumerable<T> GetAll()
    {
        return new[] { new T(), new T() };
    }

    public TResult Transform<TResult>(T input, Func<T, TResult> selector)
    {
        return selector(input);
    }
}

/// <summary>
/// Generic interface with multiple type parameters.
/// </summary>
public interface IPairService<TKey, TValue>
{
    KeyValuePair<TKey, TValue> CreatePair(TKey key, TValue value);
    TKey GetKey(KeyValuePair<TKey, TValue> pair);
    TValue GetValue(KeyValuePair<TKey, TValue> pair);
}

/// <summary>
/// Real implementation of IPairService — no mocks.
/// </summary>
public class PairService<TKey, TValue> : IPairService<TKey, TValue>
{
    public KeyValuePair<TKey, TValue> CreatePair(TKey key, TValue value)
        => new(key, value);

    public TKey GetKey(KeyValuePair<TKey, TValue> pair) => pair.Key;

    public TValue GetValue(KeyValuePair<TKey, TValue> pair) => pair.Value;
}

/// <summary>
/// Generic interface with ref/out parameters on generic methods.
/// </summary>
public interface IGenericParameterService<T>
{
    void EchoRef(ref T value);
    void GetOutput(out T output);
    T Swap(ref T first, ref T second);
}

/// <summary>
/// Real implementation of IGenericParameterService — no mocks.
/// </summary>
public class GenericParameterService<T> : IGenericParameterService<T>
{
    public void EchoRef(ref T value)
    {
        // ref is preserved as-is
    }

    public void GetOutput(out T output)
    {
        output = default!;
    }

    public T Swap(ref T first, ref T second)
    {
        (first, second) = (second, first);
        return first;
    }
}

// ============================================================================
// Service with a method that calls another injected proxied service — used by
// ComplexInterceptorScenarios for nested interceptor calls.
// ============================================================================

public interface IInnerService
{
    string Process(int input);
}

public class InnerService : IInnerService
{
    public virtual string Process(int input) => $"inner({input})";
}

public interface INestedCallService
{
    string Outer(int input);
}

public class NestedCallService : INestedCallService
{
    private readonly IInnerService _inner;

    public NestedCallService(IInnerService inner)
    {
        _inner = inner;
    }

    // Outer calls an injected IInnerService — when both are proxied, this
    // triggers a nested interceptor invocation.
    public virtual string Outer(int input)
    {
        return $"outer({_inner.Process(input)})";
    }
}

// ============================================================================
// Service that throws exceptions — used by ErrorHandlingScenarios.
// ============================================================================

public interface IThrowingService
{
    string DoWork(string input);
    Task<string> DoWorkAsync(string input);
    void ThrowImmediately();
    Task ThrowAsyncImmediately();
}

public class ThrowingService : IThrowingService
{
    public string DoWork(string input)
    {
        if (input == "fail")
        {
            throw new InvalidOperationException("sync failure");
        }
        return $"worked:{input}";
    }

    public Task<string> DoWorkAsync(string input)
    {
        if (input == "fail")
        {
            throw new InvalidOperationException("async failure");
        }
        return Task.FromResult($"worked:{input}");
    }

    public void ThrowImmediately()
    {
        throw new NotSupportedException("not supported");
    }

    public Task ThrowAsyncImmediately()
    {
        throw new TimeoutException("timed out");
    }
}

// ============================================================================
// Service with a dependency that is injected via constructor — used by
// ComplexInterceptorScenarios for DI-into-interceptor tests.
// ============================================================================

public interface IMessageProvider
{
    string GetMessage();
}

public class MessageProvider : IMessageProvider
{
    public string GetMessage() => "hello-from-provider";
}

// ============================================================================
// Interceptor that resolves a service from the DI container via
// AspectContext.ServiceProvider — used by ComplexInterceptorScenarios.
// </summary>
public sealed class DiAwareInterceptorAttribute : AbstractInterceptorAttribute
{
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        // Resolve a real service from the DI container via the context.
        var provider = context.ServiceProvider.GetService(typeof(IMessageProvider)) as IMessageProvider;
        var msg = provider?.GetMessage() ?? "no-provider";
        InterceptorLog.Entries.Add($"DiAware.Resolved:{msg}");
        await context.Invoke(next);
        // Append the provider message to the return value.
        if (context.ReturnValue is string s)
        {
            context.ReturnValue = s + ":" + msg;
        }
    }
}

// ============================================================================
// Interceptor that caches results using a static cache — used by
// ComplexInterceptorScenarios. The cache key is the method name + first parameter.
// ============================================================================

public sealed class CachingInterceptorAttribute : AbstractInterceptorAttribute
{
    private static readonly Dictionary<string, object> _cache = new();

    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        var parameters = context.GetParameters();
        var cacheKey = context.ServiceMethod.Name + ":" + (parameters.Count > 0 ? parameters[0].Value?.ToString() : "");

        lock (_cache)
        {
            if (_cache.TryGetValue(cacheKey, out var cached))
            {
                InterceptorLog.Entries.Add($"Caching.Hit:{cacheKey}");
                context.ReturnValue = cached;
                return;
            }
        }

        InterceptorLog.Entries.Add($"Caching.Miss:{cacheKey}");
        await context.Invoke(next);

        lock (_cache)
        {
            _cache[cacheKey] = context.ReturnValue;
        }
    }

    public static void ClearCache()
    {
        lock (_cache)
        {
            _cache.Clear();
        }
    }
}

// ============================================================================
// Interceptor that validates parameters and throws on invalid input — used by
// ComplexInterceptorScenarios.
// ============================================================================

public sealed class ValidationInterceptorAttribute : AbstractInterceptorAttribute
{
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        var parameters = context.GetParameters();
        if (parameters.Count > 0 && parameters[0].Value is string s && s == "invalid")
        {
            throw new ArgumentException("Parameter cannot be 'invalid'", parameters[0].Name);
        }
        await context.Invoke(next);
    }
}

// ============================================================================
// Interceptor that records entry/exit timing — used by
// ComplexInterceptorScenarios.
// ============================================================================

public sealed class TimingInterceptorAttribute : AbstractInterceptorAttribute
{
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        var sw = Stopwatch.StartNew();
        InterceptorLog.Entries.Add($"Timing.Enter:{context.ServiceMethod.Name}");
        await context.Invoke(next);
        sw.Stop();
        InterceptorLog.Entries.Add($"Timing.Exit:{context.ServiceMethod.Name}:{sw.ElapsedMilliseconds}ms");
    }
}

// ============================================================================
// Interceptor that catches and swallows exceptions — used by
// ErrorHandlingScenarios.
// ============================================================================

public sealed class SwallowExceptionInterceptorAttribute : AbstractInterceptorAttribute
{
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        try
        {
            await context.Invoke(next);
        }
        catch (Exception ex)
        {
            InterceptorLog.Entries.Add($"Swallowed:{ex.GetType().Name}");
            context.ReturnValue = "swallowed";
        }
    }
}

// ============================================================================
// Interceptor that catches and rethrows with a different exception type — used
// by ErrorHandlingScenarios.
// ============================================================================

public sealed class RethrowDifferentInterceptorAttribute : AbstractInterceptorAttribute
{
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        try
        {
            await context.Invoke(next);
        }
        catch (Exception ex)
        {
            InterceptorLog.Entries.Add($"Rethrew:{ex.GetType().Name}");
            throw new ApplicationException("wrapped by interceptor", ex);
        }
    }
}
