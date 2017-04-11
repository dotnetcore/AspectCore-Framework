using System;
using System.Reflection;
using AspectCore.Abstractions.Internal.Generator;

namespace AspectCore.Abstractions.Internal
{
    public sealed class ProxyGenerator : IProxyGenerator
    {
        private readonly IAspectValidator _aspectValidator;

        public ProxyGenerator(IAspectValidator aspectValidator)
        {
            if (aspectValidator == null)
            {
                throw new ArgumentNullException(nameof(aspectValidator));
            }
            this._aspectValidator = aspectValidator;
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
            return new ClassProxyTypeGenerator(serviceType, implementationType, interfaces, _aspectValidator).CreateTypeInfo().AsType();
        }

        public Type CreateInterfaceProxyType(Type serviceType, Type implementationType, params Type[] interfaces)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            return new InterfaceProxyTypeGenerator(serviceType, implementationType, interfaces, _aspectValidator).CreateTypeInfo().AsType();
        }
    }
}
