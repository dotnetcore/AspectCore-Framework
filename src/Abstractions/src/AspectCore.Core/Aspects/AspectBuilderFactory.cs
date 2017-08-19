using System;
using System.Collections.Concurrent;
using System.Reflection;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    [NonAspect]
    public sealed class AspectBuilderFactory : IAspectBuilderFactory
    {
        private static readonly ConcurrentDictionary<MethodInfo, IAspectBuilder> _builders = new ConcurrentDictionary<MethodInfo, IAspectBuilder>();

        private readonly IInterceptorCollector _interceptorCollector;

        public AspectBuilderFactory(IInterceptorCollector interceptorCollector)
        {
            _interceptorCollector = interceptorCollector ?? throw new ArgumentNullException(nameof(interceptorCollector));
        }

        public IAspectBuilder Create(AspectContext context)
        {
            return _builders.GetOrAdd(context.ServiceMethod, Create);
        }

        private IAspectBuilder Create(MethodInfo method)
        {
            var aspectBuilder = new AspectBuilder();

            foreach (var interceptor in _interceptorCollector.Collect(method))
                aspectBuilder.AddAspectDelegate(interceptor.Invoke);

            return aspectBuilder;
        }
    }
}