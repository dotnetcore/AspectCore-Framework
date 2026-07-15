using System;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Windsor;
using AspectCoreTest.Windsor.Fakes;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Xunit;

namespace AspectCoreTest.Windsor
{
    public class WindsorResolverAdditionalTests
    {
        private static IWindsorContainer CreateContainer()
        {
            return new WindsorContainer().AddAspectCoreFacility();
        }

        [Fact]
        public void Resolve_ReturnsService_WhenRegistered()
        {
            var container = CreateContainer();
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient());
            var resolver = (IServiceResolver)container.Resolve<IServiceProvider>();
            var service = resolver.Resolve(typeof(ICacheService));
            Assert.NotNull(service);
            Assert.IsAssignableFrom<ICacheService>(service);
        }

        [Fact]
        public void Resolve_ReturnsNull_WhenNotRegistered()
        {
            var container = CreateContainer();
            var resolver = (IServiceResolver)container.Resolve<IServiceProvider>();
            var service = resolver.Resolve(typeof(IUnregisteredService2));
            Assert.Null(service);
        }

        [Fact]
        public void GetService_ReturnsService_WhenRegistered()
        {
            var container = CreateContainer();
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient());
            var resolver = (IServiceResolver)container.Resolve<IServiceProvider>();
            var service = resolver.GetService(typeof(ICacheService));
            Assert.NotNull(service);
            Assert.IsAssignableFrom<ICacheService>(service);
        }

        [Fact]
        public void GetService_ReturnsNull_WhenNotRegistered()
        {
            var container = CreateContainer();
            var resolver = (IServiceResolver)container.Resolve<IServiceProvider>();
            var service = resolver.GetService(typeof(IUnregisteredService2));
            Assert.Null(service);
        }

        [Fact]
        public void Resolve_ReturnsProxy_WhenServiceHasInterceptor()
        {
            var container = CreateContainer();
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient());
            var resolver = (IServiceResolver)container.Resolve<IServiceProvider>();
            var service = resolver.Resolve(typeof(ICacheService));
            Assert.NotNull(service);
            Assert.IsAssignableFrom<ICacheService>(service);
        }

        [Fact]
        public void Resolve_MultipleServices_AllResolved()
        {
            var container = CreateContainer();
            container.Register(
                Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient(),
                Component.For<IController>().ImplementedBy<Controller>().LifestyleTransient());
            var resolver = (IServiceResolver)container.Resolve<IServiceProvider>();
            Assert.NotNull(resolver.Resolve(typeof(ICacheService)));
            Assert.NotNull(resolver.Resolve(typeof(IController)));
        }

        [Fact]
        public void Dispose_DoesNotThrow()
        {
            var container = CreateContainer();
            var resolver = (IServiceResolver)container.Resolve<IServiceProvider>();
            resolver.Dispose();
        }

        public interface IUnregisteredService2 { }
    }
}
