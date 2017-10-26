using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.AspectScope;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.AspNetCore
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAspectScope(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            services.AddScoped<IAspectScheduler, ScopeAspectScheduler>();
            services.AddScoped<IAspectContextFactory, ScopeAspectContextFactory>();
            services.AddScoped<IAspectBuilderFactory, ScopeAspectBuilderFactory>();
            return services;
        }
    }
}