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
            return serviceProvider.GetService(serviceType);
        }

        public IServiceProvider OriginalServiceProvider
        {
            get { return serviceProvider; }
        }
    }
}
