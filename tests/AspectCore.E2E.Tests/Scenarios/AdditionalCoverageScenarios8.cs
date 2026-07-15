using System;
using System.Linq;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using AspectCore.E2E.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// Eighth batch of E2E coverage tests — exercises ServiceContextExtensions,
/// ServiceProviderExtensions, ServiceResolverExtensions, and
/// LifetimeServiceContextExtensions. These cover the DI extension methods that
/// had 0% or low coverage. Tests use the real AspectCore DI container (not
/// Microsoft DI) — no mocks.
/// </summary>
public class AdditionalCoverageScenarios8
{
    // ========================================================================
    // ServiceContextExtensions — AddType overloads
    // ========================================================================

    [Fact]
    public void ServiceContext_AddType_WithServiceAndImplementationType_Works()
    {
        var context = new ServiceContext();
        context.AddType(typeof(ITestService), typeof(TestServiceImpl), Lifetime.Singleton);

        Assert.True(context.Contains(typeof(ITestService)));

        var resolver = context.Build();
        var service = resolver.Resolve<ITestService>();
        Assert.NotNull(service);
        Assert.Equal("test-impl", service!.GetName());
    }

    [Fact]
    public void ServiceContext_AddType_WithSingleType_Works()
    {
        var context = new ServiceContext();
        context.AddType(typeof(TestServiceImpl), Lifetime.Singleton);

        Assert.True(context.Contains(typeof(TestServiceImpl)));

        var resolver = context.Build();
        var service = resolver.Resolve<TestServiceImpl>();
        Assert.NotNull(service);
        Assert.Equal("test-impl", service!.GetName());
    }

    [Fact]
    public void ServiceContext_AddType_Generic_Works()
    {
        var context = new ServiceContext();
        context.AddType<ITestService>(Lifetime.Singleton);

        Assert.True(context.Contains(typeof(ITestService)));
    }

    [Fact]
    public void ServiceContext_AddType_GenericWithImplementation_Works()
    {
        var context = new ServiceContext();
        context.AddType<ITestService, TestServiceImpl>(Lifetime.Singleton);

        Assert.True(context.Contains(typeof(ITestService)));

        var resolver = context.Build();
        var service = resolver.Resolve<ITestService>();
        Assert.NotNull(service);
        Assert.Equal("test-impl", service!.GetName());
    }

    // ========================================================================
    // ServiceContextExtensions — AddInstance overloads
    // ========================================================================

    [Fact]
    public void ServiceContext_AddInstance_WithType_Works()
    {
        var context = new ServiceContext();
        var instance = new TestServiceImpl();
        context.AddInstance(typeof(ITestService), instance);

        Assert.True(context.Contains(typeof(ITestService)));

        var resolver = context.Build();
        var service = resolver.Resolve<ITestService>();
        Assert.Same(instance, service);
    }

    [Fact]
    public void ServiceContext_AddInstance_Generic_Works()
    {
        var context = new ServiceContext();
        var instance = new TestServiceImpl();
        context.AddInstance<ITestService>(instance);

        Assert.True(context.Contains(typeof(ITestService)));

        var resolver = context.Build();
        var service = resolver.Resolve<ITestService>();
        Assert.Same(instance, service);
    }

    // ========================================================================
    // ServiceContextExtensions — AddDelegate overloads
    // ========================================================================

    [Fact]
    public void ServiceContext_AddDelegate_WithType_Works()
    {
        var context = new ServiceContext();
        context.AddDelegate(typeof(ITestService), r => new TestServiceImpl(), Lifetime.Singleton);

        Assert.True(context.Contains(typeof(ITestService)));

        var resolver = context.Build();
        var service = resolver.Resolve<ITestService>();
        Assert.NotNull(service);
        Assert.Equal("test-impl", service!.GetName());
    }

    [Fact]
    public void ServiceContext_AddDelegate_GenericWithImplementation_Works()
    {
        var context = new ServiceContext();
        context.AddDelegate<ITestService, TestServiceImpl>(r => new TestServiceImpl(), Lifetime.Singleton);

        Assert.True(context.Contains(typeof(ITestService)));

        var resolver = context.Build();
        var service = resolver.Resolve<ITestService>();
        Assert.NotNull(service);
    }

    [Fact]
    public void ServiceContext_AddDelegate_Generic_Works()
    {
        var context = new ServiceContext();
        context.AddDelegate<ITestService>(r => new TestServiceImpl(), Lifetime.Transient);

        Assert.True(context.Contains(typeof(ITestService)));

        var resolver = context.Build();
        var service = resolver.Resolve<ITestService>();
        Assert.NotNull(service);
        Assert.Equal("test-impl", service!.GetName());
    }

    // ========================================================================
    // ServiceContextExtensions — RemoveAll
    // ========================================================================

    [Fact]
    public void ServiceContext_RemoveAll_Generic_RemovesAllMatching()
    {
        var context = new ServiceContext();
        context.AddType<ITestService, TestServiceImpl>(Lifetime.Singleton);
        context.AddType<ITestService, TestServiceImpl>(Lifetime.Transient);

        Assert.True(context.Contains(typeof(ITestService)));

        context.RemoveAll<ITestService>();

        Assert.False(context.Contains(typeof(ITestService)));
    }

    [Fact]
    public void ServiceContext_RemoveAll_WithType_RemovesAllMatching()
    {
        var context = new ServiceContext();
        context.AddType<ITestService, TestServiceImpl>(Lifetime.Singleton);
        context.AddInstance<ITestService>(new TestServiceImpl());

        Assert.True(context.Contains(typeof(ITestService)));

        context.RemoveAll(typeof(ITestService));

        Assert.False(context.Contains(typeof(ITestService)));
    }

    // ========================================================================
    // ServiceContextExtensions — ConfigureDynamicProxyEngine
    // ========================================================================

    [Fact]
    public void ServiceContext_ConfigureDynamicProxyEngine_SetsOptions()
    {
        var context = new ServiceContext();
        context.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
            options.Strict = true;
            options.AllowRuntimeFallback = false;
        });

        // Verify the options were registered.
        var resolver = context.Build();
        var options = resolver.Resolve<ProxyEngineOptions>();
        Assert.NotNull(options);
        Assert.Equal(ProxyEngine.SourceGenerator, options!.Engine);
        Assert.True(options.Strict);
        Assert.False(options.AllowRuntimeFallback);
    }

    [Fact]
    public void ServiceContext_ConfigureDynamicProxyEngine_UpdatesExistingOptions()
    {
        var context = new ServiceContext();
        context.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.DynamicProxy;
        });

        // Call again — should update the existing options, not add a new one.
        context.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.Auto;
            options.AllowRuntimeFallback = true;
        });

        var resolver = context.Build();
        var options = resolver.Resolve<ProxyEngineOptions>();
        Assert.NotNull(options);
        Assert.Equal(ProxyEngine.Auto, options!.Engine);
        Assert.True(options.AllowRuntimeFallback);
    }

    // ========================================================================
    // ServiceContextExtensions — AddSourceGeneratedProxyRegistry
    // ========================================================================

    [Fact]
    public void ServiceContext_AddSourceGeneratedProxyRegistry_RegistersRegistry()
    {
        var context = new ServiceContext();
        var registry = new TestSourceGeneratedProxyRegistry();
        context.AddSourceGeneratedProxyRegistry(registry);

        var resolver = context.Build();
        var resolved = resolver.Resolve<ISourceGeneratedProxyRegistry>();
        Assert.Same(registry, resolved);
    }

    // ========================================================================
    // ServiceProviderExtensions — Resolve, ResolveRequired, ResolveMany
    // ========================================================================

    [Fact]
    public void ServiceProviderExtensions_Resolve_Works()
    {
        var context = new ServiceContext();
        context.AddType<ITestService, TestServiceImpl>(Lifetime.Singleton);
        var resolver = context.Build();

        // IServiceResolver implements IServiceProvider, so we can use the
        // ServiceProviderExtensions.Resolve<T> method.
        var service = ((IServiceProvider)resolver).Resolve<ITestService>();
        Assert.NotNull(service);
        Assert.Equal("test-impl", service!.GetName());
    }

    [Fact]
    public void ServiceProviderExtensions_ResolveRequired_Works()
    {
        var context = new ServiceContext();
        context.AddType<ITestService, TestServiceImpl>(Lifetime.Singleton);
        var resolver = context.Build();

        var service = ((IServiceProvider)resolver).ResolveRequired<ITestService>();
        Assert.NotNull(service);
        Assert.Equal("test-impl", service!.GetName());
    }

    [Fact]
    public void ServiceProviderExtensions_ResolveMany_Works()
    {
        var context = new ServiceContext();
        context.AddType<ITestService, TestServiceImpl>(Lifetime.Singleton);
        context.AddDelegate<ITestService>(r => new TestServiceImpl2(), Lifetime.Transient);
        var resolver = context.Build();

        var services = ((IServiceProvider)resolver).ResolveMany<ITestService>();
        Assert.NotNull(services);
        Assert.Equal(2, services.Count());
    }

    // ========================================================================
    // ServiceResolverExtensions — Resolve, CreateScope, ResolveRequired, ResolveMany
    // ========================================================================

    [Fact]
    public void ServiceResolverExtensions_Resolve_Generic_Works()
    {
        var context = new ServiceContext();
        context.AddType<ITestService, TestServiceImpl>(Lifetime.Singleton);
        var resolver = context.Build();

        var service = resolver.Resolve<ITestService>();
        Assert.NotNull(service);
        Assert.Equal("test-impl", service!.GetName());
    }

    [Fact]
    public void ServiceResolverExtensions_CreateScope_Works()
    {
        var context = new ServiceContext();
        context.AddType<ITestService, TestServiceImpl>(Lifetime.Scoped);
        var resolver = context.Build();

        var scope = resolver.CreateScope();
        Assert.NotNull(scope);

        var service = scope.Resolve<ITestService>();
        Assert.NotNull(service);
        Assert.Equal("test-impl", service!.GetName());
    }

    [Fact]
    public void ServiceResolverExtensions_ResolveRequired_WithType_Works()
    {
        var context = new ServiceContext();
        context.AddType<ITestService, TestServiceImpl>(Lifetime.Singleton);
        var resolver = context.Build();

        var service = resolver.ResolveRequired(typeof(ITestService));
        Assert.NotNull(service);
        Assert.IsType<TestServiceImpl>(service);
    }

    [Fact]
    public void ServiceResolverExtensions_ResolveRequired_Generic_Works()
    {
        var context = new ServiceContext();
        context.AddType<ITestService, TestServiceImpl>(Lifetime.Singleton);
        var resolver = context.Build();

        var service = resolver.ResolveRequired<ITestService>();
        Assert.NotNull(service);
        Assert.Equal("test-impl", service!.GetName());
    }

    [Fact]
    public void ServiceResolverExtensions_ResolveMany_WithType_Works()
    {
        var context = new ServiceContext();
        context.AddType<ITestService, TestServiceImpl>(Lifetime.Singleton);
        context.AddDelegate<ITestService>(r => new TestServiceImpl2(), Lifetime.Transient);
        var resolver = context.Build();

        var services = resolver.ResolveMany(typeof(ITestService));
        Assert.NotNull(services);
        Assert.Equal(2, services.Count());
    }

    [Fact]
    public void ServiceResolverExtensions_ResolveMany_Generic_Works()
    {
        var context = new ServiceContext();
        context.AddType<ITestService, TestServiceImpl>(Lifetime.Singleton);
        context.AddDelegate<ITestService>(r => new TestServiceImpl2(), Lifetime.Transient);
        var resolver = context.Build();

        var services = resolver.ResolveMany<ITestService>();
        Assert.NotNull(services);
        Assert.Equal(2, services.Count());
    }

    // ========================================================================
    // LifetimeServiceContextExtensions — AddType, AddInstance, AddDelegate
    // ========================================================================

    [Fact]
    public void LifetimeServiceContext_AddType_WithType_Works()
    {
        var context = new ServiceContext();
        context.Singletons.AddType(typeof(TestServiceImpl));

        Assert.True(context.Contains(typeof(TestServiceImpl)));

        var resolver = context.Build();
        var service = resolver.Resolve<TestServiceImpl>();
        Assert.NotNull(service);
    }

    [Fact]
    public void LifetimeServiceContext_AddType_WithServiceAndImplementation_Works()
    {
        var context = new ServiceContext();
        context.Scopeds.AddType(typeof(ITestService), typeof(TestServiceImpl));

        Assert.True(context.Contains(typeof(ITestService)));

        var resolver = context.Build();
        var service = resolver.Resolve<ITestService>();
        Assert.NotNull(service);
        Assert.Equal("test-impl", service!.GetName());
    }

    [Fact]
    public void LifetimeServiceContext_AddType_Generic_Works()
    {
        var context = new ServiceContext();
        context.Transients.AddType<ITestService>();

        Assert.True(context.Contains(typeof(ITestService)));
    }

    [Fact]
    public void LifetimeServiceContext_AddType_GenericWithImplementation_Works()
    {
        var context = new ServiceContext();
        context.Singletons.AddType<ITestService, TestServiceImpl>();

        Assert.True(context.Contains(typeof(ITestService)));

        var resolver = context.Build();
        var service = resolver.Resolve<ITestService>();
        Assert.NotNull(service);
        Assert.Equal("test-impl", service!.GetName());
    }

    [Fact]
    public void LifetimeServiceContext_AddInstance_WithType_Works()
    {
        var context = new ServiceContext();
        var instance = new TestServiceImpl();
        context.Singletons.AddInstance(typeof(ITestService), instance);

        Assert.True(context.Contains(typeof(ITestService)));

        var resolver = context.Build();
        var service = resolver.Resolve<ITestService>();
        Assert.Same(instance, service);
    }

    [Fact]
    public void LifetimeServiceContext_AddInstance_Generic_Works()
    {
        var context = new ServiceContext();
        var instance = new TestServiceImpl();
        context.Singletons.AddInstance<ITestService>(instance);

        Assert.True(context.Contains(typeof(ITestService)));

        var resolver = context.Build();
        var service = resolver.Resolve<ITestService>();
        Assert.Same(instance, service);
    }

    [Fact]
    public void LifetimeServiceContext_AddDelegate_WithType_Works()
    {
        var context = new ServiceContext();
        context.Scopeds.AddDelegate(typeof(ITestService), r => new TestServiceImpl());

        Assert.True(context.Contains(typeof(ITestService)));

        var resolver = context.Build();
        var service = resolver.Resolve<ITestService>();
        Assert.NotNull(service);
        Assert.Equal("test-impl", service!.GetName());
    }

    [Fact]
    public void LifetimeServiceContext_AddDelegate_GenericWithImplementation_Works()
    {
        var context = new ServiceContext();
        context.Transients.AddDelegate<ITestService, TestServiceImpl>(r => new TestServiceImpl());

        Assert.True(context.Contains(typeof(ITestService)));

        var resolver = context.Build();
        var service = resolver.Resolve<ITestService>();
        Assert.NotNull(service);
    }

    [Fact]
    public void LifetimeServiceContext_AddDelegate_Generic_Works()
    {
        var context = new ServiceContext();
        context.Singletons.AddDelegate<ITestService>(r => new TestServiceImpl());

        Assert.True(context.Contains(typeof(ITestService)));

        var resolver = context.Build();
        var service = resolver.Resolve<ITestService>();
        Assert.NotNull(service);
        Assert.Equal("test-impl", service!.GetName());
    }

    // ========================================================================
    // Null argument checks — these cover the ArgumentNullException paths.
    // ========================================================================

    [Fact]
    public void ServiceContext_NullContext_ThrowsArgumentNullException()
    {
        IServiceContext? nullContext = null;
        Assert.Throws<ArgumentNullException>(() => nullContext!.AddType(typeof(ITestService)));
        Assert.Throws<ArgumentNullException>(() => nullContext!.ConfigureDynamicProxyEngine(_ => { }));
        Assert.Throws<ArgumentNullException>(() => nullContext!.AddSourceGeneratedProxyRegistry(new TestSourceGeneratedProxyRegistry()));
    }

    [Fact]
    public void ServiceProviderExtensions_NullProvider_Throws()
    {
        IServiceProvider? nullProvider = null;
        Assert.Throws<ArgumentNullException>(() => nullProvider!.Resolve<ITestService>());
        Assert.Throws<ArgumentNullException>(() => nullProvider!.ResolveRequired<ITestService>());
        Assert.Throws<ArgumentNullException>(() => nullProvider!.ResolveMany<ITestService>());
    }

    [Fact]
    public void ServiceResolverExtensions_NullResolver_Throws()
    {
        IServiceResolver? nullResolver = null;
        Assert.Throws<ArgumentNullException>(() => nullResolver!.Resolve<ITestService>());
        Assert.Throws<ArgumentNullException>(() => nullResolver!.CreateScope());
        Assert.Throws<ArgumentNullException>(() => nullResolver!.ResolveRequired(typeof(ITestService)));
        Assert.Throws<ArgumentNullException>(() => nullResolver!.ResolveRequired<ITestService>());
        Assert.Throws<ArgumentNullException>(() => nullResolver!.ResolveMany(typeof(ITestService)));
        Assert.Throws<ArgumentNullException>(() => nullResolver!.ResolveMany<ITestService>());
    }

    [Fact]
    public void LifetimeServiceContext_NullContext_Throws()
    {
        ILifetimeServiceContext? nullContext = null;
        Assert.Throws<ArgumentNullException>(() => nullContext!.AddType(typeof(ITestService)));
        Assert.Throws<ArgumentNullException>(() => nullContext!.AddType<ITestService, TestServiceImpl>());
    }

    // ========================================================================
    // Helper types
    // ========================================================================

    public interface ITestService
    {
        string GetName();
    }

    public class TestServiceImpl : ITestService
    {
        public string GetName() => "test-impl";
    }

    public class TestServiceImpl2 : ITestService
    {
        public string GetName() => "test-impl-2";
    }

    public class TestSourceGeneratedProxyRegistry : ISourceGeneratedProxyRegistry
    {
        public bool TryGetProxyType(Type serviceType, Type implementationType, SourceGeneratedProxyKind kind, out Type proxyType)
        {
            proxyType = null!;
            return false;
        }
    }
}
