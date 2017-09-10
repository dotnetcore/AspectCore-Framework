using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.Injector;

namespace AspectCore.Tests.Integrate
{
    public  class IntegrateTestBase
    {
        public IServiceResolver ServiceResolver { get; }

        public IServiceContainer ServiceContainer { get; }

        public IntegrateTestBase()
        {
            ServiceContainer = new ServiceContainer();
            ServiceContainer.Configure(Configure);
            ConfigureService(ServiceContainer);
            ServiceResolver = ServiceContainer.Build();
        }

        protected virtual void ConfigureService(IServiceContainer serviceContainer) { }

        protected virtual void Configure(IAspectConfiguration configuration) { }
    }
}