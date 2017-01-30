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

        public Type CreateClassProxyType(Type serviceType, Type implementationType, params Type[] interfaces)
        {
            return new ClassProxyTypeGenerator(serviceType, implementationType, interfaces, aspectValidator).CreateTypeInfo().AsType();
        }

        public Type CreateInterfaceProxyType(Type serviceType, Type implementationType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            return GetInterfaceProxyTypeGenerator(serviceType, implementationType).CreateTypeInfo().AsType();
        }

        private ProxyTypeGenerator GetInterfaceProxyTypeGenerator(Type serviceType, Type implementationType)
        {
            var proxyStructureAttribute = serviceType.GetTypeInfo().GetCustomAttribute<ProxyStructureAttribute>();
            if (proxyStructureAttribute != null && proxyStructureAttribute.ProxyMode == ProxyMode.Inheritance)
            {
                return new InheritanceInterfaceProxyTypeGenerator(serviceType, implementationType, aspectValidator);
            }
            return new InterfaceProxyTypeGenerator(serviceType, aspectValidator);
        }
    }
}
