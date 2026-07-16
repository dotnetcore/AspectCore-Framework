using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.DynamicProxy.ProxyBuilder;
using AspectCore.Extensions.Reflection;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public sealed class ProxyTypeGenerator : IProxyTypeGenerator
    {
        private readonly IAspectValidator _aspectValidator;
        private readonly ProxyTypeCompiler _proxyGeneratorUtils;

        public ProxyTypeGenerator(IAspectValidatorBuilder aspectValidatorBuilder)
        {
            if (aspectValidatorBuilder == null)
            {
                throw new ArgumentNullException(nameof(aspectValidatorBuilder));
            }
            _aspectValidator = aspectValidatorBuilder.Build();
            _proxyGeneratorUtils = new ProxyTypeCompiler();
        }

        public Type CreateClassProxyType(Type serviceType, Type implementationType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }
            RejectRefStruct(serviceType, nameof(serviceType));
            RejectRefStruct(implementationType, nameof(implementationType));
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

            RejectRefStruct(serviceType, nameof(serviceType));
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

            RejectRefStruct(serviceType, nameof(serviceType));
            RejectRefStruct(implementationType, nameof(implementationType));
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


        private static void RejectRefStruct(Type type, string paramName)
        {
            if (type == null)
            {
                return;
            }
            bool isRefStruct;
#if NETSTANDARD2_0
            isRefStruct = type.GetTypeInfo().GetCustomAttributes(false)
                .Any(a => a.GetType().FullName == "System.Runtime.CompilerServices.IsByRefLikeAttribute");
#else
            isRefStruct = type.IsByRefLike;
#endif
            if (isRefStruct)
            {
                throw new NotSupportedException(
                    $"Cannot create proxy for ref struct type '{type.FullName ?? type.Name}'. " +
                    "Ref struct types (such as Span<T> and ReadOnlySpan<T>) cannot be boxed, " +
                    "cannot implement interfaces, and cannot be stored as class fields, " +
                    "which makes AOP proxy generation fundamentally impossible. " +
                    "Use a regular struct or class instead.");
            }
        }

    }
}