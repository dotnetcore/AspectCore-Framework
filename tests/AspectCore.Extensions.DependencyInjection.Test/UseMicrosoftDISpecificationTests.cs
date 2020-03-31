using System;
using AspectCore.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification;

namespace AspectCore.Extensions.DependencyInjection.Test
{
    public class UseMicrosoftDISpecificationTests : DependencyInjectionSpecificationTests
    {
        protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
        {
            serviceCollection.ConfigureDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate((ctx, next) => next(ctx));
            });

            return serviceCollection.BuildDynamicProxyProvider();
        }
    }
}