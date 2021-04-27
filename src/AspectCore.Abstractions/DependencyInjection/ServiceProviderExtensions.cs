using System;
using System.Collections.Generic;
using System.Text;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 提供一些IServiceProvider接口的扩展方法
    /// </summary>
    public static class ServiceProviderExtensions
    {
        /// <summary>
        /// 从容器中获取类型T的一个服务
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <param name="serviceProvider">服务提供者</param>
        /// <returns>获取到的服务</returns>
        public static T Resolve<T>(this IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }
            if(serviceProvider is IServiceResolver resolver)
            {
                return resolver.Resolve<T>();
            }
            resolver = serviceProvider.GetService(typeof(IServiceResolver)) as IServiceResolver;
            return resolver.Resolve<T>();
        }

        /// <summary>
        /// 从容器中获取类型T的一个服务,如果不存在此服务，则引发异常
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <param name="serviceProvider">服务提供者</param>
        /// <returns>获取到的服务</returns>
        public static T ResolveRequired<T>(this IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }
            if (serviceProvider is IServiceResolver resolver)
            {
                return resolver.ResolveRequired<T>();
            }
            resolver = serviceProvider.GetService(typeof(IServiceResolver)) as IServiceResolver;
            return resolver.ResolveRequired<T>();
        }

        /// <summary>
        /// 从容器中获取类型T的多个服务
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <param name="serviceProvider">服务提供者</param>
        /// <returns>获取到的多个服务</returns>
        public static IEnumerable<T> ResolveMany<T>(this IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }
            if (serviceProvider is IServiceResolver resolver)
            {
                return resolver.ResolveMany<T>();
            }
            resolver = serviceProvider.GetService(typeof(IServiceResolver)) as IServiceResolver;
            return resolver.ResolveMany<T>();
        }


    }
}
