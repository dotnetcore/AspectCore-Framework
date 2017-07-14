using System;
using AspectCore.Abstractions;
using AspectCore.Core;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection
{
    internal static class AspectCoreBuilderExtensions
    {
        internal static IAspectCoreBuilder AddAspectBuilder(this IAspectCoreBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            builder.Services.AddTransient<IAspectActivator, AspectActivator>();

            builder.Services.AddTransient<IAspectBuilderProvider, AspectBuilderProvider>();

            builder.Services.AddTransient<IServiceInstanceProvider, ServiceInstanceProvider>();

            builder.Services.AddTransient<IProxyGenerator, ProxyGenerator>();

            builder.Services.AddTransient<IRealServiceProvider>(p => new RealServiceProvider(p));

            builder.Services.AddScoped<IAspectContextScheduler, AspectContextScheduler>();

            return builder;
        }

        internal static IAspectCoreBuilder AddAspectConfigure(this IAspectCoreBuilder builder, Action<AspectCoreOptions> options)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var aspectCoreOptions = new AspectCoreOptions();

            options?.Invoke(aspectCoreOptions);

            builder.Services.AddSingleton<IAspectConfigureProvider>(new AspectConfigureProvider(aspectCoreOptions));

            return builder;
        }

        internal static IAspectCoreBuilder AddAspectValidator(this IAspectCoreBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddTransient<IAspectValidatorBuilder, AspectValidatorBuilder>();
            builder.Services.AddTransient<IAspectValidationHandler, AccessibleAspectValidationHandler>();
            builder.Services.AddTransient<IAspectValidationHandler, AttributeAspectValidationHandler>();
            builder.Services.AddTransient<IAspectValidationHandler, CacheAspectValidationHandler>();
            builder.Services.AddTransient<IAspectValidationHandler, ConfigureAspectValidationHandler>();
            builder.Services.AddTransient<IAspectValidationHandler, DynamicallyAspectValidationHandler>();
            builder.Services.AddTransient<IAspectValidationHandler, NonAspectValidationHandler>();
            builder.Services.AddTransient<IServiceProviderFactory<IServiceCollection>, AspectCoreServiceProviderFactory>();
            return builder;
        }

        internal static IAspectCoreBuilder AddInterceptorProvider(this IAspectCoreBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddTransient<IInterceptorProvider, InterceptorProvider>();
            builder.Services.AddTransient<IInterceptorSelector, ConfigureInterceptorSelector>();
            builder.Services.AddTransient<IInterceptorSelector, TypeInterceptorSelector>();
            builder.Services.AddTransient<IInterceptorSelector, MethodInterceptorSelector>();

            return builder;
        }

        internal static IAspectCoreBuilder AddInterceptorActivator(this IAspectCoreBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddTransient<IInterceptorActivator, ActivatorUtilitieInterceptorActivator>();

            return builder;
        }

        internal static IAspectCoreBuilder AddInterceptorInjector(this IAspectCoreBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddTransient<IInterceptorInjectorProvider, InterceptorInjectorProvider>();
            builder.Services.AddTransient<IPropertyInjectorSelector, PropertyInjectorSelector>();

            return builder;
        }
    }
}