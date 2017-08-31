using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.Abstractions;

namespace AspectCore.Core.Injector
{
    internal class ServiceCallSiteCompiler
    {
        internal Func<IServiceResolver, object> Invoke(ServiceDefinition service)
        {
            if (service is InstanceServiceDefinition instanceServiceDefinition)
            {
                return new Func<IServiceResolver, object>(resolver => instanceServiceDefinition.ImplementationInstance);
            }
            else if (service is DelegateServiceDefinition delegateServiceDefinition)
            {
                return delegateServiceDefinition.ImplementationDelegate;
            }

        }
    }
}
