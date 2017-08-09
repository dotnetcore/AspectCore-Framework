using System;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.IoC.Resolves
{
    internal sealed class DelegateServiceFactory : IServiceFactory
    {
        private readonly Func<IServiceResolver, object> _implementationDelegate;

        public ServiceKey ServiceKey { get; }

        public ServiceDefinition ServiceDefinition { get; }

        public DelegateServiceFactory(DelegateServiceDefinition serviceDefinition)
        {
            ServiceDefinition = serviceDefinition;
            ServiceKey = new ServiceKey(serviceDefinition.ServiceType, serviceDefinition.Key);
            _implementationDelegate = serviceDefinition.ImplementationDelegate;
        }

        public object Invoke(IServiceResolver serviceResolver)
        {
            return _implementationDelegate(serviceResolver);
        }
    }
}