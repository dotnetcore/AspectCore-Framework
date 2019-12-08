using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.DynamicProxy;
using AspectCore.DependencyInjection;
using LightInject;

namespace AspectCore.Extensions.LightInject
{
    [NonAspect]
    internal class LightInjectServiceResolver : IServiceResolver
    {
        private readonly IServiceContainer _container;

        public LightInjectServiceResolver(IServiceContainer container)
        {
            _container = container;
        }

        public object GetService(Type serviceType)
        {
            return _container.TryGetInstance(serviceType);
        }

        public void Dispose()
        {
            _container.Dispose();
        }

        public object Resolve(Type serviceType)
        {
            return _container.TryGetInstance(serviceType);
        }
    }
}
