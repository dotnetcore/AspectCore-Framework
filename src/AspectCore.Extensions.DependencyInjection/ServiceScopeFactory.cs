using AspectCore.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection
{
    internal class ServiceScopeFactory : IServiceScopeFactory
    {
        private readonly IServiceResolver _serviceResolver;

        public ServiceScopeFactory(IServiceResolver serviceResolver)
        {
            _serviceResolver = serviceResolver;
        }

        public IServiceScope CreateScope()
        {
            return new ServiceScope(_serviceResolver.CreateScope());
        }
    }
}
