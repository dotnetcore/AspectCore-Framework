using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.DependencyInjection
{
    internal class AspectServiceProvider : IServiceProvider
    {
        private readonly IServiceCollection services;
        private readonly IServiceProvider serviceProvider;

        public AspectServiceProvider(IServiceCollection services)
        {
            this.services = services;
            services.AddSingleton<IServiceProvider>(this);
            serviceProvider = services.BuildServiceProvider();
        }

        public object GetService(Type serviceType)
        {
            var resolvedService = serviceProvider.GetService(serviceType);
            if (resolvedService == null)
            {
                return null;
            }

            

            return null;
        }
    }
}
