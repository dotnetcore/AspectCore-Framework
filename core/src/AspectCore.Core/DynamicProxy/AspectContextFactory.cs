using System;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public class AspectContextFactory : IAspectContextFactory
    {
        private static readonly object[] emptyParameters = new object[0];
        private readonly IServiceProvider _serviceProvider;

        public AspectContextFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public virtual AspectContext CreateContext(AspectActivatorContext activatorContext)
        {
            return new RuntimeAspectContext(_serviceProvider,
                activatorContext.ServiceMethod,
                activatorContext.TargetMethod,
                activatorContext.ProxyMethod,
                activatorContext.TargetInstance,
                activatorContext.ProxyInstance,
                activatorContext.Parameters ?? emptyParameters);
        }
    }
}