using System;
using System.Linq;
using System.Reflection;
using AspectCore.Lite.Abstractions.Descriptors;

namespace AspectCore.Lite.Abstractions
{
    public sealed class Target : IMethodInvoker
    {
        internal ParameterCollection ParameterCollection { get; set; }
        public MethodInfo Method { get; }
        public Type ServiceType { get; }
        public Type ImplementationType { get; }
        public object Instance { get; }

        internal Target(MethodInfo method, Type serviceType, Type implementationType, object implementationInstance)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            if (implementationType == null)
                throw new ArgumentNullException(nameof(implementationType));

            if (implementationInstance == null)
                throw new ArgumentNullException(nameof(implementationInstance));

            Method = method;
            ServiceType = serviceType;
            ImplementationType = implementationType;
            Instance = implementationInstance;
        }

        public object Invoke()
        {
            object[] args = ParameterCollection?.Select(p => p.Value)?.ToArray();
            return Method.Invoke(Instance, args);
        }
    }
}