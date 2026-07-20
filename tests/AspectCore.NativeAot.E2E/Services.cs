using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;

namespace AspectCore.NativeAot.E2E;

// ============================================================================
// Interceptors
// ============================================================================

/// <summary>
/// A simple interceptor that logs invocation and passes through.
/// </summary>
public class LogInterceptorAttribute : AbstractInterceptorAttribute
{
    public static readonly List<string> Invocations = new();

    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        Invocations.Add($"Before:{context.ServiceMethod.Name}");
        await next(context);
        Invocations.Add($"After:{context.ServiceMethod.Name}");
    }
}

/// <summary>
/// An interceptor that modifies parameters before calling next.
/// </summary>
public class ParamModifierInterceptorAttribute : AbstractInterceptorAttribute
{
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        // Double the first int parameter if present
        if (context.Parameters.Length > 0 && context.Parameters[0] is int val)
        {
            context.Parameters[0] = val * 2;
        }
        await next(context);
    }
}

/// <summary>
/// An interceptor that modifies the return value after calling next.
/// </summary>
public class ReturnModifierInterceptorAttribute : AbstractInterceptorAttribute
{
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        await next(context);
        if (context.ReturnValue is int intResult)
        {
            context.ReturnValue = intResult + 100;
        }
        else if (context.ReturnValue is string strResult)
        {
            context.ReturnValue = strResult + "_modified";
        }
    }
}

/// <summary>
/// Second interceptor for stacking tests.
/// </summary>
public class SecondInterceptorAttribute : AbstractInterceptorAttribute
{
    public static readonly List<string> Invocations = new();

    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        Invocations.Add($"Second:Before:{context.ServiceMethod.Name}");
        await next(context);
        Invocations.Add($"Second:After:{context.ServiceMethod.Name}");
    }
}

// ============================================================================
// Service Interfaces
// ============================================================================

[AspectCoreGenerateProxy]
public interface IBasicService
{
    [LogInterceptor]
    int Add(int a, int b);

    [LogInterceptor]
    void DoNothing();

    [LogInterceptor]
    Task<int> MultiplyAsync(int a, int b);

    [LogInterceptor]
    Task DoNothingAsync();

    [LogInterceptor]
    ValueTask<int> DivideAsync(int a, int b);

    [LogInterceptor]
    ValueTask DoNothingValueTaskAsync();

    [LogInterceptor]
    IAsyncEnumerable<int> GetNumbersAsync(int count);

    [LogInterceptor]
    void GetOutput(int input, out int doubled);

    [LogInterceptor]
    void Increment(ref int value);

    [LogInterceptor]
    [SecondInterceptor]
    string Stacked(string input);

    [ParamModifierInterceptor]
    int ParamModified(int value);

    [ReturnModifierInterceptor]
    int ReturnModified(int value);

    [LogInterceptor]
    T Echo<T>(T value);
}

[AspectCoreGenerateProxy]
public interface IKeyedService
{
    [LogInterceptor]
    string GetKey();
}

// ============================================================================
// Service for class proxy testing
// ============================================================================

[AspectCoreGenerateProxy]
public class ClassService
{
    [LogInterceptor]
    public virtual int Calculate(int x) => x * 3;

    [LogInterceptor]
    public virtual string Greet(string name) => $"Hello, {name}";
}

// ============================================================================
// Implementations
// ============================================================================

public class BasicService : IBasicService
{
    public int Add(int a, int b) => a + b;

    public void DoNothing() { }

    public async Task<int> MultiplyAsync(int a, int b)
    {
        await Task.Yield();
        return a * b;
    }

    public async Task DoNothingAsync()
    {
        await Task.Yield();
    }

    public async ValueTask<int> DivideAsync(int a, int b)
    {
        await Task.Yield();
        return a / b;
    }

    public async ValueTask DoNothingValueTaskAsync()
    {
        await Task.Yield();
    }

    public async IAsyncEnumerable<int> GetNumbersAsync(int count)
    {
        for (int i = 1; i <= count; i++)
        {
            await Task.Yield();
            yield return i;
        }
    }

    public void GetOutput(int input, out int doubled)
    {
        doubled = input * 2;
    }

    public void Increment(ref int value)
    {
        value += 1;
    }

    public string Stacked(string input) => $"result:{input}";

    public int ParamModified(int value) => value;

    public int ReturnModified(int value) => value;

    public T Echo<T>(T value) => value;
}

public class KeyedServiceA : IKeyedService
{
    public string GetKey() => "ServiceA";
}

public class KeyedServiceB : IKeyedService
{
    public string GetKey() => "ServiceB";
}
