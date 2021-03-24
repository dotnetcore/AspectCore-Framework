using System;
using System.Reflection;
using AspectCore.DynamicProxy;

namespace AspectCore.Configuration
{
    /// <summary>
    /// 类型拦截器工厂
    /// </summary>
    public sealed class TypeInterceptorFactory : InterceptorFactory
    {
        private readonly static object[] emptyArgs = new object[0];
        private readonly object[] _args;
        private readonly Type _interceptorType;

        /// <summary>
        /// 构造类型拦截器工厂
        /// </summary>
        /// <param name="interceptorType">拦截器类型</param>
        /// <param name="args">拦截器的构造参数</param>
        /// <param name="predicates">拦截条件数组（注：为CanCreated方法提供支持以判断是否可以创建此拦截器）</param>
        public TypeInterceptorFactory(Type interceptorType, object[] args, params AspectPredicate[] predicates)
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
            _args = args ?? emptyArgs;
        }

        /// <summary>
        /// 创建一个拦截器
        /// </summary>
        /// <param name="serviceProvider">服务提供者</param>
        /// <returns>创建的拦截器</returns>
        public override IInterceptor CreateInstance(IServiceProvider serviceProvider)
        {
            return (IInterceptor)Activator.CreateInstance(_interceptorType, _args);
        }
    }
}