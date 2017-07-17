using System;
using AspectCore.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection
{
    internal sealed class RealServiceProvider : IRealServiceProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public RealServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object GetService(Type serviceType)
        {
            return _serviceProvider.GetRequiredService(serviceType);
        }
    }
}
