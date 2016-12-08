using AspectCore.Lite.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.DynamicProxy
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
