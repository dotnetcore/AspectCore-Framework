using System;
using AspectCore.Lite.Abstractions;

namespace AspectCore.Lite.Internals
{
    internal sealed class OriginalServiceProvider : IOriginalServiceProvider
    {
        private readonly IServiceProvider serviceProvider;

        public OriginalServiceProvider(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public object GetService(Type serviceType)
        {
            return serviceProvider.GetService(serviceType);
        }
    }
}