using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using LightInject;

namespace AspectCore.Extensions.LightInject
{
    [NonAspect]
    internal class LightInjectScopeResolverFactory : IScopeResolverFactory
    {
        private readonly IServiceContainer _container;

        public LightInjectScopeResolverFactory(IServiceContainer container)
        {
            _container = container;
        }

        public IServiceResolver CreateScope()
        {
            return new LightInjectServiceResolver(_container.BeginScope());
        }
    }
}
