using System;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.E2E.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// E2E tests for nested DI scopes with proxied services: child scopes resolving
/// parent singletons, child scopes with own scoped instances, and dispose order
/// verification. Real DI containers, real proxies — no mocks.
/// </summary>
[Collection("InterceptorLog")]
public class NestedScopeScenarios
{
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

        // Resolve from root provider.
        var rootService = provider.GetService<ICalculatorService>();
        Assert.NotNull(rootService);

        // Resolve from a child scope — must be the same singleton instance.
        using var scope = provider.CreateScope();
        var childService = scope.ServiceProvider.GetService<ICalculatorService>();

        Assert.Same(rootService, childService);
        Assert.IsNotType<CalculatorService>(rootService);
    }

    [Fact]
    public void NestedScopes_ChildScopeHas_OwnScopedInstance()
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
    public void NestedScopes_SingletonInChildScope_SameAsRoot()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>(ServiceLifetime.Singleton);
        host.Add<IOrderService, OrderService>(ServiceLifetime.Scoped);

        var provider = host.CreateServiceProvider(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        var rootSingleton = provider.GetService<ICalculatorService>();

        using var scope = provider.CreateScope();
        var childSingleton = scope.ServiceProvider.GetService<ICalculatorService>();
        var childScoped = scope.ServiceProvider.GetService<IOrderService>();

        // Singleton is the same across root and child scope.
        Assert.Same(rootSingleton, childSingleton);

        // Scoped service is a proxy.
        Assert.IsNotType<OrderService>(childScoped);
    }

    [Fact]
    public void NestedScopes_DisposeOrder_ChildBeforeParent()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>(ServiceLifetime.Scoped);

        var provider = host.CreateServiceProvider(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetService<ICalculatorService>();
        Assert.NotNull(service);
        Assert.IsNotType<CalculatorService>(service);

        // Dispose child scope first — must not throw.
        scope.Dispose();

        // Parent provider still usable after child scope disposal.
        var rootService = provider.GetService<ICalculatorService>();
        Assert.NotNull(rootService);
    }

    [Fact]
    public void NestedScopes_BuiltInContainer_ScopedService_Works()
    {
        var config = new AspectConfiguration();
        config.Interceptors.AddDelegate((ctx, next) => next(ctx),
            Predicates.Implement(typeof(ICalculatorService)));

        var context = new ServiceContext(config);
        context.Scopeds.AddType<ICalculatorService, CalculatorService>();

        using var rootResolver = context.Build();

        using var scope1 = rootResolver.CreateScope();
        var s1 = scope1.Resolve<ICalculatorService>();
        var s2 = scope1.Resolve<ICalculatorService>();

        // Same scope → same instance.
        Assert.Same(s1, s2);
        Assert.IsNotType<CalculatorService>(s1);

        // Different scope → different instance.
        using var scope2 = rootResolver.CreateScope();
        var s3 = scope2.Resolve<ICalculatorService>();
        Assert.NotSame(s1, s3);
    }

    [Fact]
    public void NestedScopes_MultipleScopedServices_AreIsolated()
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

        using var scope1 = provider.CreateScope();
        var calc1 = scope1.ServiceProvider.GetRequiredService<ICalculatorService>();
        var order1 = scope1.ServiceProvider.GetRequiredService<IOrderService>();

        using var scope2 = provider.CreateScope();
        var calc2 = scope2.ServiceProvider.GetRequiredService<ICalculatorService>();
        var order2 = scope2.ServiceProvider.GetRequiredService<IOrderService>();

        // Each scope has its own instances of both services.
        Assert.NotSame(calc1, calc2);
        Assert.NotSame(order1, order2);
        Assert.IsNotType<CalculatorService>(calc1);
        Assert.IsNotType<OrderService>(order1);

        // Services are functional in both scopes.
        Assert.Equal(3, calc1.Add(1, 2));
        Assert.Equal(3, calc2.Add(1, 2));
        Assert.Equal("Order placed: x", order1.PlaceOrder("x"));
        Assert.Equal("Order placed: x", order2.PlaceOrder("x"));
    }
}
