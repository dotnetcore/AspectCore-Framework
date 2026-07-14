using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.E2E.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// E2E tests for DI container integration: service resolution, lifetime
/// management (scoped/singleton/transient), IServiceResolver resolution,
/// multi-service registration, and built-in vs Microsoft DI container
/// comparison. Real DI containers, real proxies — no mocks.
/// </summary>
[Collection("InterceptorLog")]
public class DiIntegrationScenarios
{
    [Fact]
    public void ResolveProxiedService_FromServiceProvider_Works()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var provider = host.CreateServiceProvider(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        // Resolve directly from IServiceProvider (not via Resolve<T> helper).
        var service = provider.GetService(typeof(ICalculatorService));

        Assert.NotNull(service);
        Assert.IsAssignableFrom<ICalculatorService>(service);
        // A proxy must be generated because an interceptor is configured.
        Assert.IsNotType<CalculatorService>(service);
        Assert.Equal(7, ((ICalculatorService)service!).Add(3, 4));
    }

    [Fact]
    public void ScopedService_SameInstanceWithinScope_DifferentAcrossScopes()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>(ServiceLifetime.Scoped);

        var provider = host.CreateServiceProvider(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        // Within the same scope, the same proxy instance is returned.
        using var scope1 = provider.CreateScope();
        var s1 = scope1.ServiceProvider.GetService<ICalculatorService>();
        var s2 = scope1.ServiceProvider.GetService<ICalculatorService>();
        Assert.Same(s1, s2);

        // A different scope yields a different proxy instance.
        using var scope2 = provider.CreateScope();
        var s3 = scope2.ServiceProvider.GetService<ICalculatorService>();
        Assert.NotSame(s1, s3);

        // All instances are proxies, not the original type.
        Assert.IsNotType<CalculatorService>(s1);
        Assert.IsNotType<CalculatorService>(s3);
    }

    [Fact]
    public void SingletonService_SameInstanceEverywhere()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>(ServiceLifetime.Singleton);

        var provider = host.CreateServiceProvider(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        var s1 = provider.GetService<ICalculatorService>();
        var s2 = provider.GetService<ICalculatorService>();

        // Singleton: same proxy instance from root provider.
        Assert.Same(s1, s2);
        Assert.IsNotType<CalculatorService>(s1);

        // Even from a child scope, the singleton returns the same instance.
        using var scope = provider.CreateScope();
        var s3 = scope.ServiceProvider.GetService<ICalculatorService>();
        Assert.Same(s1, s3);
    }

    [Fact]
    public void TransientService_NewInstanceEachTime()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>(ServiceLifetime.Transient);

        var provider = host.CreateServiceProvider(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        var s1 = provider.GetService<ICalculatorService>();
        var s2 = provider.GetService<ICalculatorService>();

        // Transient: a new proxy instance on every resolution.
        Assert.NotSame(s1, s2);
        Assert.IsNotType<CalculatorService>(s1);
        Assert.IsNotType<CalculatorService>(s2);
    }

    [Fact]
    public void ResolveViaIServiceResolver_Works()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var provider = host.CreateServiceProvider(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        // IServiceResolver is registered as a scoped service in the MS DI container.
        var resolver = provider.GetService<IServiceResolver>();
        Assert.NotNull(resolver);

        var service = resolver!.Resolve<ICalculatorService>();
        Assert.NotNull(service);
        Assert.IsNotType<CalculatorService>(service);
        Assert.Equal(15, service.Add(7, 8));
    }

    [Fact]
    public void MultipleServicesForSameInterface_ResolveAsIEnumerable()
    {
        using var host = new TestHost();
        // Register two different implementations against the same interface.
        host.Services.AddSingleton<ICalculatorService, CalculatorService>();
        host.Services.AddSingleton<ICalculatorService, ScientificCalculatorService>();

        var provider = host.CreateServiceProvider();

        // Resolving IEnumerable<ICalculatorService> yields all implementations.
        var services = provider.GetServices<ICalculatorService>().ToList();

        Assert.Equal(2, services.Count);
        Assert.Contains(services, s => s is CalculatorService);
        Assert.Contains(services, s => s is ScientificCalculatorService);

        // Each implementation preserves its own behavior.
        Assert.Equal("basic!", services.OfType<CalculatorService>().Single().Concat("basic"));
        Assert.Equal("scientific~", services.OfType<ScientificCalculatorService>().Single().Concat("scientific"));
    }

    [Fact]
    public void BuiltInContainer_And_MsdiContainer_BothProduceWorkingProxies()
    {
        // --- Microsoft DI container path ---
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var msdiProvider = host.CreateServiceProvider(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        var msdiService = msdiProvider.GetService<ICalculatorService>();
        Assert.NotNull(msdiService);
        Assert.IsNotType<CalculatorService>(msdiService);
        Assert.Equal(10, msdiService!.Add(4, 6));

        // --- Built-in AspectCore container path ---
        var aspectConfig = new AspectConfiguration();
        aspectConfig.Interceptors.AddDelegate((ctx, next) => next(ctx),
            Predicates.Implement(typeof(ICalculatorService)));

        var context = new ServiceContext(aspectConfig);
        context.Singletons.AddType<ICalculatorService, CalculatorService>();

        using var resolver = context.Build();
        var builtInService = resolver.Resolve<ICalculatorService>();

        Assert.NotNull(builtInService);
        Assert.IsNotType<CalculatorService>(builtInService);
        Assert.Equal(10, builtInService.Add(4, 6));

        // Both containers produce a proxy (not the original type) with correct behavior.
        Assert.Equal(msdiService.Add(1, 2), builtInService.Add(1, 2));
    }

    [Fact]
    public void BuiltInContainer_ScopedService_SameInstanceWithinScope()
    {
        var config = new AspectConfiguration();
        config.Interceptors.AddDelegate((ctx, next) => next(ctx),
            Predicates.Implement(typeof(ICalculatorService)));

        var context = new ServiceContext(config);
        context.Scopeds.AddType<ICalculatorService, CalculatorService>();

        using var rootResolver = context.Build();

        // Create a scope from the root resolver.
        using var scope = rootResolver.CreateScope();
        var s1 = scope.Resolve<ICalculatorService>();
        var s2 = scope.Resolve<ICalculatorService>();

        // Same scope → same proxy instance.
        Assert.Same(s1, s2);
        Assert.IsNotType<CalculatorService>(s1);

        // Different scope → different proxy instance.
        using var scope2 = rootResolver.CreateScope();
        var s3 = scope2.Resolve<ICalculatorService>();
        Assert.NotSame(s1, s3);
    }

    /// <summary>
    /// Second implementation of ICalculatorService for multi-service registration tests.
    /// Implements the interface directly with a distinguishable Concat behavior.
    /// </summary>
    private sealed class ScientificCalculatorService : ICalculatorService
    {
        public int Add(int a, int b) => a + b;
        public int Subtract(int a, int b) => a - b;
        public Task<int> MultiplyAsync(int a, int b) => Task.FromResult(a * b);
        public ValueTask<int> DivideAsync(int a, int b) => ValueTask.FromResult(a / b);
        public string Concat(string left, string right = "~") => left + right;
        public int Sum(int[] values) => values.Sum();
        public T Echo<T>(T value) => value;
        public void GetOutput(int input, out int doubled) => doubled = input * 2;
        public void Increment(ref int value) => value += 1;
        public string? Greet(string? name) => name == null ? "Hello, stranger" : $"Hello, {name}";
    }
}
