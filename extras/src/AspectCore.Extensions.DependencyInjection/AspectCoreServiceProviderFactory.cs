using System;
using AspectCore.DynamicProxy;
using AspectCore.Injector;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection
{
    [NonAspect]
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