using System;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.IoC.Resolves
{
    internal sealed class PropertyInjectServiceFactory : IServiceFactory
    {
        private readonly IServiceFactory _serviceFactory;

        public ServiceKey ServiceKey { get; }

        public ServiceDefinition ServiceDefinition { get; }

        public PropertyInjectServiceFactory(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
            ServiceKey = serviceFactory.ServiceKey;
            ServiceDefinition = serviceFactory.ServiceDefinition;
        }

        public object Invoke(IServiceResolver serviceResolver)
        {
            var result = _serviceFactory.Invoke(serviceResolver);
            if (result != null)
            {
                var injector = serviceResolver.Resolve<IPropertyInjectorFactory>().Create(result.GetType());
                injector.Invoke(result);
            }
            return result;
        }
    }
}