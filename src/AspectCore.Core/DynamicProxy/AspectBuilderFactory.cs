using System;
using System.Reflection;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public sealed class AspectBuilderFactory : IAspectBuilderFactory
    {
        private readonly IInterceptorCollector _interceptorCollector;
        private readonly IAspectCaching _aspectCaching;

        public AspectBuilderFactory(IInterceptorCollector interceptorCollector,
            IAspectCachingProvider aspectCachingProvider)
        {
            if (aspectCachingProvider == null)
            {
                throw new ArgumentNullException(nameof(aspectCachingProvider));
            }
            _interceptorCollector =
                interceptorCollector ?? throw new ArgumentNullException(nameof(interceptorCollector));
            _aspectCaching = aspectCachingProvider.GetAspectCaching(nameof(AspectBuilderFactory));
        }

        public IAspectBuilder Create(AspectContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            return (IAspectBuilder)_aspectCaching.GetOrAdd(GetKey(context.ServiceMethod, context.ImplementationMethod, context.PredicateMethod), key => Create((Tuple<MethodInfo, MethodInfo, MethodInfo>)key));
        }

        private IAspectBuilder Create(Tuple<MethodInfo, MethodInfo, MethodInfo> tuple)
        {
            var aspectBuilder = new AspectBuilder(context => context.Complete(), null);

            foreach (var interceptor in _interceptorCollector.Collect(tuple.Item1, tuple.Item2, tuple.Item3))
                aspectBuilder.AddAspectDelegate(interceptor.Invoke);

            return aspectBuilder;
        }

        private static object GetKey(MethodInfo serviceMethod, MethodInfo implementationMethod, MethodInfo predicateMethod)
        {
            return Tuple.Create(serviceMethod, implementationMethod, predicateMethod);
        }
    }
}