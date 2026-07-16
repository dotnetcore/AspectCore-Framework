using System;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using AspectCore.E2E.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// E2E tests for target creation and transient services: proxy creation with
/// transient target services, target creation with constructor parameters,
/// multiple proxy instances from the same service type, and TransientServiceAccessor.
/// Real DI container, real proxy generation — no mocks.
/// </summary>
[Collection("InterceptorLog")]
public class TargetCreationScenarios
{
    [Fact]
    public void TransientService_MultipleProxies_AreDifferentInstances()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>(ServiceLifetime.Transient);

        var provider = host.CreateServiceProvider(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        // Each resolution of a transient service yields a new proxy instance.
        var s1 = provider.GetService<ICalculatorService>();
        var s2 = provider.GetService<ICalculatorService>();

        Assert.NotNull(s1);
        Assert.NotNull(s2);
        Assert.NotSame(s1, s2);
        Assert.IsNotType<CalculatorService>(s1);
        Assert.IsNotType<CalculatorService>(s2);

        // Both instances are functional.
        Assert.Equal(7, s1!.Add(3, 4));
        Assert.Equal(15, s2!.Add(7, 8));
    }

    [Fact]
    public void TransientServiceAccessor_ResolvesTransientInstance()
    {
        var services = new ServiceCollection();
        services.AddTransient<ICalculatorService, CalculatorService>();
        var provider = services.BuildServiceProvider();

        // TransientServiceAccessor resolves a fresh instance each time.
        var accessor = new TransientServiceAccessor<ICalculatorService>(provider);

        var s1 = accessor.Value;
        var s2 = accessor.Value;

        Assert.NotNull(s1);
        Assert.NotNull(s2);
        // Transient: each access returns a new instance.
        Assert.NotSame(s1, s2);
        Assert.Equal(3, s1.Add(1, 2));
        Assert.Equal(7, s2.Add(3, 4));
    }

    [Fact]
    public void TransientServiceAccessor_NullProvider_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new TransientServiceAccessor<ICalculatorService>(null!));
    }

    [Fact]
    public void ClassProxy_WithConstructorParameters_ResolvesDependencies()
    {
        using var host = new TestHost();
        host.Services.AddSingleton<IMessageProvider, MessageProvider>();
        host.Add<ICtorDependencyService, CtorDependencyService>();

        var service = host.Resolve<ICtorDependencyService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICtorDependencyService)));
        });

        // The constructor dependency was resolved and the proxy works.
        Assert.Equal("hello-from-provider", service.GetMessage());
        Assert.IsNotType<CtorDependencyService>(service);
    }

    [Fact]
    public void MultipleProxies_SameServiceType_AllWork()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>(ServiceLifetime.Transient);

        var provider = host.CreateServiceProvider(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        // Create multiple proxy instances from the same service type.
        var instances = new ICalculatorService[5];
        for (var i = 0; i < 5; i++)
        {
            instances[i] = provider.GetService<ICalculatorService>()!;
            Assert.NotNull(instances[i]);
            Assert.IsNotType<CalculatorService>(instances[i]);
        }

        // All instances are functional and independent.
        for (var i = 0; i < 5; i++)
        {
            Assert.Equal(i + 1, instances[i].Add(0, i + 1));
        }
    }

    /// <summary>
    /// Service that depends on another service via constructor injection.
    /// </summary>
    public interface ICtorDependencyService
    {
        string GetMessage();
    }

    /// <summary>
    /// Real implementation with constructor dependency on IMessageProvider.
    /// </summary>
    public class CtorDependencyService : ICtorDependencyService
    {
        private readonly IMessageProvider _messageProvider;

        public CtorDependencyService(IMessageProvider messageProvider)
        {
            _messageProvider = messageProvider;
        }

        public string GetMessage() => _messageProvider.GetMessage();
    }
}
