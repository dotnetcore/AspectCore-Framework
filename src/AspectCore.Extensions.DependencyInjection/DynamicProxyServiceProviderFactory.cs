using System;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection
{
    public class DynamicProxyServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
    {
        public DynamicProxyServiceProviderFactory()
        {
        }

        public DynamicProxyServiceProviderFactory(bool validateScopes) : this(new ServiceProviderOptions
            {ValidateScopes = validateScopes})
        {
        }

        public DynamicProxyServiceProviderFactory(ServiceProviderOptions serviceProviderOptions)
        {
            ServiceProviderOptions = serviceProviderOptions;
        }

        public ServiceProviderOptions ServiceProviderOptions { get; }

        public IServiceCollection CreateBuilder(IServiceCollection services)
        {
            return services;
        }

        public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
        {
            return ServiceProviderOptions is null
                ? containerBuilder.BuildDynamicProxyProvider()
                : containerBuilder.BuildDynamicProxyProvider(ServiceProviderOptions);
        }
    }
}
