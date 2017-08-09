using System;
using System.Diagnostics;
using AspectCore.Abstractions;
using AspectCore.Extensions.IoC.Resolves;

namespace AspectCore.Extensions.IoC
{
    internal static class ServiceDefinitionExtensions
    {
        public static IServiceFactory CreateServiceFactory(this ServiceDefinition serviceDefinition)
        {
            return new PropertyInjectServiceFactory(GetDefinitionServiceFactory());

            IServiceFactory GetDefinitionServiceFactory()
            {
                if (serviceDefinition is InstanceServiceDefinition instanceServiceDefinition)
                {
                    return new InstanceServiceFactory(instanceServiceDefinition);
                }
                else if (serviceDefinition is DelegateServiceDefinition delegaetServiceDefinition)
                {
                    return new DelegateServiceFactory(delegaetServiceDefinition);
                }
                else if (serviceDefinition is TypeServiceDefinition typeServiceDefinition)
                {
                    return new TypeServiceFactory(typeServiceDefinition);
                }
                throw new InvalidOperationException("Unsupported service definition.");
            }
        }

        public static Type GetImplementationType(this ServiceDefinition serviceDefinition)
        {
            if (serviceDefinition is TypeServiceDefinition typeServiceDefinition)
            {
                return typeServiceDefinition.ImplementationType;
            }
            else if (serviceDefinition is InstanceServiceDefinition instanceServiceDefinition)
            {
                return instanceServiceDefinition.ImplementationInstance.GetType();
            }
            else if (serviceDefinition is DelegateServiceDefinition delegaetServiceDefinition)
            {
                var typeArguments = delegaetServiceDefinition.ImplementationDelegate.GetType().GenericTypeArguments;

                return typeArguments[1];
            }

            Debug.Assert(false, "ImplementationType, ImplementationInstance or ImplementationFactory must be non null");
            return null;
        }
    }
}
