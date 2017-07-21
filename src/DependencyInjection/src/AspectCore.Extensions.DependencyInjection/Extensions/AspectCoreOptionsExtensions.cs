using System;
using AspectCore.Extensions.Configuration;
using AspectCore.Extensions.DependencyInjection.Internals;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection
{
    public static class AspectCoreOptionsExtensions
    {
        public static AspectCoreOptions AddScopedServiceAccessor(this AspectCoreOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.InternalServices.AddTransient(typeof(ITransientServiceAccessor<>), typeof(TransientServiceAccessor<>));
            options.InternalServices.AddScoped(typeof(IScopedServiceAccessor<>), typeof(ScopedServiceAccessor<>));
            options.InternalServices.AddTransient<IServiceScopeFactory, AspectCoreServiceScopeFactory>();
            options.InternalServices.AddScoped<IServiceScopeAccessor, ServiceScopeAccessor>();
            options.InternalServices.AddScoped<IServiceProvider, AspectCoreServiceProvider>();

            return options;
        }
    }
}
