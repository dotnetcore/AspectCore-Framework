﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.Parameters;
using LightInject;
using IServiceContainer = LightInject.IServiceContainer;

namespace AspectCore.Extensions.LightInject
{
    public static class ContainerBuilderExtensions
    {
        private static readonly string[] _nonAspect =
        {
            "LightInject.*",
            "LightInject"
        };

        private static readonly string[] _excepts = new[]
        {
            "Microsoft.Extensions.Logging",
            "Microsoft.Extensions.Options",
            "System",
            "System.*",
            "IHttpContextAccessor",
            "ITelemetryInitializer",
            "IHostingEnvironment",
        }.Concat(_nonAspect).ToArray();

        public static IServiceContainer RegisterDynamicProxy(this IServiceContainer containerBuilder, Action<IAspectConfiguration> configure = null)
        {
            RegisterDynamicProxy(containerBuilder, null, configure);
            return containerBuilder;
        }

        public static IServiceContainer RegisterDynamicProxy(this IServiceContainer container,
            IAspectConfiguration aspectConfig, Action<IAspectConfiguration> configure = null)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }
            aspectConfig = aspectConfig ?? new AspectConfiguration();

            foreach (var m in _nonAspect)
            {
                aspectConfig.NonAspectPredicates.AddNamespace(m);
            }

            configure?.Invoke(aspectConfig);

            container.AddSingleton<IServiceFactory>(container);
            container.AddSingleton<IServiceContainer>(container);
            
            container.AddSingleton<IAspectConfiguration>(aspectConfig)
                .AddTransient(typeof(IManyEnumerable<>), typeof(ManyEnumerable<>))
                .AddSingleton<IServiceProvider, LightInjectServiceResolver>()
                .AddSingleton<IServiceResolver, LightInjectServiceResolver>()
                .AddSingleton<IScopeResolverFactory, LightInjectScopeResolverFactory>()
                .AddSingleton<IAspectContextFactory, AspectContextFactory>()
                .AddSingleton<IAspectActivatorFactory, AspectActivatorFactory>()
                .AddSingleton<IProxyGenerator, ProxyGenerator>()
                .AddSingleton<IParameterInterceptorSelector, ParameterInterceptorSelector>()
                .AddSingleton<IPropertyInjectorFactory, PropertyInjectorFactory>()
                .AddSingleton<IInterceptorCollector, InterceptorCollector>()
                .AddSingleton<IInterceptorSelector, ConfigureInterceptorSelector>(nameof(ConfigureInterceptorSelector))
                .AddSingleton<IInterceptorSelector, AttributeInterceptorSelector>(nameof(AttributeInterceptorSelector)) // To register multiple services, you should set a name for each implement type.
                .AddSingleton<IAdditionalInterceptorSelector, AttributeAdditionalInterceptorSelector>()
                .AddSingleton<IAspectValidatorBuilder, AspectValidatorBuilder>()
                .AddSingleton<IAspectBuilderFactory, AspectBuilderFactory>()
                .AddSingleton<IProxyTypeGenerator, ProxyTypeGenerator>()
                .AddSingleton<IAspectCachingProvider, AspectCachingProvider>()
                .AddTransient(typeof(IAspectExceptionWrapper), typeof(AspectExceptionWrapper));

            var aspectValidator = new AspectValidatorBuilder(aspectConfig).Build();
            container.Decorate(aspectValidator.CreateDecorator());

            return container;
        }
        
        private static Type GetImplType(this ServiceRegistration registration)
        {
            if (registration.FactoryExpression != null) // ByFactory
            {
                return registration.FactoryExpression.Method.ReturnType;
            }
            else if (registration.Value != null) // ByInstance
            {
                return registration.Value.GetType();
            }
            else // ByType
            {
                return registration.ImplementingType;
            }
        }

        private static DecoratorRegistration CreateDecorator(this IAspectValidator aspectValidator)
        {
            var registration = new DecoratorRegistration()
            {
                CanDecorate = s => CanDecorate(s, aspectValidator),
                ImplementingTypeFactory = CreateProxyType
            };
            return registration;
        }

        private static Type CreateProxyType(IServiceFactory factory, ServiceRegistration registration)
        {
            var serviceType = registration.ServiceType.GetTypeInfo();
            var implType = registration.GetImplType();
            var proxyTypeGenerator = factory.GetInstance<IProxyTypeGenerator>();

            if (serviceType.IsClass)
            {
                return proxyTypeGenerator.CreateClassProxyType(serviceType, implType);
            }
            else if (serviceType.IsGenericTypeDefinition)
            {
                return proxyTypeGenerator.CreateClassProxyType(implType, implType);
            }
            else
            {
                return proxyTypeGenerator.CreateInterfaceProxyType(serviceType, implType);
            }
        }

        private static bool CanDecorate(ServiceRegistration registration, IAspectValidator aspectValidator)
        {
            var serviceType = registration.ServiceType.GetTypeInfo();
            var implType = registration.GetImplType().GetTypeInfo();

            if (implType.IsProxy() || !implType.CanInherited())
            {
                return false;
            }
            if (_excepts.Any(x => implType.Name.Matches(x))
                || implType.Namespace != null && _excepts.Any(x => implType.Namespace.Matches(x)))
            {
                return false;
            }
            if (!serviceType.CanInherited() || serviceType.IsNonAspect())
            {
                return false;
            }

            if (!aspectValidator.Validate(serviceType, true) && !aspectValidator.Validate(implType, false))
            {
                return false;
            }
            return true;
        }

    }
}
