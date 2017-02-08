using System;
using System.Reflection;
using AspectCore.Abstractions.Internal.Generator;

namespace AspectCore.Abstractions.Internal
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
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }
            if (!serviceType.GetTypeInfo().IsClass)
            {
                throw new ArgumentException($"Type '{serviceType}' should be class.", nameof(serviceType));
            }
            return new ClassProxyTypeGenerator(serviceType, implementationType, interfaces, aspectValidator).CreateTypeInfo().AsType();
        }

        public Type CreateInterfaceProxyType(Type serviceType, Type implementationType, params Type[] interfaces)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            return GetInterfaceProxyTypeGenerator(serviceType, implementationType).CreateTypeInfo().AsType();
        }

        private ProxyTypeGenerator GetInterfaceProxyTypeGenerator(Type serviceType, Type implementationType, params Type[] interfaces)
        {
            var proxyStructureAttribute = serviceType.GetTypeInfo().GetCustomAttribute<ProxyStructureAttribute>();
            if (proxyStructureAttribute != null && proxyStructureAttribute.ProxyMode == ProxyMode.Inheritance)
            {
                return new InheritanceInterfaceProxyTypeGenerator(serviceType, implementationType, interfaces, aspectValidator);
            }
            return new InterfaceProxyTypeGenerator(serviceType, implementationType, interfaces, aspectValidator);
        }
    }
}
