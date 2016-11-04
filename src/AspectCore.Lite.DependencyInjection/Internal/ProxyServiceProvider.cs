using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.Lite.Common;

namespace AspectCore.Lite.DependencyInjection.Internal
{
    internal class ProxyServiceProvider : IServiceProvider, IDisposable
    {
        internal readonly IServiceProvider originalServiceProvider;
        private readonly IProxyMemorizer proxyMemorizer;
        private readonly ISupportProxyService supportProxyService;
        private readonly ISupportOriginalService supportOriginalService;

        internal ProxyServiceProvider(IServiceProvider serviceProvider)
        {
            ExceptionHelper.ThrowArgumentNull(serviceProvider, nameof(serviceProvider));
            originalServiceProvider = serviceProvider;
            proxyMemorizer = serviceProvider.GetRequiredService<IProxyMemorizer>();
            supportProxyService = serviceProvider.GetRequiredService<ISupportProxyService>();
            supportOriginalService = serviceProvider.GetRequiredService<ISupportOriginalService>();
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (serviceType == typeof(IServiceProvider))
            {
                return this;
            }

            var resolvedService = supportOriginalService.GetService(serviceType);
            if (resolvedService == null)
            {
                return null;
            }

            return proxyMemorizer.GetOrSetProxy(resolvedService, () => supportProxyService.GetService(serviceType));
        }

        public void Dispose()
        {
            var disposable = originalServiceProvider as IDisposable;
            disposable?.Dispose();
        }
    }
}
