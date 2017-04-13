using System;
using System.Reflection;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    public class TypedInterceptorFactory : IInterceptorFactory
    {
        public object[] Args { get; }

        public Type InterceptorType { get; }

        public Predicate<MethodInfo> Predicate { get; }

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
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }
            Predicate = predicate;
            InterceptorType = interceptorType;
            Args = args;
        }

        public IInterceptor CreateInstance(IServiceProvider serviceProvider)
        {
            var activator = (ITypedInterceptorActivator)serviceProvider.GetService(typeof(ITypedInterceptorActivator));
            return activator.CreateInstance(InterceptorType, Args);
        }
    }
}
