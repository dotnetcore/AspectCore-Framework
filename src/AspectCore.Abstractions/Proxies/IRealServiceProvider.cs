using System;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IRealServiceProvider : IServiceProvider
    {
    }

    public static class ServiceProviderExtensions
    {
        public static object GetRealService(this IServiceProvider serviceProvider, Type serviceType)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }
            var readServiceProvider = serviceProvider.GetService(typeof(IRealServiceProvider)) as IRealServiceProvider;
            return readServiceProvider?.GetService(serviceType);
        }

        public static T GetRealService<T>(this IServiceProvider serviceProvider)
        {
            return (T)serviceProvider.GetRealService(typeof(T));
        }
    }
}
