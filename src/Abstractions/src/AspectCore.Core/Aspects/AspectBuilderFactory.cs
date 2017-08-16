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

        private readonly IInterceptorProvider _interceptorProvider;

        public AspectBuilderFactory(IInterceptorProvider interceptorProvider)
        {
            _interceptorProvider = interceptorProvider ?? throw new ArgumentNullException(nameof(interceptorProvider));
        }

        public IAspectBuilder Create(AspectContext context)
        {
            return _builders.GetOrAdd(context.Target.ServiceMethod, Create);
        }

        private IAspectBuilder Create(MethodInfo method)
        {
            var aspectBuilder = new AspectBuilder();

            foreach (var interceptor in _interceptorProvider.GetInterceptors(method))
                aspectBuilder.AddAspectDelegate(interceptor.Invoke);



            return aspectBuilder;
        }
    }
}