using System;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.E2E.Tests.Fixtures;
using AspectCore.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// E2E tests for C# 9 record type proxy generation. Exercises both the
/// runtime IL emit engine (DynamicProxy / ProxyGeneratorBuilder) and the
/// source generator engine ([AspectCoreGenerateProxy] types) for record
/// classes, including <c>with</c> expressions, record inheritance, generic
/// records, and init-only properties.
/// </summary>
[Collection("InterceptorLog")]
public class RecordTypeScenarios
{
    // ========================================================================
    // IL emit (dynamic proxy) engine — record class proxy via
    // ProxyGeneratorBuilder.CreateClassProxy.
    // ========================================================================

    [Fact]
    public void DynamicProxy_RecordClass_WithExpression_CopiesAndKeepsInterception()
    {
        var builder = new ProxyGeneratorBuilder();
        builder.Configure(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                await ctx.Invoke(next);
                if (ctx.ServiceMethod.Name == nameof(RecordE2EService.Label))
                {
                    ctx.ReturnValue = $"intercepted:{ctx.ReturnValue}";
                }
            }, Predicates.ForService("*RecordE2EService"));
        });
        var generator = builder.Build();

        var proxy = generator.CreateClassProxy<RecordE2EService, RecordE2EService>("alpha", 1);
        Assert.NotNull(proxy);
        Assert.True(proxy.IsProxy());
        Assert.Equal("intercepted:alpha:1", proxy.Label());

        // with expression: copy the proxy and mutate a property.
        var copy = proxy with { Name = "beta" };
        Assert.True(copy.IsProxy());
        Assert.Equal(proxy.GetType(), copy.GetType());
        Assert.Equal("beta", copy.Name);
        Assert.Equal(1, copy.Count);
        Assert.Equal("intercepted:beta:1", copy.Label());
    }

    [Fact]
    public void DynamicProxy_RecordClass_ChainedWith_KeepsInterception()
    {
        var builder = new ProxyGeneratorBuilder();
        builder.Configure(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                await ctx.Invoke(next);
                if (ctx.ServiceMethod.Name == nameof(RecordE2EService.Label))
                {
                    ctx.ReturnValue = $"intercepted:{ctx.ReturnValue}";
                }
            }, Predicates.ForService("*RecordE2EService"));
        });
        var generator = builder.Build();

        var proxy = generator.CreateClassProxy<RecordE2EService, RecordE2EService>("alpha", 1);
        Assert.Equal("intercepted:alpha:1", proxy.Label());

        var copy1 = proxy with { Name = "beta" };
        Assert.Equal("intercepted:beta:1", copy1.Label());

        var copy2 = copy1 with { Count = 5 };
        Assert.Equal("beta", copy2.Name);
        Assert.Equal(5, copy2.Count);
        Assert.Equal("intercepted:beta:5", copy2.Label());
    }

    [Fact]
    public void DynamicProxy_GenericRecord_WithExpression_CopiesAndKeepsInterception()
    {
        var builder = new ProxyGeneratorBuilder();
        builder.Configure(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                await ctx.Invoke(next);
                if (ctx.ServiceMethod.Name == nameof(GenericRecordE2EService<int>.Describe))
                {
                    ctx.ReturnValue = $"intercepted:{ctx.ReturnValue}";
                }
            }, Predicates.ForService("*GenericRecordE2EService*"));
        });
        var generator = builder.Build();

        var proxy = generator.CreateClassProxy<GenericRecordE2EService<int>, GenericRecordE2EService<int>>(42);
        Assert.NotNull(proxy);
        Assert.True(proxy.IsProxy());
        Assert.Equal("intercepted:42", proxy.Describe());

        var copy = proxy with { Value = 99 };
        Assert.True(copy.IsProxy());
        Assert.Equal(proxy.GetType(), copy.GetType());
        Assert.Equal(99, copy.Value);
        Assert.Equal("intercepted:99", copy.Describe());
    }

    [Fact]
    public void DynamicProxy_RecordWithInitProperty_WithExpression_CopiesAndKeepsInterception()
    {
        var builder = new ProxyGeneratorBuilder();
        builder.Configure(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                await ctx.Invoke(next);
                if (ctx.ServiceMethod.Name == nameof(RecordWithInitE2EService.Label))
                {
                    ctx.ReturnValue = $"intercepted:{ctx.ReturnValue}";
                }
            }, Predicates.ForService("*RecordWithInitE2EService"));
        });
        var generator = builder.Build();

        var proxy = generator.CreateClassProxy<RecordWithInitE2EService, RecordWithInitE2EService>();
        Assert.NotNull(proxy);
        Assert.True(proxy.IsProxy());

        var initialized = proxy with { Name = "alpha", Count = 7 };
        Assert.True(initialized.IsProxy());
        Assert.Equal(proxy.GetType(), initialized.GetType());
        Assert.Equal("alpha", initialized.Name);
        Assert.Equal(7, initialized.Count);
        Assert.Equal("intercepted:alpha:7", initialized.Label());

        var copy = initialized with { Name = "beta" };
        Assert.True(copy.IsProxy());
        Assert.Equal("beta", copy.Name);
        Assert.Equal(7, copy.Count);
        Assert.Equal("intercepted:beta:7", copy.Label());
    }

    // ========================================================================
    // Source generator engine — [AspectCoreGenerateProxy] record types.
    // These tests configure ProxyEngine.SourceGenerator explicitly so the
    // source-generated proxy path is exercised (not dynamic proxy fallback).
    // ========================================================================

    [Fact]
    public void SourceGenerator_RecordClass_WithExpression_CopiesAndKeepsInterception()
    {
        using var host = new TestHost();
        // Register the record's primary constructor parameters so the
        // source-generated proxy constructor can be resolved from DI.
        host.Services.AddSingleton<string>("alpha");
        host.Services.AddSingleton(typeof(int), 1);
        host.Add<RecordE2EService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<RecordE2EService>(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                await ctx.Invoke(next);
                if (ctx.ServiceMethod.Name == nameof(RecordE2EService.Label))
                {
                    ctx.ReturnValue = $"intercepted:{ctx.ReturnValue}";
                }
            }, Predicates.ForService("*RecordE2EService"));
        });

        Assert.NotNull(service);
        Assert.True(service.IsProxy());
        Assert.Equal("intercepted:alpha:1", service.Label());

        var copy = service with { Name = "beta" };
        Assert.True(copy.IsProxy());
        Assert.Equal(service.GetType(), copy.GetType());
        Assert.Equal("beta", copy.Name);
        Assert.Equal(1, copy.Count);
        Assert.Equal("intercepted:beta:1", copy.Label());
    }

    [Fact]
    public void SourceGenerator_DerivedRecord_DoesNotCrash()
    {
        // Derived records have two string parameters (Name, Extra) which cannot
        // be resolved independently from DI. The source generator still processes
        // the [AspectCoreGenerateProxy] type without crashing — the generated
        // proxy type exists in the assembly. The full with-expression behavior
        // for derived records is covered by
        // SourceGeneratorDynamicProxyParityTests.DerivedRecord tests.
        var proxyTypeName = "AspectCore.SourceGenerated.Proxies.global__AspectCore_E2E_Tests_Fixtures_DerivedRecordE2EService__global__AspectCore_E2E_Tests_Fixtures_DerivedRecordE2EService__ClassProxy";
        var proxyType = typeof(DerivedRecordE2EService).Assembly.GetType(proxyTypeName);
        Assert.NotNull(proxyType);
        Assert.True(proxyType!.IsClass);
    }

    [Fact]
    public void SourceGenerator_GenericRecord_WithExpression_CopiesAndKeepsInterception()
    {
        using var host = new TestHost();
        // Register the record's primary constructor parameter (int Value).
        host.Services.AddSingleton(typeof(int), 42);
        host.Add<GenericRecordE2EService<int>>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<GenericRecordE2EService<int>>(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                await ctx.Invoke(next);
                if (ctx.ServiceMethod.Name == nameof(GenericRecordE2EService<int>.Describe))
                {
                    ctx.ReturnValue = $"intercepted:{ctx.ReturnValue}";
                }
            }, Predicates.ForService("*GenericRecordE2EService*"));
        });

        Assert.NotNull(service);
        Assert.True(service.IsProxy());
        Assert.Equal("intercepted:42", service.Describe());

        var copy = service with { Value = 99 };
        Assert.True(copy.IsProxy());
        Assert.Equal(service.GetType(), copy.GetType());
        Assert.Equal(99, copy.Value);
        Assert.Equal("intercepted:99", copy.Describe());
    }

    [Fact]
    public void SourceGenerator_RecordWithInitProperty_WithExpression_CopiesAndKeepsInterception()
    {
        using var host = new TestHost();
        host.Add<RecordWithInitE2EService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<RecordWithInitE2EService>(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                await ctx.Invoke(next);
                if (ctx.ServiceMethod.Name == nameof(RecordWithInitE2EService.Label))
                {
                    ctx.ReturnValue = $"intercepted:{ctx.ReturnValue}";
                }
            }, Predicates.ForService("*RecordWithInitE2EService"));
        });

        Assert.NotNull(service);
        Assert.True(service.IsProxy());

        var initialized = service with { Name = "alpha", Count = 7 };
        Assert.True(initialized.IsProxy());
        Assert.Equal(service.GetType(), initialized.GetType());
        Assert.Equal("alpha", initialized.Name);
        Assert.Equal(7, initialized.Count);
        Assert.Equal("intercepted:alpha:7", initialized.Label());

        var copy = initialized with { Name = "beta" };
        Assert.True(copy.IsProxy());
        Assert.Equal("beta", copy.Name);
        Assert.Equal(7, copy.Count);
        Assert.Equal("intercepted:beta:7", copy.Label());
    }

    // ========================================================================
    // Auto engine — tries source generator first, falls back to dynamic proxy.
    // ========================================================================

    [Fact]
    public void AutoEngine_RecordClass_WithExpression_Works()
    {
        using var host = new TestHost();
        // Register the record's primary constructor parameters for DI resolution.
        host.Services.AddSingleton<string>("alpha");
        host.Services.AddSingleton(typeof(int), 1);
        host.Add<RecordE2EService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.Auto;
        });

        var service = host.Resolve<RecordE2EService>(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                await ctx.Invoke(next);
                if (ctx.ServiceMethod.Name == nameof(RecordE2EService.Label))
                {
                    ctx.ReturnValue = $"intercepted:{ctx.ReturnValue}";
                }
            }, Predicates.ForService("*RecordE2EService"));
        });

        Assert.NotNull(service);
        Assert.True(service.IsProxy());
        Assert.Equal("intercepted:alpha:1", service.Label());

        var copy = service with { Name = "beta" };
        Assert.True(copy.IsProxy());
        Assert.Equal("beta", copy.Name);
        Assert.Equal("intercepted:beta:1", copy.Label());
    }
}
