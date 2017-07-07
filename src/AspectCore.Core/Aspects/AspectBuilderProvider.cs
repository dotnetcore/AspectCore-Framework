using System;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    public sealed class AspectBuilderProvider : IAspectBuilderProvider
    {
        private readonly IInterceptorProvider _interceptorProvider;
        private readonly IExecutableInterceptorValidator _executableInterceptorValidator;

        public AspectBuilderProvider(IInterceptorProvider interceptorProvider, IExecutableInterceptorValidator executableInterceptorValidator)
        {
            _interceptorProvider = interceptorProvider ?? throw new ArgumentNullException(nameof(interceptorProvider));
            _executableInterceptorValidator = executableInterceptorValidator ?? throw new ArgumentNullException(nameof(executableInterceptorValidator));
        }

        public IAspectBuilder GetBuilder(Abstractions.AspectContext context)
        {
            var aspectBuilder = new AspectBuilder();
            foreach (var interceptor in _interceptorProvider.GetInterceptors(context.Target.ServiceMethod))
            {
                aspectBuilder.AddAspectDelegate(interceptor.Invoke);
            }
            return aspectBuilder;
        }
    }
}
