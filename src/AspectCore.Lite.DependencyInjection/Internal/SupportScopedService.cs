using AspectCore.Lite.Common;
using AspectCore.Lite.DependencyInjection;
using System;
using System.Collections.Concurrent;

namespace AspectCore.Lite.DependencyInjection.Internal
{
    internal sealed class SupportScopedService : ISupportScopedService
    {
        private readonly ConcurrentDictionary<Type , object> scopedServices = new ConcurrentDictionary<Type , object>();

        private readonly ISupportProxyService proxyServiceProvider;

        public SupportScopedService(ISupportProxyService proxyServiceProvider)
        {
            this.proxyServiceProvider = proxyServiceProvider;
        }

        public object GetService(Type serviceType)
        {
            ExceptionHelper.ThrowArgumentNull(serviceType , nameof(serviceType));
            return scopedServices.GetOrAdd(serviceType , proxyServiceProvider.GetService);
        }

        public void Dispose()
        {
            scopedServices.Clear();
        }
    }
}
