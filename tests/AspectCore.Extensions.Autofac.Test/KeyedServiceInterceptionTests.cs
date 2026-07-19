using System;
using AspectCore.DependencyInjection;
using AspectCore.Extensions.Autofac;
using AspectCore.Extensions.Test.Fakes;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCoreTest.Autofac
{
    /// <summary>
    /// Integration tests that resolve keyed services through IServiceResolver
    /// and verify the interceptor pipeline is active on the resolved proxies.
    /// </summary>
    public class KeyedServiceInterceptionTests
    {
        private static IContainer BuildContainer(Action<ContainerBuilder> beforeBuild = null)
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();
            builder.RegisterType<Service>().As<IService>();
            beforeBuild?.Invoke(builder);
            return builder.Build();
        }

#if NET8_0_OR_GREATER
        [Fact]
        public void ResolveKeyedService_ThroughServiceResolver_InterceptionIsActive()
        {
            var container = BuildContainer(b => b.RegisterType<Service>().Keyed<IService>("alpha"));
            var resolver = container.Resolve<IServiceResolver>();
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
        public void ResolveKeyedService_ThroughServiceResolver_MultipleKeysAreIndependent()
        {
            var container = BuildContainer(b =>
            {
                b.RegisterType<Service>().Keyed<IService>("alpha");
                b.RegisterType<Service>().Keyed<IService>("beta");
            });
            var resolver = container.Resolve<IServiceResolver>();
            var keyed = (IKeyedServiceProvider)resolver;

            var alpha = Assert.IsAssignableFrom<IService>(keyed.GetKeyedService(typeof(IService), "alpha"));
            var beta = Assert.IsAssignableFrom<IService>(keyed.GetKeyedService(typeof(IService), "beta"));

            // Each key resolves an independent proxy instance.
            Assert.NotSame(alpha, beta);

            // Both have the interceptor pipeline active (cache returns same instance for same id).
            Assert.Same(alpha.Get(1), alpha.Get(1));
            Assert.Same(beta.Get(2), beta.Get(2));
        }

        [Fact]
        public void ResolveKeyedService_ThroughServiceResolver_UnregisteredKeyReturnsNull()
        {
            var container = BuildContainer(b => b.RegisterType<Service>().Keyed<IService>("alpha"));
            var resolver = container.Resolve<IServiceResolver>();
            var keyed = (IKeyedServiceProvider)resolver;

            var result = keyed.GetKeyedService(typeof(IService), "missing");
            Assert.Null(result);
        }

        [Fact]
        public void ResolveRequiredKeyedService_ThroughServiceResolver_InterceptionIsActive()
        {
            var container = BuildContainer(b => b.RegisterType<Service>().Keyed<IService>("alpha"));
            var resolver = container.Resolve<IServiceResolver>();
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
