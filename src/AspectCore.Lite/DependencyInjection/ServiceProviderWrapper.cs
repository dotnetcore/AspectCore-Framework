using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.DependencyInjection
{
    internal class ServiceProviderWrapper : IServiceProviderWrapper
    {
        private readonly IServiceProvider serviceProvider;

        public ServiceProviderWrapper(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public object GetService(Type serviceType)
        {
            return serviceProvider.GetService(serviceType);
        }
    }
}
