using System;
using AspectCore.DependencyInjection;
using AspectCore.Extensions.LightInject;
using AspectCoreTest.LightInject.Fakes;
using LightInject;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCoreTest.LightInject
{
    /// <summary>
    /// Integration tests that resolve keyed services through IServiceResolver
    /// and verify the interceptor pipeline is active on the resolved proxies.
    /// </summary>
    public class KeyedServiceInterceptionTests
    {
#if NET8_0_OR_GREATER
        private static IServiceResolver BuildKeyedResolver()
        {
            var container = new ServiceContainer().RegisterDynamicProxy();
            container.Register<IService, Service>();
            container.Register<IService, Service>("alpha");
            return container.GetInstance<IServiceResolver>();
        }

        [Fact]
        public void ResolveKeyedService_ThroughServiceResolver_InterceptionIsActive()
        {
            var resolver = BuildKeyedResolver();
            var keyed = (IKeyedServiceProvider)resolver;

            var svc = keyed.GetKeyedService(typeof(IService), "alpha");
            Assert.NotNull(svc);
            var service = Assert.IsAssignableFrom<IService>(svc);

            // CacheInterceptor: same id returns the same cached Model instance.
            var first = service.Get(1);
            var second = service.Get(1);
            Assert.Same(first, second);
        }

        [Fact]
        public void ResolveKeyedService_ThroughServiceResolver_UnregisteredKeyReturnsNull()
        {
            var resolver = BuildKeyedResolver();
            var keyed = (IKeyedServiceProvider)resolver;

            var result = keyed.GetKeyedService(typeof(IService), "missing");
            Assert.Null(result);
        }

        [Fact]
        public void ResolveRequiredKeyedService_ThroughServiceResolver_InterceptionIsActive()
        {
            var resolver = BuildKeyedResolver();
            var keyed = (IKeyedServiceProvider)resolver;

            var svc = keyed.GetRequiredKeyedService(typeof(IService), "alpha");
            var service = Assert.IsAssignableFrom<IService>(svc);

            var first = service.Get(1);
            var second = service.Get(1);
            Assert.Same(first, second);
        }
#endif
    }
}
