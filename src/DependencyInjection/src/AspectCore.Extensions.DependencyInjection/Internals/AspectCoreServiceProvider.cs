using System;
using AspectCore.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection.Internals
{
    internal sealed class AspectCoreServiceProvider : IServiceProvider, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;

        internal IServiceProvider ServiceProvider => _serviceProvider;

        public AspectCoreServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Dispose()
        {
            (_serviceProvider as IDisposable)?.Dispose();
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IServiceProvider))
            {
                return this;
            }
            if (serviceType == typeof(IServiceScopeFactory))
            {
                return new AspectCoreServiceScopeFactory(
                    _serviceProvider.GetService<IRealServiceProvider>(),
                    _serviceProvider.GetService<IServiceScopeAccessor>());
            }
            return _serviceProvider.GetService(serviceType);
        }
    }
}
