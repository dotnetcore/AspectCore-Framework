using System;
using System.Reflection;
using System.Runtime;
using System.Threading.Tasks;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 需要被代理，并通过此特性指定拦截器类型,则拦截器实例将从容器中获取
    /// </summary>
    public sealed class ServiceInterceptorAttribute : AbstractInterceptorAttribute, IEquatable<ServiceInterceptorAttribute>
    {
        /// <summary>
        /// 拦截器类型
        /// </summary>
        private readonly Type _interceptorType;

        /// <summary>
        /// 是否多用
        /// </summary>
        public override bool AllowMultiple { get; } = true;

        /// <summary>
        /// 需要被代理，并通过此特性指定拦截器类型,则拦截器实例将从容器中获取
        /// </summary>
        /// <param name="interceptorType">拦截器类型</param>
        public ServiceInterceptorAttribute(Type interceptorType)
        {
            if (interceptorType == null)
            {
                throw new ArgumentNullException(nameof(interceptorType));
            }

            if (!typeof(IInterceptor).GetTypeInfo().IsAssignableFrom(interceptorType.GetTypeInfo()))
            {
                throw new ArgumentException($"{interceptorType} is not an interceptor.", nameof(interceptorType));
            }

            _interceptorType = interceptorType;
        }

        /// <summary>
        /// 从容器中获取拦截器，并执行增强逻辑
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="next">处理AspectContext上下文的委托</param>
        /// <returns>此拦截器所代表的异步任务</returns>
        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            var instance = context.ServiceProvider.GetService(_interceptorType) as IInterceptor;
            if (instance == null)
            {
                throw new InvalidOperationException($"Cannot resolve type  '{_interceptorType}' of service interceptor.");
            }

            return instance.Invoke(context, next);
        }

        public bool Equals(ServiceInterceptorAttribute other)
        {
            if (other == null)
            {
                return false;
            }
            return _interceptorType == other._interceptorType;
        }
        
        public override bool Equals(object obj)
        {
            var other = obj as ServiceInterceptorAttribute;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return _interceptorType.GetHashCode();
        }
    }
}