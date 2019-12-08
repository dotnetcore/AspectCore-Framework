using AspectCore.DependencyInjection;
using Xunit;

namespace AspectCore.Tests.Injector
{
    public class PropertyInjectionTest : InjectorTestBase
    {
        [Fact]
        public void Public_Property_Inject()
        {
            var service = ServiceResolver.Resolve<IPropertyInjectionService>();
            Assert.NotNull(service);
        }

        [Fact]
        public void NonPublic_Property_Inject()
        {
            var service = (PropertyInjectionService)ServiceResolver.Resolve<IPropertyInjectionService>();
            Assert.NotNull(service);
            Assert.NotNull(service.InternalLogger);
            Assert.Equal(service.InternalLogger, ServiceResolver.Resolve<ILogger>());
        }

        protected override void ConfigureService(IServiceContext serviceContext)
        {
            serviceContext.Transients.AddType<IPropertyInjectionService, PropertyInjectionService>();
            serviceContext.Singletons.AddType<ILogger, Logger>();
            serviceContext.Transients.AddDelegate(typeof(IPropertyInjectionService), resolver => new PropertyInjectionService());
        }
    }
}
