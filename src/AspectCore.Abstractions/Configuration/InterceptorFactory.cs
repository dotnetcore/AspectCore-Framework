using System;
using System.Reflection;
using AspectCore.DynamicProxy;

namespace AspectCore.Configuration
{
    public abstract class InterceptorFactory
    {
        private static readonly AspectPredicate[] EmptyPredicates = new AspectPredicate[0];
        private readonly AspectPredicate[] _predicates;

        public AspectPredicate[] Predicates
        {
            get
            {
                return _predicates;
            }
        }

        public InterceptorFactory(params AspectPredicate[] predicates)
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
