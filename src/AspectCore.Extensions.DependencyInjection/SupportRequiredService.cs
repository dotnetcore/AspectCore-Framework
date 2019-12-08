using System;
using AspectCore.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection
{
    internal class SupportRequiredService : ISupportRequiredService
    {
        private readonly IServiceResolver _serviceResolver;

        public SupportRequiredService(IServiceResolver serviceResolver)
        {
            _serviceResolver = serviceResolver;
        }

        public object GetRequiredService(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }
            return _serviceResolver.ResolveRequired(serviceType);
        }
    }
}