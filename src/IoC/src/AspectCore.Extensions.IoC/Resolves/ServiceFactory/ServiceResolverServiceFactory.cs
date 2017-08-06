using AspectCore.Abstractions;

namespace AspectCore.Extensions.IoC.Resolves
{
    internal sealed class ServiceResolverServiceFactory : IServiceFactory
    {
        private static readonly ServiceKey ServiceResolverKey = new ServiceKey(typeof(IServiceResolver), null);
        public ServiceKey ServiceKey { get; } = ServiceResolverKey;

        public object Invoke(IServiceResolver resolver)
        {
            return resolver;
        }
    }
}