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

        public static T GetService<T>(this IOriginalServiceProvider originalServiceProvider)
        {
            if (originalServiceProvider == null)
            {
                throw new ArgumentNullException(nameof(originalServiceProvider));
            }

            return (T)originalServiceProvider.GetService(typeof(T));
        }
    }
}
