using System;
using AspectCore.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection
{
    public static class ServiceCollectionAddExtensions
    {
        public static IServiceCollection AddInterfaceProxy(this IServiceCollection services, Type interfaceType, ServiceLifetime serviceLifetime)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (interfaceType == null)
            {
                throw new ArgumentNullException(nameof(interfaceType));
            }
            if (!interfaceType.IsInterface)
            {
                throw new ArgumentException($"{interfaceType} should be interface.");
            }
            services.Add(ServiceDescriptor.Describe(interfaceType, provider => provider.GetRequiredService<IProxyGenerator>().CreateInterfaceProxy(interfaceType), serviceLifetime));
            return services;
        }

        public static IServiceCollection AddTransientInterfaceProxy(this IServiceCollection services, Type interfaceType)
        {
            return AddInterfaceProxy(services, interfaceType, ServiceLifetime.Transient);
        }

        public static IServiceCollection AddScopedInterfaceProxy(this IServiceCollection services, Type interfaceType)
        {
            return AddInterfaceProxy(services, interfaceType, ServiceLifetime.Scoped);
        }

        public static IServiceCollection AddSingletonInterfaceProxy(this IServiceCollection services, Type interfaceType)
        {
            return AddInterfaceProxy(services, interfaceType, ServiceLifetime.Singleton);
        }

        public static IServiceCollection AddInterfaceProxy<TInterface>(this IServiceCollection services, ServiceLifetime serviceLifetime)
        {
            return AddInterfaceProxy(services, typeof(TInterface), serviceLifetime);
        }

        public static IServiceCollection AddTransientInterfaceProxy<TInterface>(this IServiceCollection services)
        {
            return AddInterfaceProxy(services, typeof(TInterface), ServiceLifetime.Transient);
        }

        public static IServiceCollection AddScopedInterfaceProxy<TInterface>(this IServiceCollection services)
        {
            return AddInterfaceProxy(services, typeof(TInterface), ServiceLifetime.Scoped);
        }

        public static IServiceCollection AddSingletonInterfaceProxy<TInterface>(this IServiceCollection services)
        {
            return AddInterfaceProxy(services, typeof(TInterface), ServiceLifetime.Singleton);
        }

    }
}