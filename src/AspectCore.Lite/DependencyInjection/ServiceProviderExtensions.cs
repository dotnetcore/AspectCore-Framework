using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Lite.DependencyInjection
{
    public static class ServiceProviderExtensions
    {
        public static object GetOriginalService(this IServiceProvider provider, Type serviceType)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            var wrapper = provider as IServiceProviderWrapper;
            if (wrapper != null)
            {
                wrapper.GetService(serviceType);
            }

            var proxyProvider = provider as ProxyServiceProvider;
            if (proxyProvider != null)
            {
                wrapper = provider.GetRequiredService<IServiceProviderWrapper>();
                return wrapper.GetService(serviceType);
            }

            return provider.GetService(serviceType);
        }

        public static T GetOriginalService<T>(this IServiceProvider provider)
        {
            return (T)provider.GetOriginalService(typeof(T));
        }
    }
}
