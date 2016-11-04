using System;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Lite.DependencyInjection.Internal
{
    internal class ServiceScope : IServiceScope
    {
        private readonly ProxyServiceProvider proxyServiceProvider;

        public ServiceScope(ISupportOriginalService supportOriginalService)
        {
            this.proxyServiceProvider = new ProxyServiceProvider(supportOriginalService.OriginalServiceProvider);
        }

        public void Dispose()
        {
            proxyServiceProvider.Dispose();
        }

        public IServiceProvider ServiceProvider
        {
            get { return proxyServiceProvider; }
        }
    }
}