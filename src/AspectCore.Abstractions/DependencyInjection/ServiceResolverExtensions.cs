using System;
using System.Collections.Generic;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 扩展IServiceResolver接口
    /// </summary>
    public static class ServiceResolverExtensions
    {
        /// <summary>
        /// 从容器中获取类型T的一个服务，提供多种解析获取服务的方法
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <param name="serviceResolver">服务提供者</param>
        /// <returns>获取到的服务</returns>
        public static T Resolve<T>(this IServiceResolver serviceResolver)
        {
            if (serviceResolver == null)
            {
                throw new ArgumentNullException(nameof(serviceResolver));
            }
            return (T)serviceResolver.Resolve(typeof(T));
        }

        /// <summary>
        /// 创建一个作用域
        /// </summary>
        /// <param name="serviceResolver">服务提供者</param>
        /// <returns>此作用域下的服务提供者</returns>
        public static IServiceResolver CreateScope(this IServiceResolver serviceResolver)
        {
            if (serviceResolver == null)
            {
                throw new ArgumentNullException(nameof(serviceResolver));
            }
            var factory = serviceResolver.Resolve<IScopeResolverFactory>();
            return factory.CreateScope();
        }

        /// <summary>
        /// 从容器中获取类型serviceType的一个服务,如果不存在此服务，则引发异常
        /// </summary>
        /// <param name="serviceResolver">服务提供者</param>
        /// <param name="serviceType">服务的类型</param>
        /// <returns>获取到的服务</returns>
        public static object ResolveRequired(this IServiceResolver serviceResolver, Type serviceType)
        {
            if (serviceResolver == null)
            {
                throw new ArgumentNullException(nameof(serviceResolver));
            }
            var instance = serviceResolver.Resolve(serviceType);
            if (instance == null)
            {
                throw new InvalidOperationException($"No instance for type '{serviceType}' has been resolved.");
            }
            return instance;
        }

        /// <summary>
        /// 从容器中获取类型T的一个服务,如果不存在此服务，则引发异常
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <param name="serviceResolver">服务提供者</param>
        /// <returns>获取到的服务</returns>
        public static T ResolveRequired<T>(this IServiceResolver serviceResolver)
        {
            if (serviceResolver == null)
            {
                throw new ArgumentNullException(nameof(serviceResolver));
            }
            return (T)serviceResolver.ResolveRequired(typeof(T));
        }

        /// <summary>
        /// 从容器中获取类型serviceType的多个服务
        /// </summary>
        /// <param name="serviceResolver">服务提供者</param>
        /// <param name="serviceType">服务类型</param>
        /// <returns>获取到的多个服务</returns>
        public static IEnumerable<object> ResolveMany(this IServiceResolver serviceResolver, Type serviceType)
        {
            if (serviceResolver == null)
            {
                throw new ArgumentNullException(nameof(serviceResolver));
            }

            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            var genericEnumerable = typeof(IManyEnumerable<>).MakeGenericType(serviceType);
            return (IManyEnumerable<object>)serviceResolver.ResolveRequired(genericEnumerable);
        }

        /// <summary>
        /// 从容器中获取类型T的多个服务
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <param name="serviceResolver">服务提供者</param>
        /// <returns>获取到的多个服务</returns>
        public static IEnumerable<T> ResolveMany<T>(this IServiceResolver serviceResolver)
        {
            if (serviceResolver == null)
            {
                throw new ArgumentNullException(nameof(serviceResolver));
            }

            return serviceResolver.ResolveRequired<IManyEnumerable<T>>();
        }
    }
}