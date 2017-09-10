using System;
using AspectCore.Utils;

namespace AspectCore.DynamicProxy
{
    public static class ProxyGeneratorExtensions
    {
        public static object CreateClassProxy(this IProxyGenerator proxyGenerator, Type serviceType, Type implementationType)
        {
            if (proxyGenerator == null)
            {
                throw new ArgumentNullException(nameof(proxyGenerator));
            }
            return proxyGenerator.CreateClassProxy(serviceType, implementationType, ArrayUtils.Empty<object>());
        }

        public static object CreateClassProxy(this IProxyGenerator proxyGenerator, Type implementationType, params object[] args)
        {
            if (proxyGenerator == null)
            {
                throw new ArgumentNullException(nameof(proxyGenerator));
            }
            return proxyGenerator.CreateClassProxy(implementationType, implementationType, args ?? ArrayUtils.Empty<object>());
        }

        public static TService CreateClassProxy<TService, TImplementation>(this IProxyGenerator proxyGenerator, params object[] args)
            where TService : class
            where TImplementation : TService
        {
            if (proxyGenerator == null)
            {
                throw new ArgumentNullException(nameof(ProxyTypeGenerator));
            }
            return (TService)proxyGenerator.CreateClassProxy(typeof(TService), typeof(TImplementation), args ?? ArrayUtils.Empty<object>());
        }

        public static TImplementation CreateClassProxy<TImplementation>(this IProxyGenerator proxyGenerator, params object[] args)
             where TImplementation : class
        {
            if (proxyGenerator == null)
            {
                throw new ArgumentNullException(nameof(proxyGenerator));
            }
            return (TImplementation)proxyGenerator.CreateClassProxy(typeof(TImplementation), typeof(TImplementation), args ?? ArrayUtils.Empty<object>());
        }

        public static TService CreateInterfaceProxy<TService>(this IProxyGenerator proxyGenerator)
            where TService : class
        {
            if (proxyGenerator == null)
            {
                throw new ArgumentNullException(nameof(proxyGenerator));
            }
            return (TService)proxyGenerator.CreateInterfaceProxy(typeof(TService));
        }

        public static TService CreateInterfaceProxy<TService>(this IProxyGenerator proxyGenerator, TService implementationInstance)
           where TService : class
        {
            if (proxyGenerator == null)
            {
                throw new ArgumentNullException(nameof(proxyGenerator));
            }
            return (TService)proxyGenerator.CreateInterfaceProxy(typeof(TService), implementationInstance);
        }

        public static object CreateInterfaceProxy(this IProxyGenerator proxyGenerator, Type serviceType, Type implementationType, params object[] args)
        {
            if (proxyGenerator == null)
            {
                throw new ArgumentNullException(nameof(proxyGenerator));
            }
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }
            if (implementationType == null)
            {
                throw new ArgumentNullException(nameof(implementationType));
            }
            return proxyGenerator.CreateInterfaceProxy(serviceType, Activator.CreateInstance(implementationType, args ?? ArrayUtils.Empty<object>()));
        }

        public static TService CreateInterfaceProxy<TService, TImplementation>(this IProxyGenerator proxyGenerator, params object[] args)
          where TService : class
          where TImplementation : TService
        {
            return (TService)CreateInterfaceProxy(proxyGenerator, typeof(TService), typeof(TImplementation), args);
        }
    }
}