using System;
using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public class TypedInterceptorFactory : IInterceptorFactory
    {
        private static readonly Predicate<MethodInfo>[] EmptyPredicates = new Predicate<MethodInfo>[0];
        private static readonly object[] EmptyArgs = new object[0];

        private readonly ICollection<Predicate<MethodInfo>> _predicates;

        public object[] Args { get; }

        public Type InterceptorType { get; }

        public TypedInterceptorFactory(Type interceptorType)
            : this(interceptorType, null, EmptyPredicates)
        {
        }

        public TypedInterceptorFactory(Type interceptorType, object[] args)
            : this(interceptorType, args, EmptyPredicates)
        {
        }

        public TypedInterceptorFactory(Type interceptorType, Predicate<MethodInfo>[] predicates)
          : this(interceptorType, EmptyArgs, predicates)
        {
        }

        public TypedInterceptorFactory(Type interceptorType, object[] args, Predicate<MethodInfo>[] predicates)
        {
            if (interceptorType == null)
            {
                throw new ArgumentNullException(nameof(interceptorType));
            }
            if (!typeof(IInterceptor).GetTypeInfo().IsAssignableFrom(interceptorType.GetTypeInfo()))
            {
                throw new ArgumentException($"{interceptorType} is not an interceptor type.", nameof(interceptorType));
            }
            InterceptorType = interceptorType;
            Args = args ?? EmptyArgs;
            _predicates = predicates ?? EmptyPredicates;
        }

        public virtual IInterceptor CreateInstance(IServiceProvider serviceProvider)
        {
            var activator = (ITypedInterceptorActivator)serviceProvider.GetService(typeof(ITypedInterceptorActivator));
            return activator.CreateInstance(InterceptorType, Args);
        }

        public virtual bool CanCreated(MethodInfo method)
        {
            if (_predicates.Count == 0)
            {
                return true;
            }
            foreach (var predicate in _predicates)
            {
                if (predicate(method)) return true;
            }
            return false;
        }
    }
}