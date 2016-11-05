using Microsoft.Extensions.DependencyInjection;
using System;
using AspectCore.Lite.DependencyInjection;

namespace AspectCore.Lite.DependencyInjection.Test
{
    public interface IDependencyInjection
    {
    }

     public static class IDependencyInjectionExtensions
    {
        public static IServiceProvider BuildServiceProvider(this IDependencyInjection di , Action<IServiceCollection> action = null)
        {
            IServiceCollection services = new ServiceCollection();

            services.AddAspectLite();
            action?.Invoke(services);

            return services.BuildServiceProvider();
        }

        public static IServiceProvider BuildProxyServicePrivoder(this IDependencyInjection di,
            Action<IServiceCollection> action = null)
        {
            var provider = di.BuildServiceProvider(action);
            return AspectServiceProviderFactory.Create(provider);
        }
    }
}