using System;
using System.Linq;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.Parameters;
using AspectCore.Extensions.Windsor;
using AspectCoreTest.Windsor.Fakes;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Xunit;

namespace AspectCoreTest.Windsor
{
    public class FacilityExtensionsAdditionalTests
    {
        [Fact]
        public void AddAspectCoreFacility_NullKernel_Throws()
        {
            IKernel kernel = null;
            Assert.Throws<ArgumentNullException>(() => kernel.AddAspectCoreFacility());
        }

        [Fact]
        public void AddAspectCoreFacility_NullContainer_Throws()
        {
            IWindsorContainer container = null;
            Assert.Throws<ArgumentNullException>(() => container.AddAspectCoreFacility());
        }

        [Fact]
        public void AddAspectCoreFacility_ReturnsContainer()
        {
            var container = new WindsorContainer();
            var result = container.AddAspectCoreFacility();
            Assert.NotNull(result);
            Assert.Same(container, result);
        }

        [Fact]
        public void AddAspectCoreFacility_DoesNotDuplicate()
        {
            var container = new WindsorContainer();
            container.AddAspectCoreFacility();
            container.AddAspectCoreFacility();
            var facilities = container.Kernel.GetFacilities();
            int aspectCoreCount = facilities.Count(f => f is AspectCoreFacility);
            Assert.Equal(1, aspectCoreCount);
        }

        [Fact]
        public void AddAspectCoreFacility_RegistersProxyTypeGenerator()
        {
            var container = new WindsorContainer();
            container.AddAspectCoreFacility();
            var generator = container.Resolve<IProxyTypeGenerator>();
            Assert.NotNull(generator);
        }

        [Fact]
        public void AddAspectCoreFacility_RegistersServiceProvider()
        {
            var container = new WindsorContainer();
            container.AddAspectCoreFacility();
            var provider = container.Resolve<IServiceProvider>();
            Assert.NotNull(provider);
        }

        [Fact]
        public void AddAspectCoreFacility_RegistersAspectConfiguration()
        {
            var container = new WindsorContainer();
            container.AddAspectCoreFacility();
            var config = container.Resolve<IAspectConfiguration>();
            Assert.NotNull(config);
        }

        [Fact]
        public void AddAspectCoreFacility_RegistersAspectContextFactory()
        {
            var container = new WindsorContainer();
            container.AddAspectCoreFacility();
            var factory = container.Resolve<IAspectContextFactory>();
            Assert.NotNull(factory);
        }

        [Fact]
        public void AddAspectCoreFacility_RegistersAspectCachingProvider()
        {
            var container = new WindsorContainer();
            container.AddAspectCoreFacility();
            var provider = container.Resolve<IAspectCachingProvider>();
            Assert.NotNull(provider);
        }

        [Fact]
        public void AddAspectCoreFacility_RegistersPropertyInjectorFactory()
        {
            var container = new WindsorContainer();
            container.AddAspectCoreFacility();
            var factory = container.Resolve<IPropertyInjectorFactory>();
            Assert.NotNull(factory);
        }

        [Fact]
        public void AddAspectCoreFacility_RegistersParameterInterceptorSelector()
        {
            var container = new WindsorContainer();
            container.AddAspectCoreFacility();
            var selector = container.Resolve<IParameterInterceptorSelector>();
            Assert.NotNull(selector);
        }

        [Fact]
        public void AddAspectCoreFacility_RegistersAdditionalInterceptorSelector()
        {
            var container = new WindsorContainer();
            container.AddAspectCoreFacility();
            var selector = container.Resolve<IAdditionalInterceptorSelector>();
            Assert.NotNull(selector);
        }

        [Fact]
        public void AddAspectCoreFacility_RegistersAspectValidatorBuilder()
        {
            var container = new WindsorContainer();
            container.AddAspectCoreFacility();
            var builder = container.Resolve<IAspectValidatorBuilder>();
            Assert.NotNull(builder);
        }
    }
}
