using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.Abstractions;

namespace AspectCore.Core.Injector
{
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