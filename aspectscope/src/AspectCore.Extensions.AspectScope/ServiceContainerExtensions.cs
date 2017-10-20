using System;
using AspectCore.DynamicProxy;
using AspectCore.Injector;

namespace AspectCore.Extensions.AspectScope
{
    public static class ServiceContainerExtensions
    {
        public static IServiceContainer AddAspectScope(this IServiceContainer services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            services.AddType<IAspectScheduler, ScopeAspectScheduler>(Lifetime.Singleton);
            services.AddType<IAspectContextFactory, ScopeAspectContextFactory>(Lifetime.Scoped);
            services.AddType<IAspectBuilderFactory, ScopeAspectBuilderFactory>(Lifetime.Singleton);
            return services;
        }
    }
}