using System;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Windsor;
using AspectCoreTest.Windsor.Fakes;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCoreTest.Windsor
{
    public class FacilityExtensionsTests
    {
        [Fact]
        public void AddAspectCoreFacility_NullKernel_ThrowsArgumentNullException()
        {
            IKernel kernel = null;
            Assert.Throws<ArgumentNullException>(() => kernel.AddAspectCoreFacility());
        }

        [Fact]
        public void AddAspectCoreFacility_NullWindsorContainer_ThrowsArgumentNullException()
        {
            IWindsorContainer container = null;
            Assert.Throws<ArgumentNullException>(() => container.AddAspectCoreFacility());
        }

        [Fact]
        public void AddAspectCoreFacility_WithConfigure_InvokesConfigure()
        {
            var container = new WindsorContainer();
            var configured = false;
            container.AddAspectCoreFacility(config =>
            {
                configured = true;
            });
            Assert.True(configured);
        }

        [Fact]
        public void AddAspectCoreFacility_DoesNotAddDuplicateFacility()
        {
            var container = new WindsorContainer();
            container.AddAspectCoreFacility();
            container.AddAspectCoreFacility();
            var facilities = container.Kernel.GetFacilities();
            var count = 0;
            foreach (var facility in facilities)
            {
                if (facility.GetType() == typeof(AspectCoreFacility))
                    count++;
            }
            Assert.Equal(1, count);
        }

        [Fact]
        public void AddAspectCoreFacility_IKernel_WithConfigure_InvokesConfigure()
        {
            var container = new WindsorContainer();
            var configured = false;
            container.Kernel.AddAspectCoreFacility(config =>
            {
                configured = true;
            });
            Assert.True(configured);
        }
    }

    public class WindsorServiceResolverTests
    {
        private static IWindsorContainer CreateContainer()
        {
            return new WindsorContainer().AddAspectCoreFacility();
        }

        [Fact]
        public void Resolve_ReturnsRegisteredService()
        {
            var container = CreateContainer();
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient());
            var resolver = (IServiceResolver)container.Resolve<IServiceProvider>();
            var service = resolver.Resolve(typeof(ICacheService));
            Assert.NotNull(service);
            Assert.IsAssignableFrom<ICacheService>(service);
        }

        [Fact]
        public void Resolve_ReturnsNullForUnregisteredService()
        {
            var container = CreateContainer();
            var resolver = (IServiceResolver)container.Resolve<IServiceProvider>();
            var service = resolver.Resolve(typeof(IUnregisteredService));
            Assert.Null(service);
        }

        [Fact]
        public void GetService_ReturnsRegisteredService()
        {
            var container = CreateContainer();
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient());
            var resolver = (IServiceResolver)container.Resolve<IServiceProvider>();
            var service = resolver.GetService(typeof(ICacheService));
            Assert.NotNull(service);
            Assert.IsAssignableFrom<ICacheService>(service);
        }

        [Fact]
        public void Dispose_DoesNotThrow()
        {
            var container = CreateContainer();
            var resolver = (IServiceResolver)container.Resolve<IServiceProvider>();
            resolver.Dispose();
        }

#if NET8_0_OR_GREATER
        [Fact]
        public void GetKeyedService_ThrowsNotImplemented()
        {
            var container = CreateContainer();
            var resolver = (IServiceResolver)container.Resolve<IServiceProvider>();
            var keyed = (IKeyedServiceProvider)resolver;
            Assert.Throws<NotImplementedException>(() => keyed.GetKeyedService(typeof(ICacheService), "key"));
        }

        [Fact]
        public void GetRequiredKeyedService_ThrowsNotImplemented()
        {
            var container = CreateContainer();
            var resolver = (IServiceResolver)container.Resolve<IServiceProvider>();
            var keyed = (IKeyedServiceProvider)resolver;
            Assert.Throws<NotImplementedException>(() => keyed.GetRequiredKeyedService(typeof(ICacheService), "key"));
        }
#endif
    }

    public class WindsorAspectBuilderFactoryTests
    {
        [Fact]
        public void GetBuilder_TwoMethodInfo_ReturnsBuilder()
        {
            var container = new WindsorContainer().AddAspectCoreFacility();
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient());
            var service = container.Resolve<ICacheService>();
            var result = service.Get(1);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetBuilder_ThreeMethodInfo_ReturnsBuilder()
        {
            var container = new WindsorContainer().AddAspectCoreFacility();
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient());
            var service = container.Resolve<ICacheService>();
            var result = await service.GetAsync(1);
            Assert.NotNull(result);
        }
    }

    public class AspectCoreFacilityLifecycleTests
    {
        [Fact]
        public void Terminate_RemovesComponentModelCreatedHandler()
        {
            var container = new WindsorContainer();
            container.AddAspectCoreFacility();
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient());
            var service = container.Resolve<ICacheService>();
            Assert.NotNull(service);
            container.Dispose();
        }
    }

    public interface IUnregisteredService
    {
    }
}
