using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.Abstractions;
using AspectCore.Extensions.IoC.Resolves;

namespace AspectCore.Extensions.IoC
{
    internal static class ServiceDefinitionExtensions
    {
        public static IServiceFactory CreateServiceFactory(this ServiceDefinition serviceDefinition)
        {

            if(serviceDefinition is InstanceServiceDefinition instanceServiceDefinition)
            {
                return new InstanceServiceFactory(instanceServiceDefinition);
            }
            else if(serviceDefinition is DelegateServiceDefinition delegaetServiceDefinition)
            {
                return /*new DelegateServiceFactory(delegaetServiceDefinition);*/ null;
            }
            else if(serviceDefinition is TypeServiceDefinition typeServiceDefinition)
            {
                return null;
            }


            return null;
        }
    }
}
