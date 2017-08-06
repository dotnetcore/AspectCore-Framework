using AspectCore.Abstractions;

namespace AspectCore.Extensions.IoC.Resolves
{
    internal class InstanceServiceFactory : IServiceFactory
    {
        public ServiceKey ServiceKey { get; }

        public object ImplementationInstance { get; }

        public InstanceServiceFactory(InstanceServiceDefinition serviceDefinition)
        {
            ServiceKey = new ServiceKey(serviceDefinition.ServiceType, serviceDefinition.Key);
            ImplementationInstance = serviceDefinition.ImplementationInstance;
        }

        public InstanceServiceFactory(ServiceKey serviceKey, object implementationInstance)
        {
            ServiceKey = serviceKey;
            ImplementationInstance = implementationInstance;
        }

        public object Invoke(IServiceResolver resolver)
        {
            return ImplementationInstance;
        }
    }
}