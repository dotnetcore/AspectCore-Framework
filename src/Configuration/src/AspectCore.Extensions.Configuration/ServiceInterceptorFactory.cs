using System;
using System.Collections.Generic;
using System.Reflection;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.Configuration
{
    public sealed class ServiceInterceptorFactory : IInterceptorFactory
    {
        private static readonly Predicate<MethodInfo>[] EmptyPredicates = new Predicate<MethodInfo>[0];
        private readonly ICollection<Predicate<MethodInfo>> _predicates;

        public Type InterceptorType { get; }

        public ServiceInterceptorFactory(Type interceptorType)
            : this(interceptorType, EmptyPredicates)
        {
        }

        public ServiceInterceptorFactory(Type interceptorType, Predicate<MethodInfo>[] predicates)
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
            _predicates = predicates ?? EmptyPredicates;
        }

        public IInterceptor CreateInstance(IServiceProvider serviceProvider)
        {
            var interceptor = serviceProvider.GetService(InterceptorType) as IInterceptor;
            if (interceptor == null)
            {
                throw new InvalidOperationException($"The interceptor of {InterceptorType} is not registered.");
            }
            return interceptor;
        }

        public bool CanCreated(MethodInfo method)
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