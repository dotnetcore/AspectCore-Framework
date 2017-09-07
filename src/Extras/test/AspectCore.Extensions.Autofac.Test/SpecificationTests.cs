using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification;
using AspectCore.Extensions.DependencyInjection;
using AspectCore.Extensions.Autofac;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AspectCore.Abstractions;
using AspectCore.Extensions.Configuration.InterceptorFactories;
using AspectCore.Extensions.Configuration;

namespace AspectCore.Extensions.Autofac.Test
{
    public class SpecificationTests : DependencyInjectionSpecificationTests
    {
        protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
        {
            serviceCollection.AddAspectCore(config => config.InterceptorFactories.AddDelegate(next => ctx => next(ctx)));
            var services = new AspectCoreServiceProviderFactory().CreateBuilder(serviceCollection);
            var builder = new ContainerBuilder();
            builder.Populate(services);
            return new AutofacServiceProvider(builder.Build());
        }
    }

    public class Interceptor : InterceptorAttribute
    {

    }
}
