using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.Lite.Common;

namespace AspectCore.Lite.DependencyInjection.Internal
{
    internal class ProxyServiceProvider : IProxyServiceProvider
    {
        internal readonly IServiceProvider originalServiceProvider;

        public ProxyServiceProvider(IServiceProvider serviceProvider)
        {
            ExceptionHelper.ThrowArgumentNull(serviceProvider, nameof(serviceProvider));
            originalServiceProvider = serviceProvider;
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

            var supportOriginalService = originalServiceProvider.GetRequiredService<ISupportOriginalService>();
            var resolvedService = supportOriginalService.GetService(serviceType);

            if (resolvedService == null)
            {
                return null;
            }

            var proxyMemorizer = originalServiceProvider.GetRequiredService<IProxyMemorizer>();
            return proxyMemorizer.GetOrSetProxy(resolvedService, () =>
            {
                var supportProxyService = originalServiceProvider.GetRequiredService<ISupportProxyService>();
                supportProxyService.OriginalServiceInstance = resolvedService;
                return supportProxyService.GetService(serviceType);
            });
        }

        public void Dispose()
        {
            var disposable = originalServiceProvider as IDisposable;
            disposable?.Dispose();
        }
    }
}
