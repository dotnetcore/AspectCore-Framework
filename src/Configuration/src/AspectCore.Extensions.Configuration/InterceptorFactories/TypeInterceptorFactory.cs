using System;
using System.Reflection;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.Configuration.InterceptorFactories
{
    public sealed class TypeInterceptorFactory : InterceptorFactory
    {
        private readonly static object[] emptyArgs = new object[0];
        private readonly object[] _args;
        private readonly Type _interceptorType;

        public TypeInterceptorFactory(Type interceptorType, object[] args, params Func<MethodInfo, bool>[] predicates)
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

        public override IInterceptor CreateInstance(IServiceProvider serviceProvider)
        {
            var activator = (IInterceptorActivator)serviceProvider.GetService(typeof(IInterceptorActivator));
            return activator.CreateInstance(_interceptorType, _args);
        }
    }
}