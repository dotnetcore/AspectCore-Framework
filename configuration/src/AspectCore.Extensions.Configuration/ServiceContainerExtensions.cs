using System;
using AspectCore.Injector;

namespace AspectCore.Extensions.Configuration
{
    public static class ServiceContainerExtensions
    {
        public static IServiceContainer AddConfigurationInject(this IServiceContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            container.AddType<IServiceResolveCallback, ConfigurationBindResolveCallback>(Lifetime.Singleton);
            return container;
        }
    }
}