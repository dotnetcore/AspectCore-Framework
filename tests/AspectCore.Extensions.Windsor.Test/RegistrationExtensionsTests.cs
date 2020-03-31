using System.Threading.Tasks;
using AspectCore.Extensions.Windsor;
using AspectCoreTest.Windsor.Fakes;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Xunit;

namespace AspectCoreTest.Windsor
{
    public class RegistrationExtensionsTests
    {
        private static IWindsorContainer CreateWindsorContainer()
        {
            return new WindsorContainer().AddAspectCoreFacility();
        }

        [Fact]
        public async Task AsProxy_Test()
        {
            var container = CreateWindsorContainer();
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient());
            var proxyService = container.Resolve<ICacheService>();
            Assert.Equal(proxyService.Get(1), proxyService.Get(1));
            Assert.Equal(await proxyService.GetAsync(2), await proxyService.GetAsync(2));
        }

        [Fact]
        public void AsProxyWithParamter_Test()
        {
            var container = CreateWindsorContainer();
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient(),
                Component.For<IController>().ImplementedBy<Controller>().LifestyleTransient());

            var proxyService = container.Resolve<ICacheService>();
            Assert.Equal(proxyService.Get(1), proxyService.Get(1));

            var proxyController = container.Resolve<IController>();
            Assert.Equal(proxyService.Get(100), proxyController.Execute());
        }
    }
}
