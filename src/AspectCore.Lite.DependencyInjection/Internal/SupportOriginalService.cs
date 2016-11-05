using AspectCore.Lite.DependencyInjection;
using System;

namespace AspectCore.Lite.DependencyInjection.Internal
{
    internal class SupportOriginalService : ISupportOriginalService
    {
        private readonly IServiceProvider serviceProvider;

        public SupportOriginalService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public object GetService(Type serviceType)
        {
            try
            {
                return serviceProvider.GetService(serviceType);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException($"Unable to resolve original service for type '{serviceType}'.", exception);
            }
        }

        public IServiceProvider OriginalServiceProvider
        {
            get { return serviceProvider; }
        }
    }
}
