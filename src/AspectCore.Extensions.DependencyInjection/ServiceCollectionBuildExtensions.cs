using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.Parameters;
using AspectCore.Extensions.Reflection;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace AspectCore.Extensions.DependencyInjection
{
    public static class ServiceCollectionBuildExtensions
    {
        [Obsolete("Use BuildAspectInjectorProvider to return AspectCore Injector, or Use BuildDynamicProxyServiceProvider to return MSDI ServiceProvider.", true)]
        public static IServiceProvider BuildAspectCoreServiceProvider(this IServiceCollection services)
        {
            return services.BuildDynamicProxyProvider();
        }

        public static ServiceProvider BuildDynamicProxyProvider(this IServiceCollection services)
        {
            return services.WeaveDynamicProxyService().BuildServiceProvider();
        }

        public static ServiceProvider BuildDynamicProxyProvider(this IServiceCollection services, bool validateScopes)
        {
            return services.WeaveDynamicProxyService().BuildServiceProvider(validateScopes);
        }

        public static ServiceProvider BuildDynamicProxyProvider(this IServiceCollection services, ServiceProviderOptions options)
        {
            return services.WeaveDynamicProxyService().BuildServiceProvider(options);
        }

        public static IServiceCollection WeaveDynamicProxyService(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var serviceProvider = services.TryAddDynamicProxyServices().BuildServiceProvider(false);

            var serviceValidator = new ServiceValidator(serviceProvider.GetRequiredService<IAspectValidatorBuilder>());
            var proxyTypeGenerator = serviceProvider.GetRequiredService<IProxyTypeGenerator>();

            IServiceCollection dynamicProxyServices = new ServiceCollection();
            foreach (var service in services)
            {
                if (serviceValidator.TryValidate(service, out var implementationType))
                    dynamicProxyServices.Add(MakeProxyService(service, implementationType, proxyTypeGenerator));
                else
                    dynamicProxyServices.Add(service);
            }

            serviceProvider.Dispose();

            return dynamicProxyServices;
        }

#if NET8_0_OR_GREATER
        private static ServiceDescriptor MakeProxyService(ServiceDescriptor descriptor, Type implementationType, IProxyTypeGenerator proxyTypeGenerator)
        {
            var serviceTypeInfo = descriptor.ServiceType.GetTypeInfo();
            if (serviceTypeInfo.IsClass)
            {
                return descriptor.IsKeyedService
                    ? ServiceDescriptor.DescribeKeyed(
                        descriptor.ServiceType,
                        descriptor.ServiceKey,
                        proxyTypeGenerator.CreateClassProxyType(descriptor.ServiceType, implementationType),
                        descriptor.Lifetime)
                    : ServiceDescriptor.Describe(
                        descriptor.ServiceType,
                        proxyTypeGenerator.CreateClassProxyType(descriptor.ServiceType, implementationType),
                        descriptor.Lifetime);
            }
            else if (serviceTypeInfo.IsGenericTypeDefinition)
            {
                return descriptor.IsKeyedService
                    ? ServiceDescriptor.DescribeKeyed(
                        descriptor.ServiceType,
                        descriptor.ServiceKey,
                        proxyTypeGenerator.CreateClassProxyType(implementationType, implementationType),
                        descriptor.Lifetime)
                    : ServiceDescriptor.Describe(
                        descriptor.ServiceType,
                        proxyTypeGenerator.CreateClassProxyType(implementationType, implementationType),
                        descriptor.Lifetime);
            }
            else
            {
                var proxyType = proxyTypeGenerator.CreateInterfaceProxyType(descriptor.ServiceType, implementationType);
                return descriptor.IsKeyedService
                    ? ServiceDescriptor.DescribeKeyed(
                        descriptor.ServiceType,
                        descriptor.ServiceKey,
                        CreateKeyedFactory(descriptor, proxyType),
                        descriptor.Lifetime)
                    : ServiceDescriptor.Describe(
                        descriptor.ServiceType,
                        CreateFactory(descriptor, proxyType),
                        descriptor.Lifetime);
            }
        }
#else
        private static ServiceDescriptor MakeProxyService(ServiceDescriptor descriptor, Type implementationType, IProxyTypeGenerator proxyTypeGenerator)
        {
            var serviceTypeInfo = descriptor.ServiceType.GetTypeInfo();
            if (serviceTypeInfo.IsClass)
            {
                return ServiceDescriptor.Describe(
                    descriptor.ServiceType,
                    proxyTypeGenerator.CreateClassProxyType(descriptor.ServiceType, implementationType),
                    descriptor.Lifetime);
            }
            else if (serviceTypeInfo.IsGenericTypeDefinition)
            {
                return ServiceDescriptor.Describe(
                    descriptor.ServiceType,
                    proxyTypeGenerator.CreateClassProxyType(implementationType, implementationType),
                    descriptor.Lifetime);
            }
            else
            {
                var proxyType = proxyTypeGenerator.CreateInterfaceProxyType(descriptor.ServiceType, implementationType);
                return ServiceDescriptor.Describe(
                    descriptor.ServiceType,
                    CreateFactory(descriptor, proxyType),
                    descriptor.Lifetime);
            }
        }
#endif

#if NET7_0_OR_GREATER
        [RequiresDynamicCode("Uses ConstructorInfo.Invoke to create proxy instances. Source-generated proxies are preferred for AOT.")]
#endif
        private static Func<IServiceProvider, object> CreateFactory(ServiceDescriptor descriptor, Type proxyType)
        {
            // Try 3-parameter constructor first (IAspectActivatorFactory, IServiceProvider, serviceType)
            var ctor3 = proxyType.GetTypeInfo().GetConstructor(
                new Type[] { typeof(IAspectActivatorFactory), typeof(IServiceProvider), descriptor.ServiceType });
            if (ctor3 != null)
            {
                if (descriptor.ImplementationInstance != null)
                {
                    var implementationInstance = descriptor.ImplementationInstance;
                    return provider => InvokeCtor(ctor3,
                        provider.GetRequiredService<IAspectActivatorFactory>(),
                        provider, implementationInstance);
                }
                else if (descriptor.ImplementationFactory != null)
                {
                    var implementationFactory = descriptor.ImplementationFactory;
                    return provider => InvokeCtor(ctor3,
                        provider.GetRequiredService<IAspectActivatorFactory>(),
                        provider, implementationFactory(provider));
                }
                else
                {
                    var implementationType = descriptor.ImplementationType;
                    return provider =>
                    {
                        var aspectActivatorFactory = provider.GetRequiredService<IAspectActivatorFactory>();
                        var instance = ActivatorUtilities.CreateInstance(provider, implementationType);
                        return InvokeCtor(ctor3, aspectActivatorFactory, provider, instance);
                    };
                }
            }

            // Fallback: legacy 2-parameter constructor (IAspectActivatorFactory, serviceType)
            var ctor2 = proxyType.GetTypeInfo().GetConstructor(
                new Type[] { typeof(IAspectActivatorFactory), descriptor.ServiceType });
            if (ctor2 != null)
            {
                if (descriptor.ImplementationInstance != null)
                {
                    var implementationInstance = descriptor.ImplementationInstance;
                    return provider => InvokeCtor(ctor2,
                        provider.GetRequiredService<IAspectActivatorFactory>(), implementationInstance);
                }
                else if (descriptor.ImplementationFactory != null)
                {
                    var implementationFactory = descriptor.ImplementationFactory;
                    return provider => InvokeCtor(ctor2,
                        provider.GetRequiredService<IAspectActivatorFactory>(), implementationFactory(provider));
                }
                else
                {
                    var implementationType = descriptor.ImplementationType;
                    return provider =>
                    {
                        var aspectActivatorFactory = provider.GetRequiredService<IAspectActivatorFactory>();
                        var instance = ActivatorUtilities.CreateInstance(provider, implementationType);
                        return InvokeCtor(ctor2, aspectActivatorFactory, instance);
                    };
                }
            }

            throw new InvalidOperationException($"Cannot find suitable constructor on proxy type '{proxyType.FullName}'.");
        }

        /// <summary>
        /// Invokes a constructor using ConstructorInfo.Invoke directly.
        /// This is NativeAOT-safe (unlike ConstructorReflector which uses DynamicMethod).
        /// </summary>
        private static object InvokeCtor(ConstructorInfo ctor, params object[] args)
        {
            return ctor.Invoke(args);
        }

#if NET8_0_OR_GREATER
#if NET7_0_OR_GREATER
        [RequiresDynamicCode("Uses ConstructorInfo.Invoke to create proxy instances. Source-generated proxies are preferred for AOT.")]
#endif
        private static Func<IServiceProvider, object, object> CreateKeyedFactory(ServiceDescriptor descriptor, Type proxyType)
        {
            // Try 3-parameter constructor first (IAspectActivatorFactory, IServiceProvider, serviceType)
            var ctor3 = proxyType.GetTypeInfo().GetConstructor(
                new Type[] { typeof(IAspectActivatorFactory), typeof(IServiceProvider), descriptor.ServiceType });
            if (ctor3 != null)
            {
                if (descriptor.KeyedImplementationInstance != null)
                {
                    var implementationInstance = descriptor.KeyedImplementationInstance;
                    return (provider, serviceKey) => InvokeCtor(ctor3,
                        provider.GetRequiredService<IAspectActivatorFactory>(),
                        provider, implementationInstance);
                }
                else if (descriptor.KeyedImplementationFactory != null)
                {
                    var implementationFactory = descriptor.KeyedImplementationFactory;
                    return (provider, serviceKey) => InvokeCtor(ctor3,
                        provider.GetRequiredService<IAspectActivatorFactory>(),
                        provider, implementationFactory(provider, serviceKey));
                }
                else
                {
                    var implementationType = descriptor.KeyedImplementationType;
                    return (provider, serviceKey) =>
                    {
                        var aspectActivatorFactory = provider.GetRequiredService<IAspectActivatorFactory>();
                        var instance = ActivatorUtilities.CreateInstance(provider, implementationType);
                        return InvokeCtor(ctor3, aspectActivatorFactory, provider, instance);
                    };
                }
            }

            // Fallback: legacy 2-parameter constructor (IAspectActivatorFactory, serviceType)
            var ctor2 = proxyType.GetTypeInfo().GetConstructor(
                new Type[] { typeof(IAspectActivatorFactory), descriptor.ServiceType });
            if (ctor2 != null)
            {
                if (descriptor.KeyedImplementationInstance != null)
                {
                    var implementationInstance = descriptor.KeyedImplementationInstance;
                    return (provider, serviceKey) => InvokeCtor(ctor2,
                        provider.GetRequiredService<IAspectActivatorFactory>(), implementationInstance);
                }
                else if (descriptor.KeyedImplementationFactory != null)
                {
                    var implementationFactory = descriptor.KeyedImplementationFactory;
                    return (provider, serviceKey) => InvokeCtor(ctor2,
                        provider.GetRequiredService<IAspectActivatorFactory>(), implementationFactory(provider, serviceKey));
                }
                else
                {
                    var implementationType = descriptor.KeyedImplementationType;
                    return (provider, serviceKey) =>
                    {
                        var aspectActivatorFactory = provider.GetRequiredService<IAspectActivatorFactory>();
                        var instance = ActivatorUtilities.CreateInstance(provider, implementationType);
                        return InvokeCtor(ctor2, aspectActivatorFactory, instance);
                    };
                }
            }

            throw new InvalidOperationException($"Cannot find suitable constructor on proxy type '{proxyType.FullName}'.");
        }
#endif
    }
}