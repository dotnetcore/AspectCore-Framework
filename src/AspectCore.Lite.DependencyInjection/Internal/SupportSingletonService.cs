using AspectCore.Lite.Common;
using System;
using System.Collections.Concurrent;

namespace AspectCore.Lite.DependencyInjection.Internal
{
    internal sealed class SupportSingletonService : ISupportSingletonService
    {
        private readonly static ConcurrentDictionary<Type , object> singletonServices = new ConcurrentDictionary<Type , object>();

        private readonly ISupportProxyService proxyServiceProvider;

        public SupportSingletonService(ISupportProxyService proxyServiceProvider)
        {
            this.proxyServiceProvider = proxyServiceProvider;
        }

        public void Dispose()
        {
            singletonServices.Clear();
        }

        public object GetService(Type serviceType)
        {
            ExceptionHelper.ThrowArgumentNull(serviceType , nameof(serviceType));
            return singletonServices.GetOrAdd(serviceType , proxyServiceProvider.GetService);
        }

    }
}