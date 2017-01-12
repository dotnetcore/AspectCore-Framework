using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Abstractions.Resolution;
using System;

namespace AspectCore.Lite.DynamicProxy
{
    public class ProxyFactoryBuilder
    {
        private IServiceProvider serviceProvider;

        public ProxyFactoryBuilder()
        {
            serviceProvider = new ServiceProvider(new AspectConfiguration());
        }

        public ProxyFactoryBuilder UseConfigure(Action<IAspectConfiguration> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }
            var aspectConfiguration = serviceProvider.GetService<IAspectConfiguration>();
            configure(aspectConfiguration);
            return this;
        }

        public ProxyFactoryBuilder UseServiceProvider(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }
            this.serviceProvider = serviceProvider;
            return this;
        }

        public IProxyFactory Build()
        {
            return new ProxyFactory(serviceProvider);
        }
    }
}
