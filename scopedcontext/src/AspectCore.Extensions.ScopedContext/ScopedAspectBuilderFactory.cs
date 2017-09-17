using System;
using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.ScopedContext
{
    [NonAspect]
    internal sealed class ScopedAspectBuilderFactory : IAspectBuilderFactory
    {
        private readonly IInterceptorCollector _interceptorCollector;
        private readonly IAspectContextScheduler _aspectContextScheduler;

        public ScopedAspectBuilderFactory(IInterceptorCollector interceptorProvider, IAspectContextScheduler aspectContextScheduler)
        {
            _interceptorCollector = interceptorProvider ?? throw new ArgumentNullException(nameof(interceptorProvider));
            _aspectContextScheduler = aspectContextScheduler ?? throw new ArgumentNullException(nameof(aspectContextScheduler));
        }

        public IAspectBuilder Create(AspectContext context)
        {
            var aspectBuilder = new AspectBuilder();

            foreach (var interceptor in _interceptorCollector.Collect(context.ServiceMethod))
            {
                if (interceptor is IScopedInterceptor scopedInterceptor)
                {
                    if (!_aspectContextScheduler.TryInclude(context as ScopedAspectContext, scopedInterceptor))
                        continue;
                }
                aspectBuilder.AddAspectDelegate(interceptor.Invoke);
            }

            return aspectBuilder;
        }
    }
}
