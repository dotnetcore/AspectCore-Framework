using System;
using System.Reflection;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.Configuration.InterceptorFactories
{
    public abstract class InterceptorFactory : IInterceptorFactory
    {
        private static readonly Func<MethodInfo, bool>[] EmptyPredicates = new Func<MethodInfo, bool>[0];
        private readonly Func<MethodInfo, bool>[] _predicates;

        public InterceptorFactory(params Func<MethodInfo, bool>[] predicates)
        {
            _predicates = predicates ?? EmptyPredicates;
        }

        public bool CanCreated(MethodInfo method)
        {
            if (_predicates.Length == 0)
            {
                return true;
            }
            foreach (var predicate in _predicates)
            {
                if (predicate(method)) return true;
            }
            return false;
        }

        public abstract IInterceptor CreateInstance(IServiceProvider serviceProvider);
    }
}
