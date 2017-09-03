using AspectCore.Injector;
using Xunit;

namespace AspectCore.Tests.Injector
{
    public class PropertyInjectionTest : InjectorTestBase
    {
        [Fact]
        public void Public_Property_Inject()
        {
            var service = ServiceResolver.Resolve<IService>();
            Assert.NotNull(service);
            Assert.NotNull(service.Logger);
            Assert.Equal(service.Logger, ServiceResolver.Resolve<ILogger>());
        }

        [Fact]
        public void NonPublic_Property_Inject()
        {
            var service = (PropertyInjectionService)ServiceResolver.Resolve<IService>();
            Assert.NotNull(service);
            Assert.NotNull(service.InternalLogger);
            Assert.Equal(service.InternalLogger, ServiceResolver.Resolve<ILogger>());
        }

        protected override void ConfigureService(IServiceContainer serviceContainer)
        {
            serviceContainer.Transients.AddType<IService, PropertyInjectionService>();
            serviceContainer.Singletons.AddType<ILogger, Logger>();
            serviceContainer.Transients.AddDelegate(typeof(IService), resolver => new PropertyInjectionService());
        }
    }
}
