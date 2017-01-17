using System;

namespace AspectCore.Abstractions
{
    public static class OriginalServiceProviderExtensions
    {
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
