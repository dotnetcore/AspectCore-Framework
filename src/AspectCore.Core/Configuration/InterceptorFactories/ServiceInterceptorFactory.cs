using System;
using System.Reflection;
using AspectCore.DynamicProxy;

namespace AspectCore.Configuration
{
    /// <summary>
    /// 服务拦截器工厂
    /// </summary>
    public sealed class ServiceInterceptorFactory : InterceptorFactory
    {
        private readonly Type _interceptorType;

        // <summary>
        /// 此工厂创建的拦截器,由DI激活使用
        /// </summary>
        /// <param name="interceptorType">拦截器类型</param>
        /// <param name="predicates">拦截条件数组（注：为CanCreated方法提供支持以判断是否可以创建此拦截器）</param>
        public ServiceInterceptorFactory(Type interceptorType, params AspectPredicate[] predicates)
            : base(predicates)
        {
            if (interceptorType == null)
            {
                throw new ArgumentNullException(nameof(interceptorType));
            }
            if (!typeof(IInterceptor).GetTypeInfo().IsAssignableFrom(interceptorType.GetTypeInfo()))
            {
                throw new ArgumentException($"{interceptorType} is not an interceptor type.", nameof(interceptorType));
            }
            _interceptorType = interceptorType;
        }

        /// <summary>
        /// 创建由DI激活使用的拦截器
        /// </summary>
        /// <param name="serviceProvider">服务提供者</param>
        /// <returns>拦截器</returns>
        public override IInterceptor CreateInstance(IServiceProvider serviceProvider)
        {
            return new ServiceInterceptorAttribute(_interceptorType);
        }
    }
}
