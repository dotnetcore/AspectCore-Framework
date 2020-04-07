using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.DynamicProxy;
using AspectCore.DependencyInjection;
using LightInject;

namespace AspectCore.Extensions.LightInject
{
    [NonAspect]
    public class LightInjectServiceResolver : IServiceResolver
    {
        private readonly IServiceFactory _serviceFactory;
        private bool _isDisposed = false;

        public LightInjectServiceResolver(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        public object GetService(Type serviceType)
        {
            return _serviceFactory.TryGetInstance(serviceType);
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            // To avoid dispose more than one time
            // And this code block must be before "Dispose"
            if (_serviceFactory is IDisposable disposable)
                disposable.Dispose();
        }

        public object Resolve(Type serviceType)
        {
            return _serviceFactory.TryGetInstance(serviceType);
        }
    }
}
