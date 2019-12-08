using System;
using AspectCore.DynamicProxy;
using AspectCore.DependencyInjection;

namespace AspectCore.Extensions.AspectScope
{
    public static class ServiceContainerExtensions
    {
        public static IServiceContext AddAspectScope(this IServiceContext services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            services.AddType<IAspectScheduler, ScopeAspectScheduler>(Lifetime.Scoped);
            services.AddType<IAspectContextFactory, ScopeAspectContextFactory>(Lifetime.Scoped);
            services.AddType<IAspectBuilderFactory, ScopeAspectBuilderFactory>(Lifetime.Scoped);
            return services;
        }
    }
}