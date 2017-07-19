using System;
using AspectCore.Abstractions;
using AspectCore.Core;
using AspectCore.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection
{
    internal static class AspectCoreBuilderExtensions
    {
        internal static IAspectCoreBuilder AddAspectActivator(this IAspectCoreBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            builder.Services.AddTransient<IAspectActivator, AspectActivator>();

            builder.Services.AddScoped<IServiceInstanceProvider, ServiceInstanceProvider>();

            builder.Services.AddTransient<IProxyGenerator, ProxyGenerator>();

            builder.Services.AddTransient<IRealServiceProvider>(p => new RealServiceProvider(p));

            builder.Services.AddTransient<IServiceProviderFactory<IServiceCollection>, AspectCoreServiceProviderFactory>();

            return builder;
        }

        internal static IAspectCoreBuilder AddAspectContext(this IAspectCoreBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddTransient<IAspectBuilderFactory, AspectBuilderFactory>();

            builder.Services.AddTransient<IAspectContextFactory, AspectContextFactory>();

            return builder;
        }

        internal static IAspectCoreBuilder AddAspectConfigure(this IAspectCoreBuilder builder, Action<AspectCoreOptions> options)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var aspectCoreOptions = new AspectCoreOptions(builder.Services);

            options?.Invoke(aspectCoreOptions);

            builder.Services.AddSingleton<IAspectConfigureProvider>(new AspectConfigureProvider
                (aspectCoreOptions.InterceptorFactories, aspectCoreOptions.NonAspectPredicates));

            if (builder.Services != aspectCoreOptions.InternalServices)
            {
                foreach (var descriptor in aspectCoreOptions.InternalServices)
                {
                    builder.Services.Add(descriptor);
                }
            }
          
            return builder;
        }

        internal static IAspectCoreBuilder AddAspectValidator(this IAspectCoreBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddTransient<IAspectValidatorBuilder, AspectValidatorBuilder>();
            builder.Services.AddSingleton<IAspectValidationHandler, AccessibleAspectValidationHandler>();
            builder.Services.AddSingleton<IAspectValidationHandler, AttributeAspectValidationHandler>();
            builder.Services.AddSingleton<IAspectValidationHandler, CacheAspectValidationHandler>();
            builder.Services.AddSingleton<IAspectValidationHandler, ConfigureAspectValidationHandler>();
            builder.Services.AddSingleton<IAspectValidationHandler, DynamicallyAspectValidationHandler>();
            builder.Services.AddSingleton<IAspectValidationHandler, NonAspectValidationHandler>();

            return builder;
        }

        internal static IAspectCoreBuilder AddInterceptorProvider(this IAspectCoreBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddScoped<IInterceptorProvider, InterceptorProvider>();
            builder.Services.AddScoped<IInterceptorSelector, ConfigureInterceptorSelector>();
            builder.Services.AddSingleton<IInterceptorSelector, TypeInterceptorSelector>();
            builder.Services.AddSingleton<IInterceptorSelector, MethodInterceptorSelector>();

            return builder;
        }

        internal static IAspectCoreBuilder AddInterceptorActivator(this IAspectCoreBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddScoped<IInterceptorActivator, ActivatorUtilitieInterceptorActivator>();

            return builder;
        }

        internal static IAspectCoreBuilder AddInterceptorInjector(this IAspectCoreBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddScoped<IInterceptorInjectorProvider, InterceptorInjectorProvider>();
            builder.Services.AddSingleton<IPropertyInjectorSelector, PropertyInjectorSelector>();

            return builder;
        }
    }
}