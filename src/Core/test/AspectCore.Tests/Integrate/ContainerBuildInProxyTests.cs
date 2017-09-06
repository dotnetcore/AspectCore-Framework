using AspectCore.Configuration;
using AspectCore.Injector;
using Xunit;
using AspectCore.DynamicProxy;

namespace AspectCore.Tests.Integrate
{
    public class ContainerBuildInProxyTests : IntegrateTestBase
    {
        [Fact]
        public void Interface_Proxy()
        {
            var serviceContainer = new ServiceContainer();
            ConfigureService(serviceContainer);
            serviceContainer.Configure(Configure);
            var resolver = serviceContainer.Build();

            var transient = resolver.Resolve<ITransient>();
            Assert.True(transient.IsProxy());
        }

        [Fact]
        public void Class_Proxy()
        {
            var serviceContainer = new ServiceContainer();
            ConfigureService(serviceContainer);
            serviceContainer.Configure(Configure);
            var resolver = serviceContainer.Build();

            var transient = resolver.Resolve<Transient>();
            Assert.True(transient.IsProxy());
        }

        protected override void ConfigureService(IServiceContainer serviceContainer)
        {
            serviceContainer.AddType<ITransient, Transient>();
            serviceContainer.AddType<ILogger, Logger>();
            serviceContainer.AddType<Transient>();
        }

        protected override void Configure(IAspectConfiguration configuration)
        {
            configuration.Interceptors.AddDelegate(next => ctx => next(ctx));
        }
    }
}