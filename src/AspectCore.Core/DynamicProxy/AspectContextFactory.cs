using System;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public sealed class AspectContextFactory : IAspectContextFactory
    {
        private static readonly object[] emptyParameters = new object[0];
        private readonly IServiceProvider _serviceProvider;

        public AspectContextFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public AspectContext CreateContext(AspectActivatorContext activatorContext)
        {
            return new RuntimeAspectContext(_serviceProvider,
                activatorContext.ServiceMethod,
                activatorContext.TargetMethod,
                activatorContext.ProxyMethod,
                activatorContext.PredicateMethod,
                activatorContext.TargetInstance,
                activatorContext.ProxyInstance,
                activatorContext.Parameters ?? emptyParameters);
        }

        public AspectContext CreateContext(AspectActivatorContext activatorContext, IAspectInvokeDelegate invokeDelegate)
        {
            return new SourceGeneratedAspectContext(_serviceProvider,
                activatorContext.ServiceMethod,
                activatorContext.TargetMethod,
                activatorContext.ProxyMethod,
                activatorContext.PredicateMethod,
                activatorContext.TargetInstance,
                activatorContext.ProxyInstance,
                activatorContext.Parameters ?? emptyParameters,
                invokeDelegate);
        }

        public void ReleaseContext(AspectContext aspectContext)
        {
            (aspectContext as IDisposable)?.Dispose();
        }
    }
}