using System;
using AspectCore.Lite.Common;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Lite.DependencyInjection
{
    public static class AspectServiceProviderFactory
    {
        /// <summary>
        /// Create Aspect Proxy ServiceProvider.
        /// Before call Create method , must call the IServiceCollection.AddAspectLite extension method to add the AspectLiteServices to the serviceCollection.
        /// </summary>
        /// <param name="serviceProvider">original serviceProvider</param>
        /// <exception cref="InvalidOperationException">There is no services of AspectLite dependent on.</exception>
        /// <returns>Aspect Proxy ServiceProvider</returns>
        /// <example>
        /// IServiceCollection services = new ServiceCollection();
        /// services.AddAspectLite();
        /// IServiceProvider proxyServiceProvider = AspectServiceProviderFactory.Create(services.BuildServiceProvider());
        /// </example>
        public static IServiceProvider Create(IServiceProvider serviceProvider)
        {
            ExceptionHelper.ThrowArgumentNull(serviceProvider, nameof(serviceProvider));
            return serviceProvider.GetRequiredService<IProxyServiceProvider>();
        }
    }
}