using System;
using System.Reflection;

namespace AspectCore.Abstractions
{
    public class TypedInterceptorFactory : IInterceptorFactory
    {
        public object[] Args { get; }

        public Type InterceptorType { get; }

        public Predicate<MethodInfo> Predicate { get; }

        public TypedInterceptorFactory(Type interceptorType)
            : this(m => true, interceptorType,null)
        {
        }

        public TypedInterceptorFactory(Type interceptorType, object[] args)
            : this(m => true, interceptorType, args)
        {
        }

        public TypedInterceptorFactory(Predicate<MethodInfo> predicate, Type interceptorType)
          : this(predicate, interceptorType, null)
        {
        }

        public TypedInterceptorFactory(Predicate<MethodInfo> predicate, Type interceptorType, object[] args)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            if (interceptorType == null)
            {
                throw new ArgumentNullException(nameof(interceptorType));
            }
            if (!typeof(IInterceptor).GetTypeInfo().IsAssignableFrom(interceptorType.GetTypeInfo()))
            {
                throw new ArgumentException($"{interceptorType} is not an interceptor type.", nameof(interceptorType));
            }
            Predicate = predicate;
            InterceptorType = interceptorType;
            Args = args ?? new object[0];
        }

        public IInterceptor CreateInstance(IServiceProvider serviceProvider)
        {
            var activator = (ITypedInterceptorActivator)serviceProvider.GetService(typeof(ITypedInterceptorActivator));
            return activator.CreateInstance(InterceptorType, Args);
        }
    }
}
