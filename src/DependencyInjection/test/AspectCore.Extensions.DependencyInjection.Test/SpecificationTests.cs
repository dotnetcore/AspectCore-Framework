using Microsoft.Extensions.DependencyInjection.Specification;
using System;
using Microsoft.Extensions.DependencyInjection;
using AspectCore.Abstractions;
using AspectCore.Extensions.Configuration;

namespace AspectCore.Extensions.DependencyInjection.Test
{
    public class SpecificationTests : DependencyInjectionSpecificationTests
    {
        protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
        {
            serviceCollection.AddAspectCore(option =>
            {
                option.InterceptorFactories.AddDelegate((ctx, next) => next(ctx));
                option.InterceptorFactories.AddDelegate(next => ctx => next(ctx));
            });
            return serviceCollection.BuildAspectCoreServiceProvider();
        }
    }

    public class Interceptor : InterceptorAttribute { }
}
