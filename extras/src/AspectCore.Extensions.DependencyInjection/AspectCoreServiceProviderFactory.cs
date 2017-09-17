using System;
using AspectCore.Injector;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection
{
    public class AspectCoreServiceProviderFactory : IServiceProviderFactory<IServiceContainer>
    {
        public IServiceContainer CreateBuilder(IServiceCollection services)
        {
            return services.ToServiceContainer();
        }

        public IServiceProvider CreateServiceProvider(IServiceContainer containerBuilder)
        {
            return containerBuilder.Build();
        }
    }
}