using System;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.DependencyInjection.Internals
{
    internal sealed class RealServiceProvider : IRealServiceProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public RealServiceProvider(IServiceProvider serviceProvider)
        {
            if (serviceProvider is AspectCoreServiceProvider wrapper)
            {
                _serviceProvider = wrapper.ServiceProvider;
            }
            else
            {
                _serviceProvider = serviceProvider;
            }
        }

        public object GetService(Type serviceType)
        {
            return _serviceProvider.GetService(serviceType);
        }
    }
}
