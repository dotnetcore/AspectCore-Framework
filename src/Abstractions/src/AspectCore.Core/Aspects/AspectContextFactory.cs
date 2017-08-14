using System;
using System.Reflection;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    public class AspectContextFactory : IAspectContextFactory
    {
        private static readonly ParameterCollection emptyParameterCollection = new ParameterCollection(new object[0], new ParameterInfo[0]);
        private readonly IServiceProvider _serviceProvider;

        public AspectContextFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public virtual AspectContext CreateContext<TReturn>(AspectActivatorContext activatorContext)
        {
            var serviceMethod = activatorContext.ServiceMethod;
            var targetMethod = activatorContext.TargetMethod;
            var proxyMethod = activatorContext.ProxyMethod;
            var parameters = activatorContext.Parameters;
            var target = new TargetDescriptor(activatorContext.ServiceInstance, serviceMethod, activatorContext.ServiceType, targetMethod);
            var proxy = new ProxyDescriptor(activatorContext.ProxyInstance, proxyMethod, proxyMethod.DeclaringType);
            var parameterCollection = parameters == null ? emptyParameterCollection : new ParameterCollection(parameters, serviceMethod.GetParameters());
            var returnParameter = new ReturnParameterDescriptor(default(TReturn), serviceMethod.ReturnParameter);
            return new RuntimeAspectContext(_serviceProvider, target, proxy, parameterCollection, returnParameter);
        }
    }
}