using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.EngineParity;

public class InterpolatedStringHandlerAndIndexRangeParityTests
{
    // ── Index / Range ──────────────────────────────────────────────────────

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void ClassProxy_IndexReturn_Should_PassThrough_And_Intercept(ProxyEngine engine)
    {
        var calls = new ConcurrentQueue<string>();
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate((ctx, next) =>
                {
                    calls.Enqueue(ctx.ServiceMethod.Name);
                    return ctx.Invoke(next);
                });
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        var proxy = proxyGenerator.CreateClassProxy<IndexRangeService>();
        Assert.True(proxy.IsProxy());
        var result = proxy.GetIndex(5);
        Assert.Equal(new Index(5), result);
        Assert.Contains(nameof(IndexRangeService.GetIndex), calls);
    }

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void ClassProxy_RangeReturn_Should_PassThrough_And_Intercept(ProxyEngine engine)
    {
        var calls = new ConcurrentQueue<string>();
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate((ctx, next) =>
                {
                    calls.Enqueue(ctx.ServiceMethod.Name);
                    return ctx.Invoke(next);
                });
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        var proxy = proxyGenerator.CreateClassProxy<IndexRangeService>();
        Assert.True(proxy.IsProxy());
        var result = proxy.GetRange(1, 5);
        Assert.Equal(new Range(1, 5), result);
        Assert.Contains(nameof(IndexRangeService.GetRange), calls);
    }

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void ClassProxy_IndexParameter_Should_BeForwarded(ProxyEngine engine)
    {
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next));
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        var proxy = proxyGenerator.CreateClassProxy<IndexRangeService>();
        Assert.True(proxy.IsProxy());
        var idx = new Index(3, fromEnd: true);
        var result = proxy.GetIndexValue(idx, 10);
        Assert.Equal(7, result);
    }

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void ClassProxy_RangeParameter_Should_BeForwarded(ProxyEngine engine)
    {
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next));
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        var proxy = proxyGenerator.CreateClassProxy<IndexRangeService>();
        Assert.True(proxy.IsProxy());
        var range = new Range(2, 8);
        var result = proxy.GetRangeOffsetAndLength(range, 20);
        Assert.Equal((2, 6), result);
    }

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public async Task ClassProxy_IndexReturn_Should_AllowInterceptorReplacement(ProxyEngine engine)
    {
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate(async (ctx, next) =>
                {
                    await ctx.Invoke(next);
                    if (ctx.ReturnValue is Index)
                        ctx.ReturnValue = new Index(99);
                });
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        var proxy = proxyGenerator.CreateClassProxy<IndexRangeService>();
        Assert.True(proxy.IsProxy());
        var result = proxy.GetIndex(5);
        Assert.Equal(new Index(99), result);
    }

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public async Task ClassProxy_RangeReturn_Should_AllowInterceptorReplacement(ProxyEngine engine)
    {
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate(async (ctx, next) =>
                {
                    await ctx.Invoke(next);
                    if (ctx.ReturnValue is Range)
                        ctx.ReturnValue = new Range(0, 0);
                });
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        var proxy = proxyGenerator.CreateClassProxy<IndexRangeService>();
        Assert.True(proxy.IsProxy());
        var result = proxy.GetRange(1, 5);
        Assert.Equal(new Range(0, 0), result);
    }

    // ── Interpolated String Handler ────────────────────────────────────────

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void ClassProxy_CustomInterpolatedHandlerReturn_Should_PassThrough(ProxyEngine engine)
    {
        var calls = new ConcurrentQueue<string>();
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate((ctx, next) =>
                {
                    calls.Enqueue(ctx.ServiceMethod.Name);
                    return ctx.Invoke(next);
                });
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        var proxy = proxyGenerator.CreateClassProxy<InterpolatedHandlerService>();
        Assert.True(proxy.IsProxy());
        var handler = proxy.BuildHandler("Hello", 42);
        Assert.Equal("Hello42", handler.ToString());
        Assert.Contains(nameof(InterpolatedHandlerService.BuildHandler), calls);
    }

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void ClassProxy_CustomInterpolatedHandlerParameter_Should_BeForwarded(ProxyEngine engine)
    {
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next));
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        var proxy = proxyGenerator.CreateClassProxy<InterpolatedHandlerService>();
        Assert.True(proxy.IsProxy());
        var handler = new CustomInterpolatedStringHandler(0, 0);
        handler.AppendLiteral("Test");
        handler.AppendFormatted(123);
        var result = proxy.HandlerToUpper(handler);
        Assert.Equal("TEST123", result);
    }

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public async Task ClassProxy_InterpolatedHandlerReturn_Should_AllowInterceptorReplacement(ProxyEngine engine)
    {
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate(async (ctx, next) =>
                {
                    await ctx.Invoke(next);
                    if (ctx.ReturnValue is CustomInterpolatedStringHandler)
                    {
                        var replacement = new CustomInterpolatedStringHandler(0, 0);
                        replacement.AppendLiteral("Replaced");
                        ctx.ReturnValue = replacement;
                    }
                });
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        var proxy = proxyGenerator.CreateClassProxy<InterpolatedHandlerService>();
        Assert.True(proxy.IsProxy());
        var result = proxy.BuildHandler("Hello", 42);
        Assert.Equal("Replaced", result.ToString());
    }
}

[AspectCoreGenerateProxy]
public class IndexRangeService
{
    public virtual Index GetIndex(int value) => new Index(value);
    public virtual Range GetRange(int start, int end) => new Range(start, end);
    public virtual int GetIndexValue(Index index, int length) => index.GetOffset(length);
    public virtual (int offset, int length) GetRangeOffsetAndLength(Range range, int length)
    {
        var (offset, len) = range.GetOffsetAndLength(length);
        return (offset, len);
    }
}

[AspectCoreGenerateProxy]
public class InterpolatedHandlerService
{
    public virtual CustomInterpolatedStringHandler BuildHandler(string text, int number)
    {
        var handler = new CustomInterpolatedStringHandler(text.Length, 1);
        handler.AppendLiteral(text);
        handler.AppendFormatted(number);
        return handler;
    }

    public virtual string HandlerToUpper(CustomInterpolatedStringHandler handler) =>
        handler.ToString().ToUpperInvariant();
}

[InterpolatedStringHandler]
public struct CustomInterpolatedStringHandler
{
    private string _buffer;

    public CustomInterpolatedStringHandler(int literalLength, int formattedCount)
    {
        _buffer = string.Empty;
    }

    public void AppendLiteral(string value) => _buffer += value;
    public void AppendFormatted<T>(T value) => _buffer += value?.ToString();
    public override string ToString() => _buffer ?? string.Empty;
}
