using System;
using AspectCore.Configuration;

namespace AspectCore.DependencyInjection
{
    public static class ConfigurationExtensions
    {
        public static IServiceContext Configure(this IServiceContext serviceContext, Action<IAspectConfiguration> configure)
        {
            if (serviceContext == null)
            {
                throw new ArgumentNullException(nameof(serviceContext));
            }
            configure?.Invoke(serviceContext.Configuration);
            return serviceContext;
        }
    }
}