using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Abstractions;
using AspectCore.Core.Internal;
using AspectCore.Core.Internal.Generator;

namespace AspectCore.Core
{
    [NonAspect]
    public sealed class ProxyGenerator : IProxyGenerator
    {
        private readonly IAspectValidator _aspectValidator;

        public ProxyGenerator(IAspectValidatorBuilder aspectValidatorBuilder)
        {
            if (aspectValidatorBuilder == null)
            {
                throw new ArgumentNullException(nameof(aspectValidatorBuilder));
            }
            _aspectValidator = aspectValidatorBuilder.Build();
        }

        public Type CreateClassProxyType(Type serviceType, Type implementationType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }
            if (!serviceType.GetTypeInfo().IsClass)
            {
                throw new ArgumentException($"Type '{serviceType}' should be class.", nameof(serviceType));
            }
            return new ClassProxyTypeGenerator(serviceType, implementationType, implementationType.GetTypeInfo().GetInterfaces(), _aspectValidator).CreateTypeInfo().AsType();
        }

        public Type CreateInterfaceProxyType(Type serviceType, Type implementationType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (!serviceType.GetTypeInfo().IsInterface)
            {
                throw new ArgumentException($"Type '{serviceType}' should be interface.", nameof(serviceType));
            }
            return ProxyGeneratorHelpers.CreateInterfaceProxy(serviceType, implementationType, GetInterfaces(implementationType, serviceType).ToArray(), _aspectValidator);
            //return new InterfaceProxyTypeGenerator(serviceType, implementationType, serviceType.GetTypeInfo().GetInterfaces(), _aspectValidator).CreateTypeInfo().AsType();
        }

        private IEnumerable<Type> GetInterfaces(Type type, params Type[] exceptInterfaces)
        {
            var hashSet = new HashSet<Type>(exceptInterfaces);
            foreach (var interfaceType in type.GetTypeInfo().GetInterfaces())
            {
                if (!hashSet.Contains(interfaceType))
                    yield return interfaceType;
            }
        }
    }
}