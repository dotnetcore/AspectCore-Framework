using System;
using AspectCore.Injector;

namespace AspectCore.Configuration
{
    public static class ConfigurationExtensions
    {
        public static IServiceContainer Configure(this IServiceContainer serviceContainer, Action<IAspectConfiguration> configure)
        {
            if (serviceContainer == null)
            {
                throw new ArgumentNullException(nameof(serviceContainer));
            }
            configure?.Invoke(serviceContainer.Configuration);
            return serviceContainer;
        }
    }
}