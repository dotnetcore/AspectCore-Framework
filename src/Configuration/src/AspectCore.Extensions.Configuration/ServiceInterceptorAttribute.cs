using System;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.Abstractions;
using AspectCore.Extensions.Configuration.InterceptorFactories;

namespace AspectCore.Extensions.Configuration
{
    public sealed class ServiceInterceptorAttribute : InterceptorAttribute
    {
        const string Wildcard = "*";

        private readonly Type interceptorType;

        public override bool AllowMultiple { get; } = true;

        public string Service { get; set; }

        public string Method { get; set; }

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

            this.interceptorType = interceptorType;
        }

        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            var factory = new ServiceInterceptorFactory(interceptorType, new Func<MethodInfo, bool>[] { Predicates.ForMethod(Service ?? Wildcard, Method ?? Wildcard) });

            if (factory.CanCreated(context.Target.ServiceMethod))
            {
                var instance = factory.CreateInstance(context.ServiceProvider);
                return instance.Invoke(context, next);
            }

            return next(context);
        }
    }
}