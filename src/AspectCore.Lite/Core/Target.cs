using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Lite.Core
{
    public sealed class Target
    {
        public MethodInfo Method { get; }
        public Type ServiceType { get; }
        public Type ImplementationType { get; }

        internal Target(MethodInfo method, Type serviceType, Type implementationType)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            if (implementationType == null)
                throw new ArgumentNullException(nameof(implementationType));

            Method = method;
            ServiceType = serviceType;
            ImplementationType = implementationType;
        }
    }
}
