using System;
using AspectCore.DynamicProxy;

namespace AspectCore.DependencyInjection
{
    [NonAspect]
    internal class ScopeResolverFactory : IScopeResolverFactory
    {
        private readonly ServiceResolver _serviceResolver;

        public ScopeResolverFactory(IServiceResolver serviceResolver)
        {
            _serviceResolver = serviceResolver as ServiceResolver;
        }

        public IServiceResolver CreateScope()
        {
            if (_serviceResolver == null)
            {
                throw new ArgumentNullException("ServiceResolver");
            }
            return new ServiceResolver(_serviceResolver._root ?? _serviceResolver);
        }
    }
}