using AspectCore.Configuration;
using AspectCore.Injector;
using Xunit;

namespace AspectCore.Tests.Integrate
{
    public class IoCWithProxyTests : IntegrateTestBase
    {
        [Fact]
        public void Interface_Proxy()
        {
            var transient = ServiceResolver.Resolve<ITransient>();
        }

        protected override void ConfigureService(IServiceContainer serviceContainer)
        {
            serviceContainer.AddType<ITransient, Transient>();
            serviceContainer.AddType<ILogger, Logger>();
        }

        protected override void Configure(IAspectConfiguration configuration)
        {
            configuration.Interceptors.AddDelegate(next => ctx => next(ctx), Predicates.ForService("ITransient"));
        }
    }
}