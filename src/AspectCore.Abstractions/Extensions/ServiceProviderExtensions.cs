using System;

namespace AspectCore.Abstractions.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static IAspectActivator GetAspectActivator(this IServiceProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            return (IAspectActivator)provider.GetService(typeof(IAspectActivator));
        }
    }
}
