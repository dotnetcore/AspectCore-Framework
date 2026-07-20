using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;

namespace AspectCore.E2E.Tests.Fixtures;

// ============================================================================
// Interface service with comprehensive method signatures for exercising the
// SourceGeneratedAspectContext.Complete() code paths via the SG engine.
// ============================================================================

/// <summary>
/// Interface service exercising multiple method signatures through the
/// source-generated proxy path (SourceGeneratedAspectContext).
/// </summary>
[AspectCoreGenerateProxy]
public interface ISgBasicService
{
    int Add(int a, int b);

    void DoNothing();

    string Concat(string left, string right);

    Task<int> MultiplyAsync(int a, int b);

    Task DoNothingAsync();

    ValueTask<int> DivideAsync(int a, int b);

    ValueTask DoNothingValueTaskAsync();

    IAsyncEnumerable<int> GetNumbersAsync(int count, CancellationToken cancellationToken = default);

    void GetOutput(int input, out int doubled);

    void Increment(ref int value);

    T Echo<T>(T value);

    Task<T> EchoAsync<T>(T value);

    int MultiParam(int a, int b, int c, string label);
}

public class SgBasicService : ISgBasicService
{
    public int Add(int a, int b) => a + b;

    public void DoNothing() { }

    public string Concat(string left, string right) => left + right;

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

    public async IAsyncEnumerable<int> GetNumbersAsync(int count, [EnumeratorCancellation] CancellationToken cancellationToken = default)
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

    public T Echo<T>(T value) => value;

    public async Task<T> EchoAsync<T>(T value)
    {
        await Task.Yield();
        return value;
    }

    public int MultiParam(int a, int b, int c, string label) => a + b + c;
}

// ============================================================================
// Class service for exercising the class proxy path (base-call trampoline)
// through SourceGeneratedAspectContext.Complete().
// ============================================================================

/// <summary>
/// Class service exercising class proxy with base-call trampoline.
/// </summary>
[AspectCoreGenerateProxy]
public class SgClassService
{
    public virtual int Calculate(int x) => x * 3;

    public virtual string Greet(string name) => $"Hello, {name}";

    public virtual async Task<int> ComputeAsync(int x)
    {
        await Task.Yield();
        return x * 2;
    }

    public virtual async ValueTask<string> GetLabelAsync()
    {
        await Task.Yield();
        return "class-label";
    }

    public virtual T Identity<T>(T value) => value;
}

// ============================================================================
// Class service with init-only properties for exercising the
// MethodReflector fallback path in Complete().
// ============================================================================

/// <summary>
/// Class with init-only properties to exercise the init-only setter
/// interception path through the SG engine.
/// </summary>
[AspectCoreGenerateProxy]
public class SgInitOnlyService
{
    public virtual string Name { get; init; } = "default";

    public virtual int Count { get; init; }

    public virtual string Describe() => $"{Name}:{Count}";
}

// ============================================================================
// Interceptor attributes for the SG engine E2E tests.
// ============================================================================

/// <summary>
/// Simple pass-through interceptor that logs invocation for verification.
/// </summary>
public sealed class SgLogInterceptorAttribute : AbstractInterceptorAttribute
{
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        InterceptorLog.Entries.Add($"SgLog.Before:{context.ServiceMethod.Name}");
        await context.Invoke(next);
        InterceptorLog.Entries.Add($"SgLog.After:{context.ServiceMethod.Name}");
    }
}

/// <summary>
/// Second interceptor for stacking / multiple interceptor tests.
/// </summary>
public sealed class SgSecondInterceptorAttribute : AbstractInterceptorAttribute
{
    public SgSecondInterceptorAttribute()
    {
        Order = 2;
    }

    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        InterceptorLog.Entries.Add($"SgSecond.Before:{context.ServiceMethod.Name}");
        await context.Invoke(next);
        InterceptorLog.Entries.Add($"SgSecond.After:{context.ServiceMethod.Name}");
    }
}

/// <summary>
/// Interceptor that modifies return value — used to verify the interceptor
/// pipeline works correctly with SourceGeneratedAspectContext.
/// </summary>
public sealed class SgReturnModifierInterceptorAttribute : AbstractInterceptorAttribute
{
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        await context.Invoke(next);
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
