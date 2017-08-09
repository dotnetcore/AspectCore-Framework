using System;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.IoC.Resolves
{
    internal sealed class ServiceResolverServiceFactory : IServiceFactory
    {
        public static readonly ServiceKey ServiceResolverKey = new ServiceKey(typeof(IServiceResolver), null);
        public ServiceKey ServiceKey { get; } = ServiceResolverKey;

        public ServiceDefinition ServiceDefinition { get; } = new TypeServiceDefinition(typeof(IServiceResolver), typeof(ServiceResolver), Lifetime.Scoped, null);

        public object Invoke(IServiceResolver serviceResolver)
        {
            return serviceResolver;
        }
    }
}