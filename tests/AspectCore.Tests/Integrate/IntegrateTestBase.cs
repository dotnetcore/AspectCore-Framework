using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.DependencyInjection;

namespace AspectCore.Tests.Integrate
{
    public  class IntegrateTestBase
    {
        public IServiceResolver ServiceResolver { get; }

        public IServiceContext ServiceContext { get; }

        public IntegrateTestBase()
        {
            ServiceContext = new ServiceContext();
            ServiceContext.Configure(Configure);
            ConfigureService(ServiceContext);
            ServiceResolver = ServiceContext.Build();
        }

        protected virtual void ConfigureService(IServiceContext serviceContext) { }

        protected virtual void Configure(IAspectConfiguration configuration) { }
    }
}