using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Internal
{
    internal static class ServiceLocator
    {
        private static Func<IServiceProvider> serviceFactory;

        internal static IServiceProvider ServiceProvider
        {
            get
            {
                return serviceFactory?.Invoke();
            }
        }

        internal static void SetServiceProvider(Func<IServiceProvider> factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }
            serviceFactory = factory;
        }
    }
}
