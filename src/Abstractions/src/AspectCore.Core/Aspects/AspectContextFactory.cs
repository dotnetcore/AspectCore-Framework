using System;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    public class AspectContextFactory : IAspectContextFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public AspectContextFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public virtual AspectContext CreateContext<TReturn>(AspectActivatorContext activatorContext)
        {
            var target = new TargetDescriptor(activatorContext.TargetInstance,
              activatorContext.ServiceMethod,
              activatorContext.ServiceType,
              activatorContext.TargetMethod,
              activatorContext.TargetMethod.DeclaringType);

            var proxy = new ProxyDescriptor(activatorContext.ProxyInstance,
                activatorContext.ProxyMethod,
                activatorContext.ProxyInstance.GetType());

            var parameters = new ParameterCollection(activatorContext.Parameters,
                activatorContext.ServiceMethod.GetParameters());

            var returnParameter = new ReturnParameterDescriptor(default(TReturn),
                activatorContext.ServiceMethod.ReturnParameter);

            return new RuntimeAspectContext(_serviceProvider, target, proxy, parameters, returnParameter);
        }
    }
}
