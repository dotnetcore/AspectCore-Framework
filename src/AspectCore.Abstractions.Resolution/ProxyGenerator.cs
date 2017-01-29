using AspectCore.Abstractions.Resolution.Generators;
using System;
using System.Reflection;

namespace AspectCore.Abstractions.Resolution
{
    public sealed class ProxyGenerator : IProxyGenerator
    {
        private readonly IAspectValidator aspectValidator;

        public ProxyGenerator(IAspectValidator aspectValidator)
        {
            if (aspectValidator == null)
            {
                throw new ArgumentNullException(nameof(aspectValidator));
            }
            this.aspectValidator = aspectValidator;
        }

        public Type CreateType(Type serviceType, Type implementationType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }
            var proxyGenerator = GetProxyTypeGenerator(serviceType);
            return proxyGenerator.CreateTypeInfo().AsType();
        }

        private ProxyTypeGenerator GetProxyTypeGenerator(Type serviceType)
        {
            if (serviceType.GetTypeInfo().IsInterface)
            {
                return new InterfaceProxyTypeGenerator(serviceType, aspectValidator);
            }
            else
            {
                return new ClassProxyTypeGenerator(serviceType, aspectValidator);
            }
        }
    }
}
