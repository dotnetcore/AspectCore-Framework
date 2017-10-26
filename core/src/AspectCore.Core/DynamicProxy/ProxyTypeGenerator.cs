using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Utils;
using AspectCore.Extensions.Reflection;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public sealed class ProxyTypeGenerator : IProxyTypeGenerator
    {
        private readonly IAspectValidator _aspectValidator;
        private readonly ProxyGeneratorUtils _proxyGeneratorUtils;

        public ProxyTypeGenerator(IAspectValidatorBuilder aspectValidatorBuilder)
        {
            if (aspectValidatorBuilder == null)
            {
                throw new ArgumentNullException(nameof(aspectValidatorBuilder));
            }
            _aspectValidator = aspectValidatorBuilder.Build();
            _proxyGeneratorUtils = new ProxyGeneratorUtils();
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
            return _proxyGeneratorUtils.CreateClassProxy(serviceType, implementationType, GetInterfaces(implementationType).ToArray(), _aspectValidator);
        }

        public Type CreateInterfaceProxyType(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (!serviceType.GetTypeInfo().IsInterface)
            {
                throw new ArgumentException($"Type '{serviceType}' should be interface.", nameof(serviceType));
            }
            return _proxyGeneratorUtils.CreateInterfaceProxy(serviceType, GetInterfaces(serviceType, serviceType).ToArray(), _aspectValidator);
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
            return _proxyGeneratorUtils.CreateInterfaceProxy(serviceType, implementationType, GetInterfaces(implementationType, serviceType).ToArray(), _aspectValidator);
        }

        private IEnumerable<Type> GetInterfaces(Type type, params Type[] exceptInterfaces)
        {
            var hashSet = new HashSet<Type>(exceptInterfaces);
            foreach (var interfaceType in type.GetTypeInfo().GetInterfaces().Distinct())
            {
                if (!interfaceType.GetTypeInfo().IsVisible())
                {
                    continue;
                }
                if (!hashSet.Contains(interfaceType))
                {      
                    if (interfaceType.GetTypeInfo().ContainsGenericParameters && type.GetTypeInfo().ContainsGenericParameters)
                    {
                        if (!hashSet.Contains(interfaceType.GetGenericTypeDefinition()))
                            yield return interfaceType;
                    }
                    else
                    {
                        yield return interfaceType;
                    }
                }
            }
        }

    }
}