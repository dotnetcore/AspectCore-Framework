using System;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using AspectCore.E2E.Tests.Fixtures;
using AspectCore.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// E2E tests for DI extension internals: DynamicProxyServiceProviderFactory,
/// ServiceContextProviderFactory, nested DI scopes with proxied services,
/// service resolution from child scopes, and MS DI integration with various
/// service lifetimes. Real DI containers — no mocks.
/// </summary>
[Collection("InterceptorLog")]
public class DiExtensionInternalsScenarios
{
    [Fact]
    public void DynamicProxyServiceProviderFactory_CreateBuilder_ReturnsSameCollection()
    {
        var factory = new DynamicProxyServiceProviderFactory();
        var services = new ServiceCollection();
        services.AddSingleton<ICalculatorService, CalculatorService>();

        // CreateBuilder returns the same IServiceCollection (no-op).
        var builder = factory.CreateBuilder(services);
        Assert.Same(services, builder);
    }

    [Fact]
    public void DynamicProxyServiceProviderFactory_CreateServiceProvider_ProducesWorkingProvider()
    {
        var factory = new DynamicProxyServiceProviderFactory();
        var services = new ServiceCollection();
        services.AddSingleton<ICalculatorService, CalculatorService>();
        services.ConfigureDynamicProxy(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        var provider = factory.CreateServiceProvider(services);

        var service = provider.GetService<ICalculatorService>();
        Assert.NotNull(service);
        Assert.IsNotType<CalculatorService>(service);
        Assert.Equal(7, service!.Add(3, 4));
    }

    [Fact]
    public void DynamicProxyServiceProviderFactory_WithValidateScopes_EnforcesScopeValidation()
    {
        var factory = new DynamicProxyServiceProviderFactory(validateScopes: true);
        var services = new ServiceCollection();
        services.AddScoped<ICalculatorService, CalculatorService>();
        services.ConfigureDynamicProxy(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        var provider = factory.CreateServiceProvider(services);

        // With scope validation, resolving a scoped service from the root
        // provider throws InvalidOperationException.
        Assert.Throws<InvalidOperationException>(() =>
            provider.GetService<ICalculatorService>());
    }

    [Fact]
    public void ServiceContextProviderFactory_CreateBuilder_ConvertsToServiceContext()
    {
        var factory = new ServiceContextProviderFactory();
        var services = new ServiceCollection();
        services.AddSingleton<ICalculatorService, CalculatorService>();

        var context = factory.CreateBuilder(services);
        Assert.NotNull(context);
        Assert.IsType<ServiceContext>(context);
    }

    [Fact]
    public void ServiceContextProviderFactory_CreateServiceProvider_ProducesWorkingProvider()
    {
        var factory = new ServiceContextProviderFactory();
        var services = new ServiceCollection();
        services.AddSingleton<ICalculatorService, CalculatorService>();

        var context = factory.CreateBuilder(services);
        var provider = factory.CreateServiceProvider(context);

        var service = provider.GetService(typeof(ICalculatorService));
        Assert.NotNull(service);
        Assert.IsAssignableFrom<ICalculatorService>(service);
        Assert.Equal(15, ((ICalculatorService)service!).Add(7, 8));
    }

    [Fact]
    public void NestedScopes_ChildScopeResolves_ParentSingleton()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>(ServiceLifetime.Singleton);

        var provider = host.CreateServiceProvider(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        var rootService = provider.GetService<ICalculatorService>();
        Assert.NotNull(rootService);

        // Child scope resolves the same singleton instance.
        using var scope = provider.CreateScope();
        var childService = scope.ServiceProvider.GetService<ICalculatorService>();

        Assert.Same(rootService, childService);
        Assert.IsNotType<CalculatorService>(rootService);
    }

    [Fact]
    public void NestedScopes_ScopedService_DifferentInstancePerScope()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>(ServiceLifetime.Scoped);

        var provider = host.CreateServiceProvider(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();

        var s1 = scope1.ServiceProvider.GetService<ICalculatorService>();
        var s2 = scope2.ServiceProvider.GetService<ICalculatorService>();

        // Different scopes → different proxy instances.
        Assert.NotSame(s1, s2);
        Assert.IsNotType<CalculatorService>(s1);
        Assert.IsNotType<CalculatorService>(s2);

        // Same scope → same proxy instance.
        var s1Again = scope1.ServiceProvider.GetService<ICalculatorService>();
        Assert.Same(s1, s1Again);
    }

    [Fact]
    public void NestedScopes_TransientService_NewInstanceEachResolution()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>(ServiceLifetime.Transient);

        var provider = host.CreateServiceProvider(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        using var scope = provider.CreateScope();

        var s1 = scope.ServiceProvider.GetService<ICalculatorService>();
        var s2 = scope.ServiceProvider.GetService<ICalculatorService>();

        // Transient: new instance on every resolution.
        Assert.NotSame(s1, s2);
        Assert.IsNotType<CalculatorService>(s1);
        Assert.IsNotType<CalculatorService>(s2);
    }

    [Fact]
    public void CrossScope_ServiceResolution_WorksAcrossBoundaries()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>(ServiceLifetime.Scoped);
        host.Add<IOrderService, OrderService>(ServiceLifetime.Scoped);

        var provider = host.CreateServiceProvider(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IOrderService)));
        });

        using var scope = provider.CreateScope();

        var calc = scope.ServiceProvider.GetService<ICalculatorService>();
        var order = scope.ServiceProvider.GetService<IOrderService>();

        Assert.NotNull(calc);
        Assert.NotNull(order);
        Assert.IsNotType<CalculatorService>(calc);
        Assert.IsNotType<OrderService>(order);
        Assert.Equal(3, calc!.Add(1, 2));
        Assert.Equal("Order placed: x", order!.PlaceOrder("x"));
    }
}
