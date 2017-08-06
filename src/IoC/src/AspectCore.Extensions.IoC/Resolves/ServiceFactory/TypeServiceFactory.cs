using System;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.IoC.Resolves
{
    internal sealed class TypeServiceFactory : IServiceFactory
    {
        public ServiceKey ServiceKey { get; }

        public Type ImplementationType { get; }

        public TypeServiceFactory(TypeServiceDefinition serviceDefinition)
        {
            ServiceKey = new ServiceKey(serviceDefinition.ServiceType, serviceDefinition.Key);
            ImplementationType = serviceDefinition.ImplementationType;
        }

        public object Invoke(IServiceResolver serviceResolver)
        {
            var selector = serviceResolver.Resolve<ConstructorSelector>();
            var constructorResolver = selector.Select(ImplementationType);
            if (constructorResolver == null)
            {
                throw new InvalidOperationException($"Failed to create instance of type '{ServiceKey.ServiceType}'. Possible reason is cannot match the best constructor of type '{ImplementationType}'.");
            }
            return constructorResolver.Resolve(serviceResolver);
        }
    }
}