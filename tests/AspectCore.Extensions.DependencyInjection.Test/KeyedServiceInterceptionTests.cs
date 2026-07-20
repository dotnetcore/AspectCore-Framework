using System;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.Extensions.DependencyInjection.Test
{
    /// <summary>
    /// Integration tests that resolve keyed services through IServiceResolver
    /// (the resolution path interceptors use) and verify the interceptor
    /// pipeline is active on the resolved proxies.
    /// </summary>
    public class KeyedServiceInterceptionTests
    {
#if NET8_0_OR_GREATER
        private static ServiceProvider BuildProvider(
            Action<IServiceCollection> configureServices,
            ProxyEngine engine = ProxyEngine.DynamicProxy)
        {
            var services = new ServiceCollection();
            services.ConfigureDynamicProxyEngine(options =>
            {
                options.Engine = engine;
                options.Strict = engine == ProxyEngine.SourceGenerator;
                options.AllowRuntimeFallback = engine == ProxyEngine.SourceGenerator ? false : (bool?)null;
            });
            configureServices(services);
            return services.BuildDynamicProxyProvider();
        }

        [Fact]
        public void ResolveKeyedService_ThroughServiceResolver_InterceptionIsActive()
        {
            var provider = BuildProvider(services =>
                services.AddKeyedTransient<IKeyedCounter, KeyedCounter>("alpha"));
            var resolver = provider.GetRequiredService<IServiceResolver>();

            var svc = resolver.GetKeyedService(typeof(IKeyedCounter), "alpha");
            Assert.NotNull(svc);
            var counter = Assert.IsAssignableFrom<IKeyedCounter>(svc);

            // Non-intercepted method returns the raw value.
            Assert.Equal(1, counter.GetBase());
            // Intercepted method: base (1) + interceptor amount (100) = 101.
            Assert.Equal(101, counter.GetIntercepted());
        }

        [Fact]
        public void ResolveKeyedService_ThroughServiceResolver_MultipleKeysAreIndependent()
        {
            var provider = BuildProvider(services =>
            {
                services.AddKeyedTransient<IKeyedCounter, KeyedCounter>("alpha");
                services.AddKeyedTransient<IKeyedCounter, KeyedCounter>("beta");
            });
            var resolver = provider.GetRequiredService<IServiceResolver>();

            var alpha = Assert.IsAssignableFrom<IKeyedCounter>(
                resolver.GetKeyedService(typeof(IKeyedCounter), "alpha"));
            var beta = Assert.IsAssignableFrom<IKeyedCounter>(
                resolver.GetKeyedService(typeof(IKeyedCounter), "beta"));

            // Each key resolves an independent proxy instance.
            Assert.NotSame(alpha, beta);

            // Both have the interceptor pipeline active.
            Assert.Equal(101, alpha.GetIntercepted());
            Assert.Equal(101, beta.GetIntercepted());
        }

        [Fact]
        public void ResolveKeyedService_ThroughServiceResolver_UnregisteredKeyReturnsNull()
        {
            var provider = BuildProvider(services =>
                services.AddKeyedTransient<IKeyedCounter, KeyedCounter>("alpha"));
            var resolver = provider.GetRequiredService<IServiceResolver>();

            var result = resolver.GetKeyedService(typeof(IKeyedCounter), "missing");
            Assert.Null(result);
        }

        [Fact]
        public void ResolveRequiredKeyedService_ThroughServiceResolver_InterceptionIsActive()
        {
            var provider = BuildProvider(services =>
                services.AddKeyedTransient<IKeyedCounter, KeyedCounter>("alpha"));
            var resolver = provider.GetRequiredService<IServiceResolver>();

            var svc = resolver.GetRequiredKeyedService(typeof(IKeyedCounter), "alpha");
            var counter = Assert.IsAssignableFrom<IKeyedCounter>(svc);
            Assert.Equal(101, counter.GetIntercepted());
        }

        [Fact]
        public void ResolveRequiredKeyedService_ThroughServiceResolver_UnregisteredKeyThrows()
        {
            var provider = BuildProvider(_ => { });
            var resolver = provider.GetRequiredService<IServiceResolver>();

            Assert.Throws<InvalidOperationException>(() =>
                resolver.GetRequiredKeyedService(typeof(IKeyedCounter), "missing"));
        }

        [Fact]
        public void ResolveKeyedService_ThroughServiceResolver_SameAsServiceProviderDirect()
        {
            var provider = BuildProvider(services =>
                services.AddKeyedTransient<IKeyedCounter, KeyedCounter>("alpha"));
            var resolver = provider.GetRequiredService<IServiceResolver>();

            // Both resolution paths should yield an intercepted proxy.
            var viaResolver = Assert.IsAssignableFrom<IKeyedCounter>(
                resolver.GetKeyedService(typeof(IKeyedCounter), "alpha"));
            var viaProvider = provider.GetKeyedService<IKeyedCounter>("alpha");

            Assert.NotNull(viaProvider);
            Assert.Equal(viaResolver.GetIntercepted(), viaProvider.GetIntercepted());
        }

        [Fact]
        public void ResolveKeyedSingleton_ThroughServiceResolver_PreservesLifetimeAndInterception()
        {
            var provider = BuildProvider(services =>
                services.AddKeyedSingleton<IKeyedCounter, KeyedCounter>("alpha"));
            var resolver = provider.GetRequiredService<IServiceResolver>();

            var first = Assert.IsAssignableFrom<IKeyedCounter>(
                resolver.GetKeyedService(typeof(IKeyedCounter), "alpha"));
            var second = Assert.IsAssignableFrom<IKeyedCounter>(
                resolver.GetKeyedService(typeof(IKeyedCounter), "alpha"));

            Assert.Same(first, second);
            Assert.Equal(101, first.GetIntercepted());
        }

        [Fact]
        public void ResolveKeyedAndNonKeyedServices_ThroughServiceResolver_AreIndependentAndIntercepted()
        {
            var provider = BuildProvider(services =>
            {
                services.AddTransient<IKeyedCounter, KeyedCounter>();
                services.AddKeyedTransient<IKeyedCounter, KeyedCounter>("alpha");
            });
            var resolver = provider.GetRequiredService<IServiceResolver>();

            var nonKeyed = Assert.IsAssignableFrom<IKeyedCounter>(
                resolver.GetRequiredService(typeof(IKeyedCounter)));
            var keyed = Assert.IsAssignableFrom<IKeyedCounter>(
                resolver.GetRequiredKeyedService(typeof(IKeyedCounter), "alpha"));

            Assert.NotSame(nonKeyed, keyed);
            Assert.Equal(101, nonKeyed.GetIntercepted());
            Assert.Equal(101, keyed.GetIntercepted());
        }

        [Theory]
        [InlineData(ProxyEngine.DynamicProxy)]
        [InlineData(ProxyEngine.SourceGenerator)]
        public void ResolveKeyedTransient_ThroughServiceResolver_InterceptionIsActive_ForBothEngines(ProxyEngine engine)
        {
            var provider = BuildProvider(services =>
                services.AddKeyedTransient<IKeyedCounter, KeyedCounter>("alpha"), engine);
            var resolver = provider.GetRequiredService<IServiceResolver>();

            var first = Assert.IsAssignableFrom<IKeyedCounter>(
                resolver.GetKeyedService(typeof(IKeyedCounter), "alpha"));
            var second = Assert.IsAssignableFrom<IKeyedCounter>(
                resolver.GetKeyedService(typeof(IKeyedCounter), "alpha"));

            Assert.NotSame(first, second);
            Assert.Equal(1, first.GetBase());
            Assert.Equal(101, first.GetIntercepted());
            Assert.Equal(101, second.GetIntercepted());
        }

        [Fact]
        public void ResolveKeyedServices_SourceGenerator_MultipleKeysMixedWithNonKeyedRemainIndependent()
        {
            var provider = BuildProvider(services =>
            {
                services.AddTransient<IKeyedCounter, KeyedCounter>();
                services.AddKeyedSingleton<IKeyedCounter, KeyedCounter>("singleton");
                services.AddKeyedTransient<IKeyedCounter, KeyedCounter>("transient");
            }, ProxyEngine.SourceGenerator);
            var resolver = provider.GetRequiredService<IServiceResolver>();

            var nonKeyed = Assert.IsAssignableFrom<IKeyedCounter>(
                resolver.GetRequiredService(typeof(IKeyedCounter)));
            var singletonFirst = Assert.IsAssignableFrom<IKeyedCounter>(
                resolver.GetRequiredKeyedService(typeof(IKeyedCounter), "singleton"));
            var singletonSecond = Assert.IsAssignableFrom<IKeyedCounter>(
                resolver.GetRequiredKeyedService(typeof(IKeyedCounter), "singleton"));
            var transientFirst = Assert.IsAssignableFrom<IKeyedCounter>(
                resolver.GetRequiredKeyedService(typeof(IKeyedCounter), "transient"));
            var transientSecond = Assert.IsAssignableFrom<IKeyedCounter>(
                resolver.GetRequiredKeyedService(typeof(IKeyedCounter), "transient"));

            Assert.NotSame(nonKeyed, singletonFirst);
            Assert.Same(singletonFirst, singletonSecond);
            Assert.NotSame(transientFirst, transientSecond);
            Assert.Equal(101, nonKeyed.GetIntercepted());
            Assert.Equal(101, singletonFirst.GetIntercepted());
            Assert.Equal(101, transientFirst.GetIntercepted());
            Assert.Null(resolver.GetKeyedService(typeof(IKeyedCounter), "missing"));
        }
#endif
    }
}
