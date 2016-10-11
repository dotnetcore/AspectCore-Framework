using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.Lite.DependencyInjection;

namespace AspectCore.Lite.Test
{
    public static class DependencyResolver
    {
        public static IServiceProvider GetServiceProvider(Action<IServiceCollection> action = null)
        {
            IServiceCollection services = new ServiceCollection();

            services.AddAspectLite();
            action?.Invoke(services);

            return services.BuildServiceProvider();
        }
    }
}
