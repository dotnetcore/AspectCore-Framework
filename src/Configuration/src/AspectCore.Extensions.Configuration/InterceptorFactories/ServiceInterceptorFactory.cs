using System;
using System.Reflection;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.Configuration.InterceptorFactories
{
    public sealed class ServiceInterceptorFactory : InterceptorFactory
    {
        private readonly Type _interceptorType;

        public ServiceInterceptorFactory(Type interceptorType, params Func<MethodInfo, bool>[] predicates)
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

        public override IInterceptor CreateInstance(IServiceProvider serviceProvider)
        {
            return new ServiceInterceptorAttribute(_interceptorType);
        }
    }
}
