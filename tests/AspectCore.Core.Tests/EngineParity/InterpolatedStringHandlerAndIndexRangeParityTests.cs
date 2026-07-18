using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.EngineParity;

/// <summary>
/// Tests that C# 10 interpolated string handler structs and C# 8 Index/Range structs
/// are correctly handled by the Sync return/parameter path in both DynamicProxy and
/// Source Generator engines.
/// </summary>
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
        Assert.Equal(7, result); // 10 - 3 = 7
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
        Assert.Equal((2, 6), result); // offset=2, length=8-2=6
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
                    // Replace the struct return value to prove interception works.
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

    /// <summary>
    /// Uses real C# 10 interpolated-string-handler lowering at the call site:
    /// the compiler rewrites <c>proxy.Log($"Test{123}")</c> into a
    /// <c>CustomInterpolatedStringHandler</c> constructor call followed by
    /// <c>AppendLiteral</c>/<c>AppendFormatted</c> invocations, then passes the
    /// populated handler to the proxy method. This proves the proxy correctly
    /// forwards handler parameters produced by interpolation lowering, not just
    /// manually constructed structs.
    /// </summary>
    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void ClassProxy_InterpolatedHandlerArgument_Should_ForwardViaInterpolation(ProxyEngine engine)
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

        // Real $"" lowering: compiler builds the handler via AppendLiteral/AppendFormatted.
        var number = 123;
        var result = proxy.HandlerToUpper($"Test{number}");

        Assert.Equal("TEST123", result);
        Assert.Contains(nameof(InterpolatedHandlerService.HandlerToUpper), calls);
    }

    /// <summary>
    /// Exercises <see cref="InterpolatedStringHandlerArgumentAttribute"/>: the
    /// handler parameter is associated with the <c>category</c> parameter. The
    /// C# 10 compiler uses the associated argument to drive handler construction
    /// when the call site uses <c>$"..."</c> syntax.
    /// </summary>
    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void ClassProxy_InterpolatedHandlerArgumentAttribute_Should_Forward(ProxyEngine engine)
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

        // $"" lowering with [InterpolatedStringHandlerArgument("category")].
        var value = 42;
        var result = proxy.FormatWithCategory("INFO", $"Value is {value}");

        Assert.Equal("[INFO] Value is 42", result);
    }

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void ClassProxy_InterpolatedHandlerReturn_Should_AllowInterceptorReplacement(ProxyEngine engine)
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

// ── Service types for Index / Range ──────────────────────────────────────

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

// ── Service types for interpolated string handler ───────────────────────

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

    /// <summary>
    /// Accepts a custom interpolated string handler parameter. When the caller
    /// uses <c>$"..."</c> syntax the compiler lowers the call into handler
    /// construction + AppendLiteral/AppendFormatted at the call site.
    /// </summary>
    public virtual string HandlerToUpper(CustomInterpolatedStringHandler handler) =>
        handler.ToString().ToUpperInvariant();

    /// <summary>
    /// Demonstrates <see cref="InterpolatedStringHandlerArgumentAttribute"/>: the
    /// handler is associated with the <c>category</c> parameter.
    /// </summary>
    public virtual string FormatWithCategory(
        string category,
        [InterpolatedStringHandlerArgument("category")] CustomInterpolatedStringHandler handler) =>
        $"[{category}] {handler}";
}

/// <summary>
/// A simple non-ref struct interpolated string handler for testing.
/// Custom handlers must be boxable (not ref struct) to pass through the proxy's object[] args.
/// </summary>
[InterpolatedStringHandler]
public struct CustomInterpolatedStringHandler
{
    private string _buffer;

    public CustomInterpolatedStringHandler(int literalLength, int formattedCount)
    {
        _buffer = string.Empty;
    }

    /// <summary>
    /// Constructor used by <see cref="InterpolatedStringHandlerArgumentAttribute"/>
    /// when the handler is associated with the <c>category</c> parameter.
    /// </summary>
    public CustomInterpolatedStringHandler(int literalLength, int formattedCount, string category)
    {
        _buffer = string.Empty;
    }

    public void AppendLiteral(string value) => _buffer += value;

    public void AppendFormatted<T>(T value) => _buffer += value?.ToString();

    public override string ToString() => _buffer ?? string.Empty;
}
