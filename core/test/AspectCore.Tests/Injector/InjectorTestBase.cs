using AspectCore.Injector;
using AspectCore.DynamicProxy;

namespace AspectCore.Tests.Injector
{
    public class InjectorTestBase
    {
        public IServiceResolver ServiceResolver { get; }

        public IServiceContainer ServiceContainer { get; }

        public InjectorTestBase()
        {
            ServiceContainer = new ServiceContainer();
            ConfigureService(ServiceContainer);
            ServiceResolver = ServiceContainer.Build();
        }

        protected virtual void ConfigureService(IServiceContainer serviceContainer) { }
    }
}
