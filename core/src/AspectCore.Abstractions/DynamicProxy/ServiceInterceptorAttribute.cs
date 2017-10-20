using System;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.DynamicProxy
{
    public sealed class ServiceInterceptorAttribute : AbstractInterceptorAttribute
    {
        private readonly Type _interceptorType;

        public override bool AllowMultiple { get; } = true;

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

        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            var instance = context.ServiceProvider.GetService(_interceptorType) as IInterceptor;
            if (instance == null)
            {
                throw new InvalidOperationException($"Cannot resolve type  '{_interceptorType}' of service interceptor.");
            }
            return instance.Invoke(context, next);
        }
    }
}