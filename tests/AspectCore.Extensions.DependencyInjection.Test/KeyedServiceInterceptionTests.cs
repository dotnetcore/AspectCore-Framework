using System;
using System.Threading.Tasks;
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
        public class IncrementInterceptor : AbstractInterceptorAttribute
        {
            private readonly int _amount;

            public IncrementInterceptor(int amount)
            {
                _amount = amount;
            }

            public override async Task Invoke(AspectContext context, AspectDelegate next)
            {
                await context.Invoke(next);
                if (context.ReturnValue is int value)
                {
                    context.ReturnValue = value + _amount;
                }
            }
        }

        public interface IKeyedCounter
        {
            int GetBase();
            int GetIntercepted();
        }

        public class KeyedCounter : IKeyedCounter
        {
            public int GetBase() => 1;

            [IncrementInterceptor(100)]
            public int GetIntercepted() => 1;
        }

#if NET8_0_OR_GREATER
        [Fact]
        public void ResolveKeyedService_ThroughServiceResolver_InterceptionIsActive()
        {
            var services = new ServiceCollection();
            services.AddKeyedTransient<IKeyedCounter, KeyedCounter>("alpha");
            var provider = services.BuildDynamicProxyProvider();
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
            var services = new ServiceCollection();
            services.AddKeyedTransient<IKeyedCounter, KeyedCounter>("alpha");
            services.AddKeyedTransient<IKeyedCounter, KeyedCounter>("beta");
            var provider = services.BuildDynamicProxyProvider();
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
            var services = new ServiceCollection();
            services.AddKeyedTransient<IKeyedCounter, KeyedCounter>("alpha");
            var provider = services.BuildDynamicProxyProvider();
            var resolver = provider.GetRequiredService<IServiceResolver>();

            var result = resolver.GetKeyedService(typeof(IKeyedCounter), "missing");
            Assert.Null(result);
        }

        [Fact]
        public void ResolveRequiredKeyedService_ThroughServiceResolver_InterceptionIsActive()
        {
            var services = new ServiceCollection();
            services.AddKeyedTransient<IKeyedCounter, KeyedCounter>("alpha");
            var provider = services.BuildDynamicProxyProvider();
            var resolver = provider.GetRequiredService<IServiceResolver>();

            var svc = resolver.GetRequiredKeyedService(typeof(IKeyedCounter), "alpha");
            var counter = Assert.IsAssignableFrom<IKeyedCounter>(svc);
            Assert.Equal(101, counter.GetIntercepted());
        }

        [Fact]
        public void ResolveRequiredKeyedService_ThroughServiceResolver_UnregisteredKeyThrows()
        {
            var services = new ServiceCollection();
            var provider = services.BuildDynamicProxyProvider();
            var resolver = provider.GetRequiredService<IServiceResolver>();

            Assert.Throws<InvalidOperationException>(() =>
                resolver.GetRequiredKeyedService(typeof(IKeyedCounter), "missing"));
        }

        [Fact]
        public void ResolveKeyedService_ThroughServiceResolver_SameAsServiceProviderDirect()
        {
            var services = new ServiceCollection();
            services.AddKeyedTransient<IKeyedCounter, KeyedCounter>("alpha");
            var provider = services.BuildDynamicProxyProvider();
            var resolver = provider.GetRequiredService<IServiceResolver>();

            // Both resolution paths should yield an intercepted proxy.
            var viaResolver = Assert.IsAssignableFrom<IKeyedCounter>(
                resolver.GetKeyedService(typeof(IKeyedCounter), "alpha"));
            var viaProvider = provider.GetKeyedService<IKeyedCounter>("alpha");

            Assert.NotNull(viaProvider);
            Assert.Equal(viaResolver.GetIntercepted(), viaProvider.GetIntercepted());
        }
#endif
    }
}
