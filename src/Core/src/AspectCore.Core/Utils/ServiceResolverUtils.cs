using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.Abstractions;
using AspectCore.Core.Injector;

namespace AspectCore.Core.Utils
{
    internal static class ServiceResolverUtils
    {
        internal static Func<IServiceResolver, object> CreateFactory(ServiceDefinition service)
        {
            if(service is InstanceServiceDefinition instanceServiceDefinition)
            {
                return new Func<IServiceResolver, object>(resolver => instanceServiceDefinition.ImplementationInstance);
            }
            else if(service is DelegateServiceDefinition delegateServiceDefinition)
            {
                return delegateServiceDefinition.ImplementationDelegate;
            }

        }
    }
}