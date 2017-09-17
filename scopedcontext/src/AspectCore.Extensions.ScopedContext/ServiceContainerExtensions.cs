using System;
using AspectCore.DynamicProxy;
using AspectCore.Injector;

namespace AspectCore.Extensions.ScopedContext
{
    public static class ServiceContainerExtensions
    {
        public static IServiceContainer AddScopedContext(this IServiceContainer services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            services.AddType<IAspectContextScheduler, ScopedAspectContextScheduler>();
            services.AddType<IAspectContextFactory, ScopedAspectContextFactory>();
            services.AddType<IAspectBuilderFactory, ScopedAspectBuilderFactory>();
            return services;
        }
    }
}