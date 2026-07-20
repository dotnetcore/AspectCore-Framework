using AspectCore.DynamicProxy;
using Castle.DynamicProxy;

namespace AspectCore.Benchmarks.Competitive;

// ── Interceptors for benchmarking ──────────────────────────────────────────

/// <summary>
/// Minimal Castle DynamicProxy interceptor - just proceeds.
/// </summary>
public sealed class CastlePassthroughInterceptor : Castle.DynamicProxy.IInterceptor
{
    public static readonly CastlePassthroughInterceptor Instance = new();

    public void Intercept(IInvocation invocation)
    {
        invocation.Proceed();
    }
}

/// <summary>
/// Castle interceptor that does minimal work (simulates logging).
/// </summary>
public sealed class CastleLoggingInterceptor : Castle.DynamicProxy.IInterceptor
{
    public static readonly CastleLoggingInterceptor Instance = new();
    public int CallCount;

    public void Intercept(IInvocation invocation)
    {
        Interlocked.Increment(ref CallCount);
        invocation.Proceed();
    }
}

/// <summary>
/// Minimal AspectCore interceptor - just calls next.
/// </summary>
public sealed class AspectCorePassthroughInterceptor : AbstractInterceptorAttribute
{
    public override Task Invoke(AspectContext context, AspectDelegate next)
    {
        return next(context);
    }
}

/// <summary>
/// AspectCore interceptor that does minimal work (simulates logging).
/// </summary>
public sealed class AspectCoreLoggingInterceptor : AbstractInterceptorAttribute
{
    public int CallCount;

    public override Task Invoke(AspectContext context, AspectDelegate next)
    {
        Interlocked.Increment(ref CallCount);
        return next(context);
    }
}
