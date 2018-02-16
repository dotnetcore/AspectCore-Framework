using System;
using AspectCore.Configuration;

namespace AspectCore.Injector
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