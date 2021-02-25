using System;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection
{
    public class DynamicProxyServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
    {
        private ServiceProviderOptions _serviceProviderOptions;


        public DynamicProxyServiceProviderFactory()
            : this(null)
        {
        }

        public DynamicProxyServiceProviderFactory(bool validateScopes)
            : this(new ServiceProviderOptions() {ValidateScopes = validateScopes})
        {
        }

        public DynamicProxyServiceProviderFactory(ServiceProviderOptions serviceProviderOptions)
        {
            _serviceProviderOptions = serviceProviderOptions;
        }

        public IServiceCollection CreateBuilder(IServiceCollection services)
        {
            return services;
        }

        public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
        {
            return _serviceProviderOptions == null
                ? containerBuilder.BuildDynamicProxyProvider()
                : containerBuilder.BuildDynamicProxyProvider(_serviceProviderOptions);
        }
    }
}