using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// E2E tests for C# 7.0 <c>ref</c> / <c>ref readonly</c> return methods on the
/// runtime IL emit engine (DynamicProxy / <see cref="ProxyGeneratorBuilder"/>).
///
/// The interceptor pipeline is value-based, so an intercepted ref return is
/// materialised into a StrongBox&lt;T&gt; and returned by ref: reads observe the
/// (possibly interceptor-replaced) value; writes through the returned ref are
/// observable via that ref but do not propagate to the target's backing field.
/// See docs/3.CSharp-Language-Features-AOP-Emit-Adaptation.md 6.6.
/// </summary>
public class RefReturnScenarios
{
    [Fact]
    public void DynamicProxy_RefReturn_ReadsValue_AndIntercepts()
    {
        var intercepted = false;
        var builder = new ProxyGeneratorBuilder();
        builder.Configure(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                intercepted = true;
                await ctx.Invoke(next);
            }, Predicates.ForService("*RefReturnE2EService"));
        });
        using var generator = builder.Build();

        var proxy = generator.CreateClassProxy<RefReturnE2EService>();
        Assert.True(proxy.IsProxy());

        ref int slot = ref proxy.Current();
        Assert.Equal(5, slot);
        Assert.True(intercepted);
    }

    [Fact]
    public void DynamicProxy_RefReturn_InterceptorReplacesValue()
    {
        var builder = new ProxyGeneratorBuilder();
        builder.Configure(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                await ctx.Invoke(next);
                ctx.ReturnValue = 777;
            }, Predicates.ForService("*RefReturnE2EService"));
        });
        using var generator = builder.Build();

        var proxy = generator.CreateClassProxy<RefReturnE2EService>();
        ref int slot = ref proxy.Current();
        Assert.Equal(777, slot);
    }

    [Fact]
    public void DynamicProxy_RefReadOnlyReturn_ReadsValue()
    {
        var builder = new ProxyGeneratorBuilder();
        builder.Configure(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next),
                Predicates.ForService("*RefReturnE2EService"));
        });
        using var generator = builder.Build();

        var proxy = generator.CreateClassProxy<RefReturnE2EService>();
        ref readonly string name = ref proxy.Name();
        Assert.Equal("aspectcore", name);
    }
}

public class RefReturnE2EService
{
    private int _value = 5;
    private string _name = "aspectcore";

    public virtual ref int Current() => ref _value;

    public virtual ref readonly string Name() => ref _name;
}
