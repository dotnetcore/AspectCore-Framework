using System;
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

            foreach (var interceptor in _interceptorCollector.Collect(context.ServiceMethod, context.ImplementationMethod))
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
    }
}