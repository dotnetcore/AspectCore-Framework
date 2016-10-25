using System;
using AspectCore.Lite.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using AspectCore.Lite.Internal;

namespace AspectCore.Lite.DependencyInjection
{
    public static class ServiceProviderExtensions
    {
        public static object GetOriginalService(this IServiceProvider provider, Type serviceType)
        {
            ExceptionHelper.ThrowArgumentNull(provider , nameof(provider));

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
