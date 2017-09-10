using System;
using System.Collections.Generic;
using AspectCore.Injector;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection
{
    public static class ServiceContainerExtensions
    {
        public static void Populate(this IServiceContainer serviceContainer, IEnumerable<ServiceDescriptor> services)
        {
        }
    }
}