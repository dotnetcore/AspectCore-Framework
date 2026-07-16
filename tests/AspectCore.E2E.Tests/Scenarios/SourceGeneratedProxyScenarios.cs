using System;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.E2E.Tests.Fixtures;
using AspectCore.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// E2E tests for source-generated proxy engine: ProxyEngine.DynamicProxy,
/// ProxyEngine.Auto (with fallback to DynamicProxy), and proxy engine options.
/// Since no [AspectCoreGenerateProxy] types exist in this test assembly, the
/// source generator path falls back to dynamic proxy — exercising the fallback
/// code paths in SourceGeneratedProxyTypeGenerator.
/// </summary>
[Collection("InterceptorLog")]
public class SourceGeneratedProxyScenarios
{
    [Fact]
    public void DynamicProxyEngine_InterfaceProxy_Works()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();
        // Explicitly configure DynamicProxy engine.
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.DynamicProxy;
        });

        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        Assert.NotNull(service);
        Assert.IsNotType<CalculatorService>(service);
        Assert.Equal(7, service.Add(3, 4));
    }

    [Fact]
    public void AutoEngine_InterfaceProxy_FallsBackToDynamicProxy()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();
        // Auto engine: tries source-generated first, falls back to dynamic proxy.
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.Auto;
        });

        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        Assert.NotNull(service);
        Assert.IsNotType<CalculatorService>(service);
        Assert.Equal(15, service.Add(7, 8));
    }

    [Fact]
    public void AutoEngine_ClassProxy_FallsBackToDynamicProxy()
    {
        using var host = new TestHost();
        host.Add<CovariantService>();
        // Auto engine for class proxy.
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.Auto;
        });

        var service = host.Resolve<CovariantService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.ForService(nameof(CovariantService)));
        });

        Assert.NotNull(service);
        Assert.IsNotType<CovariantService>(service);
        var result = service.Get();
        Assert.Equal("derived", result.Name);
    }

    [Fact]
    public void AutoEngine_WithInterceptors_AllExecute()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.Auto;
        });

        InterceptorLog.Clear();
        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add("AutoEngine.Before");
                await ctx.Invoke(next);
                InterceptorLog.Entries.Add("AutoEngine.After");
            }, Predicates.Implement(typeof(ICalculatorService)));
        });

        var result = service.Add(1, 2);
        Assert.Equal(3, result);
        Assert.Contains("AutoEngine.Before", InterceptorLog.Entries);
        Assert.Contains("AutoEngine.After", InterceptorLog.Entries);
    }

    [Fact]
    public void SourceGeneratorEngine_WithoutRegistry_ThrowsMissingProxy()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();
        // SourceGenerator engine without a registry: throws because no
        // source-generated proxy type is available and the engine does not
        // allow runtime fallback by design.
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        // No [AspectCoreGenerateProxy] triggers exist in this assembly, so
        // the source generator cannot resolve a proxy type and throws.
        Assert.Throws<InvalidOperationException>(() =>
            host.Resolve<ICalculatorService>(config =>
            {
                config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                    Predicates.Implement(typeof(ICalculatorService)));
            }));
    }

    [Fact]
    public async Task AutoEngine_AsyncMethod_Works()
    {
        using var host = new TestHost();
        host.Add<IAsyncService, AsyncService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.Auto;
        });

        var service = host.Resolve<IAsyncService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IAsyncService)));
        });

        var name = await service.GetNameAsync();
        Assert.Equal("async-name", name);

        var label = await service.GetLabelAsync();
        Assert.Equal("async-label", label);
    }
}
