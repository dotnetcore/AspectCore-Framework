using System;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.CastleCompat;
using Castle.DynamicProxy;
using Xunit;
using CastleProxyGenerator = Castle.DynamicProxy.ProxyGenerator;

namespace AspectCore.Extensions.CastleCompat.Tests;

// ── Services for reverse adapter testing ────────────────────────────────

public interface IReverseService
{
    string Process(string input);
    int Compute(int a, int b);
}

public class ReverseServiceImpl : IReverseService
{
    public string Process(string input) => $"processed:{input}";
    public int Compute(int a, int b) => a + b;
}

// ── AspectCore interceptors for testing ─────────────────────────────────

public sealed class PrefixAspectCoreInterceptor : AbstractInterceptorAttribute
{
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        await next(context);
        if (context.ReturnValue is string s)
        {
            context.ReturnValue = $"[intercepted]{s}";
        }
    }
}

public sealed class DoublingAspectCoreInterceptor : AbstractInterceptorAttribute
{
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        await next(context);
        if (context.ReturnValue is int val)
        {
            context.ReturnValue = val * 2;
        }
    }
}

// ── Tests ───────────────────────────────────────────────────────────────

public class AspectCoreInterceptorAdapterTests
{
    [Fact]
    public void AspectCoreInterceptor_Works_In_Castle_Pipeline()
    {
        var generator = new CastleProxyGenerator();
        var adapter = new AspectCoreInterceptorAdapter(new PrefixAspectCoreInterceptor());

        var proxy = generator.CreateInterfaceProxyWithTarget<IReverseService>(
            new ReverseServiceImpl(), adapter);

        var result = proxy.Process("hello");

        Assert.Equal("[intercepted]processed:hello", result);
    }

    [Fact]
    public void AspectCoreInterceptor_DoublingInterceptor_Works()
    {
        var generator = new CastleProxyGenerator();
        var adapter = new AspectCoreInterceptorAdapter(new DoublingAspectCoreInterceptor());

        var proxy = generator.CreateInterfaceProxyWithTarget<IReverseService>(
            new ReverseServiceImpl(), adapter);

        // (3+4)*2 = 14
        var result = proxy.Compute(3, 4);

        Assert.Equal(14, result);
    }

    [Fact]
    public void Null_AspectCoreInterceptor_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new AspectCoreInterceptorAdapter(null!));
    }
}
