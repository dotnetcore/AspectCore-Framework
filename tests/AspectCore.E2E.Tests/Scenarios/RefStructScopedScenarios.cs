using System;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.DependencyInjection;
using AspectCore.E2E.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// E2E tests for ref struct type rejection and scoped parameter forwarding.
/// All tests run through the real proxy pipeline with interceptors configured.
/// </summary>
public class RefStructScopedScenarios
{
    [Fact]
    public void ScopedRef_Parameter_ThroughProxy_Works()
    {
        using var host = new TestHost();
        host.Add<IScopedService, ScopedService>();

        var service = host.Resolve<IScopedService>();

        int value = 42;
        service.SetValue(ref value);
        Assert.Equal(42, value);
    }

    [Fact]
    public void ScopedRef_Parameter_WithInterceptor_Works()
    {
        using var host = new TestHost();
        host.Add<IScopedService, ScopedService>();

        bool intercepted = false;
        var service = host.Resolve<IScopedService>(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                intercepted = true;
                await ctx.Invoke(next);
            }, Predicates.Implement(typeof(IScopedService)));
        });

        int value = 10;
        service.SetValue(ref value);
        Assert.True(intercepted);
        Assert.Equal(10, value);
    }

    [Fact]
    public void RefStruct_As_ServiceType_Throws_NotSupportedException()
    {
        var generator = new ProxyGeneratorBuilder().Build().TypeGenerator;

        Assert.Throws<NotSupportedException>(() =>
            generator.CreateInterfaceProxyType(typeof(ISpanInterface), typeof(SpanImpl)));
    }

    [Fact]
    public void RefStruct_Rejection_Message_Contains_RefStruct_And_Span()
    {
        var generator = new ProxyGeneratorBuilder().Build().TypeGenerator;

        var ex = Assert.Throws<NotSupportedException>(() =>
            generator.CreateInterfaceProxyType(typeof(ISpanInterface), typeof(SpanImpl)));

        Assert.Contains("ref struct", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Span", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}

public interface IScopedService
{
    void SetValue(scoped ref int value);
}

public class ScopedService : IScopedService
{
    public void SetValue(scoped ref int value)
    {
        value = value * 1;
    }
}

public interface ISpanInterface
{
    int GetLength();
}

public ref struct SpanImpl : ISpanInterface
{
    private readonly Span<byte> _data;

    public SpanImpl(Span<byte> data)
    {
        _data = data;
    }

    public int GetLength() => _data.Length;
}
