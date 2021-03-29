using System;
using AspectCore.Utils;

namespace AspectCore.DynamicProxy
{
    public static class ProxyGeneratorExtensions
    {
        /// <summary>
        /// 以子类代理方式创建代理对象
        /// </summary>
        /// <param name="proxyGenerator">代理生成器</param>
        /// <param name="serviceType">服务类型</param>
        /// <param name="implementationType">实现类型</param>
        /// <returns>代理对象</returns>
        public static object CreateClassProxy(this IProxyGenerator proxyGenerator, Type serviceType, Type implementationType)
        {
            if (proxyGenerator == null)
            {
                throw new ArgumentNullException(nameof(proxyGenerator));
            }
            return proxyGenerator.CreateClassProxy(serviceType, implementationType, ArrayUtils.Empty<object>());
        }

        /// <summary>
        /// 以子类代理方式创建代理对象
        /// </summary>
        /// <param name="proxyGenerator">代理生成器</param>
        /// <param name="implementationType">实现类型</param>
        /// <param name="args">构造参数</param>
        /// <returns>代理对象</returns>
        public static object CreateClassProxy(this IProxyGenerator proxyGenerator, Type implementationType, params object[] args)
        {
            if (proxyGenerator == null)
            {
                throw new ArgumentNullException(nameof(proxyGenerator));
            }
            return proxyGenerator.CreateClassProxy(implementationType, implementationType, args ?? ArrayUtils.Empty<object>());
        }

        /// <summary>
        /// 以子类代理方式创建代理对象
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <typeparam name="TImplementation">实现类型</typeparam>
        /// <param name="proxyGenerator">代理生成器</param>
        /// <param name="args">构造参数</param>
        /// <returns>代理</returns>
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

        /// <summary>
        /// 以子类代理方式创建代理对象
        /// </summary>
        /// <typeparam name="TImplementation">被代理的类型</typeparam>
        /// <param name="proxyGenerator">代理生成器</param>
        /// <param name="args">构造参数</param>
        /// <returns>代理</returns>
        public static TImplementation CreateClassProxy<TImplementation>(this IProxyGenerator proxyGenerator, params object[] args)
             where TImplementation : class
        {
            if (proxyGenerator == null)
            {
                throw new ArgumentNullException(nameof(proxyGenerator));
            }
            return (TImplementation)proxyGenerator.CreateClassProxy(typeof(TImplementation), typeof(TImplementation), args ?? ArrayUtils.Empty<object>());
        }

        /// <summary>
        /// 以接口代理方式创建代理对象
        /// </summary>
        /// <typeparam name="TService">服务接口</typeparam>
        /// <param name="proxyGenerator">代理生成器</param>
        /// <returns>代理</returns>
        public static TService CreateInterfaceProxy<TService>(this IProxyGenerator proxyGenerator)
            where TService : class
        {
            if (proxyGenerator == null)
            {
                throw new ArgumentNullException(nameof(proxyGenerator));
            }
            return (TService)proxyGenerator.CreateInterfaceProxy(typeof(TService));
        }

        /// <summary>
        /// 以接口代理方式创建代理对象
        /// </summary>
        /// <typeparam name="TService">服务接口</typeparam>
        /// <param name="proxyGenerator">代理生成器</param>
        /// <param name="implementationInstance">实现实例</param>
        /// <returns>代理</returns>
        public static TService CreateInterfaceProxy<TService>(this IProxyGenerator proxyGenerator, TService implementationInstance)
           where TService : class
        {
            if (proxyGenerator == null)
            {
                throw new ArgumentNullException(nameof(proxyGenerator));
            }
            return (TService)proxyGenerator.CreateInterfaceProxy(typeof(TService), implementationInstance);
        }

        /// <summary>
        ///  以接口代理方式创建代理对象
        /// </summary>
        /// <param name="proxyGenerator">代理生成器</param>
        /// <param name="serviceType">服务接口</param>
        /// <param name="implementationType">实现类型</param>
        /// <param name="args">构造参数</param>
        /// <returns>代理</returns>
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

        /// <summary>
        /// 以接口代理方式创建代理对象
        /// </summary>
        /// <typeparam name="TService">服务接口</typeparam>
        /// <typeparam name="TImplementation">实现类型</typeparam>
        /// <param name="proxyGenerator">代理生成器</param>
        /// <param name="args">构造参数</param>
        /// <returns>代理</returns>
        public static TService CreateInterfaceProxy<TService, TImplementation>(this IProxyGenerator proxyGenerator, params object[] args)
          where TService : class
          where TImplementation : TService
        {
            return (TService)CreateInterfaceProxy(proxyGenerator, typeof(TService), typeof(TImplementation), args);
        }
    }
}