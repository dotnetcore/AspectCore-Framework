using System;
using AspectCore.Abstractions;

namespace AspectCore.Core.DynamicProxy
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
