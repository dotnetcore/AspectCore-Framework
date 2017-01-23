using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Abstractions.Resolution
{
    public sealed class DefaultAspectActivatorContext : AspectActivatorContext
    {
        public override object[] Parameters { get; }

        public override object ProxyInstance { get; }

        public override MethodInfo ProxyMethod { get; }

        public override MethodInfo ServiceMethod { get; }

        public override Type ServiceType { get; }

        public override object TargetInstance { get; }

        public override MethodInfo TargetMethod { get; }

        public DefaultAspectActivatorContext(Type serviceType, MethodInfo serviceMethod, MethodInfo targetMethod, MethodInfo proxyMethod, object targetInstance, object proxyInstance, object[] parameters)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }
            if (serviceMethod == null)
            {
                throw new ArgumentNullException(nameof(serviceMethod));
            }
            if (targetMethod == null)
            {
                throw new ArgumentNullException(nameof(targetMethod));
            }
            if (proxyMethod == null)
            {
                throw new ArgumentNullException(nameof(proxyMethod));
            }
            if (targetInstance == null)
            {
                throw new ArgumentNullException(nameof(targetInstance));
            }
            if (proxyInstance == null)
            {
                throw new ArgumentNullException(nameof(proxyInstance));
            }
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            ServiceType = serviceType;
            ServiceMethod = serviceMethod;
            TargetMethod = targetMethod;
            ProxyMethod = proxyMethod;
            TargetInstance = targetInstance;
            ProxyInstance = proxyInstance;
            Parameters = parameters;
        }
    }
}
