using System;
using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.Abstractions
{
    public class TypedInterceptorFactory : IInterceptorFactory
    {
        private static readonly Predicate<MethodInfo>[] Empty = new Predicate<MethodInfo>[0];

        private readonly ICollection<Predicate<MethodInfo>> _predicates;

        public object[] Args { get; }

        public Type InterceptorType { get; }

        public TypedInterceptorFactory(Type interceptorType)
            : this(interceptorType, null, Empty)
        {
        }

        public TypedInterceptorFactory(Type interceptorType, object[] args)
            : this(interceptorType, args, Empty)
        {
        }

        public TypedInterceptorFactory(Type interceptorType, Predicate<MethodInfo>[] predicates)
          : this(interceptorType, null, predicates)
        {
        }

        public TypedInterceptorFactory(Type interceptorType, object[] args, Predicate<MethodInfo>[] predicates)
        {
            if (predicates == null)
            {
                throw new ArgumentNullException(nameof(predicates));
            }
            if (interceptorType == null)
            {
                throw new ArgumentNullException(nameof(interceptorType));
            }
            if (!typeof(IInterceptor).GetTypeInfo().IsAssignableFrom(interceptorType.GetTypeInfo()))
            {
                throw new ArgumentException($"{interceptorType} is not an interceptor type.", nameof(interceptorType));
            }
            InterceptorType = interceptorType;
            Args = args ?? new object[0];
            _predicates = predicates;
        }

        public IInterceptor CreateInstance(IServiceProvider serviceProvider)
        {
            var activator = (ITypedInterceptorActivator)serviceProvider.GetService(typeof(ITypedInterceptorActivator));
            return activator.CreateInstance(InterceptorType, Args);
        }

        public bool CanCreated(MethodInfo method)
        {
            foreach (var predicate in _predicates)
            {
                if (predicate(method)) return true;
            }
            return false;
        }
    }
}