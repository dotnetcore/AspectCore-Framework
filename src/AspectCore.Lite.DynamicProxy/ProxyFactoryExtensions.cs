using System;
using System.Linq;

namespace AspectCore.Lite.DynamicProxy
{
    public static class ProxyFactoryExtensions
    {
        public static object CreateProxy(this IProxyFactory factory, Type serviceType, object implementationInstance, params object[] args)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            if (implementationInstance == null)
            {
                throw new ArgumentNullException(nameof(implementationInstance));
            }

            return factory.CreateProxy(serviceType, implementationInstance.GetType(), implementationInstance, args);
        }

        public static object CreateProxy(this IProxyFactory factory, object implementationInstance, params object[] args)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            if (implementationInstance == null)
            {
                throw new ArgumentNullException(nameof(implementationInstance));
            }

            return factory.CreateProxy(implementationInstance.GetType(), implementationInstance, args);
        }

        public static TService CreateProxy<TService, TImplementation>(this IProxyFactory factory, TImplementation implementationInstance, params object[] args)
            where TImplementation : TService
        {
            return (TService)factory.CreateProxy(typeof(TService), typeof(TImplementation), implementationInstance, args);
        }

        public static TImplementation CreateProxy<TImplementation>(this IProxyFactory factory, TImplementation implementationInstance, params object[] args)
        {
            return (TImplementation)factory.CreateProxy(typeof(TImplementation), typeof(TImplementation), implementationInstance, args);
        }

        public static object CreateProxy(this IProxyFactory factory, Type serviceType, Type implementationType, params object[] args)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            if (implementationType == null)
            {
                throw new ArgumentNullException(nameof(implementationType));
            }

            var implementationInstance = Activator.CreateInstance(implementationType, args);

            return factory.CreateProxy(serviceType, implementationType, implementationInstance, args);
        }

        public static object CreateProxy(this IProxyFactory factory, Type implementationType, params object[] args)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            return CreateProxy(factory, implementationType, implementationType, args);
        }

        public static TService CreateProxy<TService, TImplementation>(this IProxyFactory factory, params object[] args)
            where TImplementation : TService
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            return (TService)CreateProxy(factory, typeof(TService), typeof(TImplementation), args);
        }

        public static TImplementation CreateProxy<TImplementation>(this IProxyFactory factory, params object[] args)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            return (TImplementation)CreateProxy(factory, typeof(TImplementation), typeof(TImplementation), args);
        }
    }
}
