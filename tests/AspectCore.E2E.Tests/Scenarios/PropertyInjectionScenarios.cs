using System;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.E2E.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// E2E tests for property injection via [FromServiceContext]: properties
/// decorated with the attribute are resolved from the DI container and injected
/// into the proxy target. Uses the built-in AspectCore container (ServiceContext)
/// where property injection is natively supported. Real DI container, real
/// resolution — no mocks.
/// </summary>
[Collection("InterceptorLog")]
public class PropertyInjectionScenarios
{
    [Fact]
    public void PropertyInjection_SingleProperty_ResolvedFromContainer()
    {
        var config = new AspectConfiguration();
        config.Interceptors.AddDelegate((ctx, next) => next(ctx),
            Predicates.ForService("PropertyInjectedService"));

        var context = new ServiceContext(config);
        context.Singletons.AddType<IMessageProvider, MessageProvider>();
        context.Singletons.AddType<IPropertyInjectedService, PropertyInjectedService>();

        using var resolver = context.Build();
        var service = resolver.Resolve<IPropertyInjectedService>();

        // The property was injected from the DI container.
        var result = service.GetInjectedMessage();
        Assert.Equal("hello-from-provider", result);
    }

    [Fact]
    public void PropertyInjection_MultipleProperties_AllResolved()
    {
        var config = new AspectConfiguration();
        config.Interceptors.AddDelegate((ctx, next) => next(ctx),
            Predicates.ForService("MultiPropertyService"));

        var context = new ServiceContext(config);
        context.Singletons.AddType<IMessageProvider, MessageProvider>();
        context.Singletons.AddType<ICalculatorService, CalculatorService>();
        context.Singletons.AddType<IMultiPropertyService, MultiPropertyService>();

        using var resolver = context.Build();
        var service = resolver.Resolve<IMultiPropertyService>();

        // Both properties were injected.
        Assert.Equal("hello-from-provider", service.GetMessage());
        Assert.Equal(15, service.Calculate(7, 8));
    }

    [Fact]
    public void PropertyInjection_WithMethodInterception_Works()
    {
        InterceptorLog.Clear();
        var config = new AspectConfiguration();
        config.Interceptors.AddDelegate(async (ctx, next) =>
        {
            InterceptorLog.Entries.Add("PropInjection.Before");
            await ctx.Invoke(next);
            InterceptorLog.Entries.Add("PropInjection.After");
        }, Predicates.Implement(typeof(IPropertyInjectedService)));

        var context = new ServiceContext(config);
        context.Singletons.AddType<IMessageProvider, MessageProvider>();
        context.Singletons.AddType<IPropertyInjectedService, PropertyInjectedService>();

        using var resolver = context.Build();
        var service = resolver.Resolve<IPropertyInjectedService>();

        var result = service.GetInjectedMessage();
        Assert.Equal("hello-from-provider", result);
        Assert.Contains("PropInjection.Before", InterceptorLog.Entries);
        Assert.Contains("PropInjection.After", InterceptorLog.Entries);
    }

    [Fact]
    public void PropertyInjection_TransientDependency_ResolvedEachTime()
    {
        var config = new AspectConfiguration();
        config.Interceptors.AddDelegate((ctx, next) => next(ctx),
            Predicates.ForService("PropertyInjectedService"));

        var context = new ServiceContext(config);
        context.Transients.AddType<IMessageProvider, MessageProvider>();
        context.Transients.AddType<IPropertyInjectedService, PropertyInjectedService>();

        using var resolver = context.Build();

        // Each resolution creates a new instance with injected properties.
        var s1 = resolver.Resolve<IPropertyInjectedService>();
        var s2 = resolver.Resolve<IPropertyInjectedService>();

        Assert.NotSame(s1, s2);
        Assert.Equal("hello-from-provider", s1.GetInjectedMessage());
        Assert.Equal("hello-from-provider", s2.GetInjectedMessage());
    }

    [Fact]
    public void PropertyInjection_ScopedDependency_SameInstanceWithinScope()
    {
        var config = new AspectConfiguration();
        config.Interceptors.AddDelegate((ctx, next) => next(ctx),
            Predicates.ForService("PropertyInjectedService"));

        var context = new ServiceContext(config);
        context.Scopeds.AddType<IMessageProvider, MessageProvider>();
        context.Scopeds.AddType<IPropertyInjectedService, PropertyInjectedService>();

        using var rootResolver = context.Build();

        using var scope = rootResolver.CreateScope();
        var s1 = scope.Resolve<IPropertyInjectedService>();
        var s2 = scope.Resolve<IPropertyInjectedService>();

        // Same scope → same instance with injected properties.
        Assert.Same(s1, s2);
        Assert.Equal("hello-from-provider", s1.GetInjectedMessage());

        // Different scope → different instance.
        using var scope2 = rootResolver.CreateScope();
        var s3 = scope2.Resolve<IPropertyInjectedService>();
        Assert.NotSame(s1, s3);
        Assert.Equal("hello-from-provider", s3.GetInjectedMessage());
    }

    /// <summary>
    /// Service with a single [FromServiceContext] injected property.
    /// </summary>
    public interface IPropertyInjectedService
    {
        string GetInjectedMessage();
    }

    /// <summary>
    /// Real implementation with a [FromServiceContext] property that is
    /// resolved from the DI container by PropertyInjector.
    /// </summary>
    public class PropertyInjectedService : IPropertyInjectedService
    {
        [FromServiceContext]
        public virtual IMessageProvider? MessageProvider { get; set; }

        public string GetInjectedMessage() => MessageProvider?.GetMessage() ?? "no-provider";
    }

    /// <summary>
    /// Service with multiple [FromServiceContext] injected properties.
    /// </summary>
    public interface IMultiPropertyService
    {
        string GetMessage();
        int Calculate(int a, int b);
    }

    /// <summary>
    /// Real implementation with two [FromServiceContext] properties.
    /// </summary>
    public class MultiPropertyService : IMultiPropertyService
    {
        [FromServiceContext]
        public virtual IMessageProvider? MessageProvider { get; set; }

        [FromServiceContext]
        public virtual ICalculatorService? Calculator { get; set; }

        public string GetMessage() => MessageProvider?.GetMessage() ?? "no-provider";

        public int Calculate(int a, int b) => Calculator?.Add(a, b) ?? 0;
    }
}
