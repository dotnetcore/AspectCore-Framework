using System;
using AspectCore.DependencyInjection;
using AspectCore.Extensions.Windsor;
using AspectCoreTest.Windsor.Fakes;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCoreTest.Windsor
{
    /// <summary>
    /// Integration tests that resolve keyed services through IServiceResolver
    /// and verify the interceptor pipeline is active on the resolved proxies.
    /// </summary>
    public class KeyedServiceInterceptionTests
    {
        private static IWindsorContainer CreateContainer()
        {
            return new WindsorContainer().AddAspectCoreFacility();
        }

#if NET8_0_OR_GREATER
        [Fact]
        public void ResolveKeyedService_ThroughServiceResolver_InterceptionIsActive()
        {
            var container = CreateContainer();
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient().Named("alpha"));
            var resolver = (IServiceResolver)container.Resolve<IServiceProvider>();
            var keyed = (IKeyedServiceProvider)resolver;

            var svc = keyed.GetKeyedService(typeof(ICacheService), "alpha");
            Assert.NotNull(svc);
            var service = Assert.IsAssignableFrom<ICacheService>(svc);

            // CacheInterceptor: same id returns the same cached Model instance.
            var first = service.Get(1);
            var second = service.Get(1);
            Assert.Same(first, second);
        }

        [Fact]
        public void ResolveKeyedService_ThroughServiceResolver_MultipleKeysAreIndependent()
        {
            var container = CreateContainer();
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient().Named("alpha"));
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient().Named("beta"));
            var resolver = (IServiceResolver)container.Resolve<IServiceProvider>();
            var keyed = (IKeyedServiceProvider)resolver;

            var alpha = Assert.IsAssignableFrom<ICacheService>(keyed.GetKeyedService(typeof(ICacheService), "alpha"));
            var beta = Assert.IsAssignableFrom<ICacheService>(keyed.GetKeyedService(typeof(ICacheService), "beta"));

            // Each key resolves an independent proxy instance.
            Assert.NotSame(alpha, beta);

            // Both have the interceptor pipeline active (cache returns same instance for same id).
            Assert.Same(alpha.Get(1), alpha.Get(1));
            Assert.Same(beta.Get(2), beta.Get(2));
        }

        [Fact]
        public void ResolveKeyedService_ThroughServiceResolver_UnregisteredKeyReturnsNull()
        {
            var container = CreateContainer();
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient().Named("alpha"));
            var resolver = (IServiceResolver)container.Resolve<IServiceProvider>();
            var keyed = (IKeyedServiceProvider)resolver;

            var result = keyed.GetKeyedService(typeof(ICacheService), "missing");
            Assert.Null(result);
        }

        [Fact]
        public void ResolveRequiredKeyedService_ThroughServiceResolver_InterceptionIsActive()
        {
            var container = CreateContainer();
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient().Named("alpha"));
            var resolver = (IServiceResolver)container.Resolve<IServiceProvider>();
            var keyed = (IKeyedServiceProvider)resolver;

            var svc = keyed.GetRequiredKeyedService(typeof(ICacheService), "alpha");
            var service = Assert.IsAssignableFrom<ICacheService>(svc);

            var first = service.Get(1);
            var second = service.Get(1);
            Assert.Same(first, second);
        }
#endif
    }
}
