using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Abstractions.Resolution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.DynamicProxy
{
    public class ProxyFactoryBuilder
    {
        private IAspectConfiguration aspectConfiguration;
        private IServiceProvider serviceProvider;

        public ProxyFactoryBuilder UseConfigure(Action<IAspectConfiguration> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }
            aspectConfiguration = new AspectConfiguration();
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
            return new ProxyFactory(serviceProvider, aspectConfiguration ?? new AspectConfiguration());
        }
    }
}
