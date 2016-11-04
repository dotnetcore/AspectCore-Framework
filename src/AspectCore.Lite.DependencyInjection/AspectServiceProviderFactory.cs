using System;
using AspectCore.Lite.DependencyInjection.Internal;

namespace AspectCore.Lite.DependencyInjection
{
    public static class AspectServiceProviderFactory
    {
        public static IServiceProvider Create(IServiceProvider serviceProvider)
        {
            return new ProxyServiceProvider(serviceProvider);
        }
    }
}