using System;
using System.Diagnostics.CodeAnalysis;
using AspectCore.DependencyInjection;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// Provides utility methods for creating proxy instances with runtime constructor arguments,
    /// similar to ActivatorUtilities.CreateInstance but with AOP interception support.
    /// </summary>
    public static class ProxyActivatorUtilities
    {
        /// <summary>
        /// Creates a proxy instance of <typeparamref name="T"/> using the specified constructor arguments.
        /// The returned instance will have interceptors applied according to the configured aspects.
        /// </summary>
        /// <typeparam name="T">The service type to proxy. Must be a class with virtual members.</typeparam>
        /// <param name="serviceProvider">The service provider used to resolve the proxy generator.</param>
        /// <param name="parameters">Constructor arguments to pass to the proxy instance.</param>
        /// <returns>A proxy instance of <typeparamref name="T"/> with interception enabled.</returns>
        [RequiresDynamicCode("Creates proxy type instances via reflection. Use source-generated proxies for AOT compatibility.")]
        public static T CreateProxyInstance<T>(IServiceProvider serviceProvider, params object[] parameters) where T : class
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var proxyGenerator = serviceProvider.ResolveRequired<IProxyGenerator>();
            return (T)proxyGenerator.CreateClassProxy(typeof(T), typeof(T), parameters);
        }

        /// <summary>
        /// Creates a proxy instance of the specified <paramref name="serviceType"/> using the specified constructor arguments.
        /// The returned instance will have interceptors applied according to the configured aspects.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve the proxy generator.</param>
        /// <param name="serviceType">The service type to proxy. Must be a class with virtual members.</param>
        /// <param name="parameters">Constructor arguments to pass to the proxy instance.</param>
        /// <returns>A proxy instance of <paramref name="serviceType"/> with interception enabled.</returns>
        [RequiresDynamicCode("Creates proxy type instances via reflection. Use source-generated proxies for AOT compatibility.")]
        public static object CreateProxyInstance(IServiceProvider serviceProvider, Type serviceType, params object[] parameters)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var proxyGenerator = serviceProvider.ResolveRequired<IProxyGenerator>();
            return proxyGenerator.CreateClassProxy(serviceType, serviceType, parameters);
        }
    }

    /// <summary>
    /// Extension methods for <see cref="IServiceProvider"/> to create proxy instances with runtime arguments.
    /// </summary>
    public static class ProxyActivatorServiceProviderExtensions
    {
        /// <summary>
        /// Creates a proxy instance of <typeparamref name="T"/> using the specified constructor arguments.
        /// </summary>
        [RequiresDynamicCode("Creates proxy type instances via reflection. Use source-generated proxies for AOT compatibility.")]
        public static T CreateProxyInstance<T>(this IServiceProvider serviceProvider, params object[] parameters) where T : class
        {
            return ProxyActivatorUtilities.CreateProxyInstance<T>(serviceProvider, parameters);
        }

        /// <summary>
        /// Creates a proxy instance of the specified <paramref name="serviceType"/> using the specified constructor arguments.
        /// </summary>
        [RequiresDynamicCode("Creates proxy type instances via reflection. Use source-generated proxies for AOT compatibility.")]
        public static object CreateProxyInstance(this IServiceProvider serviceProvider, Type serviceType, params object[] parameters)
        {
            return ProxyActivatorUtilities.CreateProxyInstance(serviceProvider, serviceType, parameters);
        }
    }
}
