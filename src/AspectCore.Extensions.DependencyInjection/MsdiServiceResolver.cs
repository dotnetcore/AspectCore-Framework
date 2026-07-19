using System;
using AspectCore.DependencyInjection;
#if NET8_0_OR_GREATER
using Microsoft.Extensions.DependencyInjection;
#endif

namespace AspectCore.Extensions.DependencyInjection
{
    internal class MsdiServiceResolver : IServiceResolver
    {
        private readonly IServiceProvider _serviceProvider;
        public MsdiServiceResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Dispose()
        {
            var d = _serviceProvider as IDisposable;
            d?.Dispose();
        }

        public object GetService(Type serviceType)
        {
            return _serviceProvider.GetService(serviceType);
        }

        public object Resolve(Type serviceType)
        {
            return _serviceProvider.GetService(serviceType);
        }

#if NET8_0_OR_GREATER
        public object GetKeyedService(Type serviceType, object serviceKey)
        {
            if (_serviceProvider is IKeyedServiceProvider keyedProvider)
            {
                return keyedProvider.GetKeyedService(serviceType, serviceKey);
            }
            return _serviceProvider.GetService(serviceType);
        }

        public object GetRequiredKeyedService(Type serviceType, object serviceKey)
        {
            if (_serviceProvider is IKeyedServiceProvider keyedProvider)
            {
                return keyedProvider.GetRequiredKeyedService(serviceType, serviceKey);
            }
            var service = _serviceProvider.GetService(serviceType);
            if (service == null)
            {
                throw new InvalidOperationException($"No service for type '{serviceType}' has been registered.");
            }
            return service;
        }
#endif
    }
}
