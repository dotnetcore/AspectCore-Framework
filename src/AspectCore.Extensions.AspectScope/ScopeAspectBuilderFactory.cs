using System;
using System.Reflection;
using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.AspectScope
{
    [NonAspect]
    public sealed class ScopeAspectBuilderFactory : IAspectBuilderFactory
    {
        private readonly IInterceptorCollector _interceptorCollector;
        private readonly IAspectScheduler _aspectContextScheduler;

        public ScopeAspectBuilderFactory(IInterceptorCollector interceptorProvider, IAspectScheduler aspectContextScheduler)
        {
            _interceptorCollector = interceptorProvider ?? throw new ArgumentNullException(nameof(interceptorProvider));
            _aspectContextScheduler = aspectContextScheduler ?? throw new ArgumentNullException(nameof(aspectContextScheduler));
        }

        public IAspectBuilder Create(AspectContext context)
        {
            var aspectBuilder = new AspectBuilder(ctx => ctx.Complete(), null);

            foreach (var interceptor in _interceptorCollector.Collect(context.ServiceMethod, context.ImplementationMethod, context.PredicateMethod))
            {
                if (interceptor is IScopeInterceptor scopedInterceptor)
                {
                    if (!_aspectContextScheduler.TryRelate(context, scopedInterceptor))
                        continue;
                }
                aspectBuilder.AddAspectDelegate(interceptor.Invoke);
            }

            return aspectBuilder;
        }

        /// <summary>
        /// Scope-aware GetBuilder: returns a builder with ALL interceptors (no scope filtering)
        /// since scope suppression requires a per-call context to determine the active call chain.
        /// </summary>
        public IAspectBuilder GetBuilder(MethodInfo serviceMethod, MethodInfo implementationMethod)
        {
            return GetBuilder(serviceMethod, implementationMethod, serviceMethod);
        }

        /// <summary>
        /// Scope-aware GetBuilder with predicate method: returns a builder with ALL interceptors
        /// (no scope filtering) since scope suppression requires a per-call context to determine
        /// the active call chain.
        /// </summary>
        public IAspectBuilder GetBuilder(MethodInfo serviceMethod, MethodInfo implementationMethod, MethodInfo predicateMethod)
        {
            var aspectBuilder = new AspectBuilder(ctx => ctx.Complete(), null);

            foreach (var interceptor in _interceptorCollector.Collect(serviceMethod, implementationMethod, predicateMethod))
            {
                aspectBuilder.AddAspectDelegate(interceptor.Invoke);
            }

            return aspectBuilder;
        }
    }
}