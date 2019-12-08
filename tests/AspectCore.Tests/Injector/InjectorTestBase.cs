using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;

namespace AspectCore.Tests.Injector
{
    public class InjectorTestBase
    {
        public IServiceResolver ServiceResolver { get; }

        public IServiceContext ServiceContext { get; }

        public InjectorTestBase()
        {
            ServiceContext = new ServiceContext();
            ConfigureService(ServiceContext);
            ServiceResolver = ServiceContext.Build();
        }

        protected virtual void ConfigureService(IServiceContext serviceContext) { }
    }
}
