using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.Parameters;
using AspectCore.DependencyInjection;
using Autofac;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using AParameter = Autofac.Core.Parameter;

namespace AspectCore.Extensions.Autofac
{
    public static class ContainerBuilderExtensions
    {
        #region RegisterDynamicProxy
        private static readonly List<string> excepts = new List<string>
        {
            "Microsoft.Extensions.Logging",
            "Microsoft.Extensions.Options",
            "System",
            "System.*",
            "IHttpContextAccessor",
            "ITelemetryInitializer",
            "IHostingEnvironment",
            "Autofac.*",
            "Autofac"
        };

        public static ContainerBuilder RegisterDynamicProxy(this ContainerBuilder containerBuilder, Action<IAspectConfiguration> configure = null)
        {
            RegisterDynamicProxy(containerBuilder, null, configure);
            return containerBuilder;
        }

        public static ContainerBuilder RegisterDynamicProxy(this ContainerBuilder containerBuilder, IAspectConfiguration configuration, Action<IAspectConfiguration> configure = null)
        {
            if (containerBuilder == null)
            {
                throw new ArgumentNullException(nameof(containerBuilder));
            }
            configuration = configuration ?? new AspectConfiguration();

            configuration.NonAspectPredicates.
                AddNamespace("Autofac").
                AddNamespace("Autofac.*");

            configure?.Invoke(configuration);

            containerBuilder.RegisterInstance<IAspectConfiguration>(configuration).SingleInstance();
            containerBuilder.RegisterGeneric(typeof(ManyEnumerable<>)).As(typeof(IManyEnumerable<>)).InstancePerDependency();
            containerBuilder.RegisterType<AutofacServiceResolver>().As<IServiceProvider, IServiceResolver>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<AutofacScopeResolverFactory>().As<IScopeResolverFactory>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<AspectContextFactory>().As<IAspectContextFactory>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<AspectActivatorFactory>().As<IAspectActivatorFactory>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<ProxyGenerator>().As<IProxyGenerator>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<ParameterInterceptorSelector>().As<IParameterInterceptorSelector>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<PropertyInjectorFactory>().As<IPropertyInjectorFactory>().InstancePerLifetimeScope();

            containerBuilder.RegisterType<InterceptorCollector>().As<IInterceptorCollector>().SingleInstance();
            containerBuilder.RegisterType<ConfigureInterceptorSelector>().As<IInterceptorSelector>().SingleInstance();
            containerBuilder.RegisterType<AttributeInterceptorSelector>().As<IInterceptorSelector>().SingleInstance();
            containerBuilder.RegisterType<AttributeAdditionalInterceptorSelector>().As<IAdditionalInterceptorSelector>().SingleInstance();
            containerBuilder.RegisterType<AspectValidatorBuilder>().As<IAspectValidatorBuilder>().SingleInstance();
            containerBuilder.RegisterType<AspectBuilderFactory>().As<IAspectBuilderFactory>().SingleInstance();
            containerBuilder.RegisterType<ProxyTypeGenerator>().As<IProxyTypeGenerator>().SingleInstance();
            containerBuilder.RegisterType<AspectCachingProvider>().As<IAspectCachingProvider>().SingleInstance();
            containerBuilder.RegisterType<AspectExceptionWrapper>().As<IAspectExceptionWrapper>().SingleInstance();
            
            containerBuilder.RegisterCallback(registryBuilder =>
            {
                registryBuilder.Registered += Registry_Registered;
            });

            return containerBuilder;
        }

        private static void Registry_Registered(object sender, ComponentRegisteredEventArgs e)
        {
            e.ComponentRegistration.Activating += ComponentRegistration_Activating;
        }

        private static void ComponentRegistration_Activating(object sender, ActivatingEventArgs<object> e)
        {
            if (e.Instance == null || e.Instance.IsProxy())
            {
                return;
            }
            if (!(e.Component.Activator is ReflectionActivator))
            {
                return;
            }
            var limitType = e.Instance.GetType();
            if (!limitType.GetTypeInfo().CanInherited())
            {
                return;
            }
            if (excepts.Any(x => limitType.Name.Matches(x)) || excepts.Any(x => limitType.Namespace.Matches(x)))
            {
                return;
            }
            var services = e.Component.Services.Select(x => ((IServiceWithType)x).ServiceType).ToList();
            if (!services.All(x => x.GetTypeInfo().CanInherited()) || services.All(x => x.GetTypeInfo().IsNonAspect()))
            {
                return;
            }
            var aspectValidator = new AspectValidatorBuilder(e.Context.Resolve<IAspectConfiguration>()).Build();
            if (services.All(x => !aspectValidator.Validate(x, true)) && !aspectValidator.Validate(limitType, false))
            {
                return;
            }
            var proxyTypeGenerator = e.Context.Resolve<IProxyTypeGenerator>();
            Type proxyType; object instance;
            var interfaceType = services.FirstOrDefault(x => x.GetTypeInfo().IsInterface);
            if (interfaceType == null)
            {
                var baseType = services.FirstOrDefault(x => x.GetTypeInfo().IsClass) ?? limitType;
                proxyType = proxyTypeGenerator.CreateClassProxyType(baseType, limitType);
                var activator = new ReflectionActivator(proxyType, new DefaultConstructorFinder(type => type.GetTypeInfo().DeclaredConstructors.ToArray()), new MostParametersConstructorSelector(), new AParameter[0], new AParameter[0]);
                instance = activator.ActivateInstance(e.Context, e.Parameters);
            }
            else
            {
                proxyType = proxyTypeGenerator.CreateInterfaceProxyType(interfaceType, limitType);
                instance = Activator.CreateInstance(proxyType, new object[] { e.Context.Resolve<IAspectActivatorFactory>(), e.Instance });
            }

            var propertyInjector = e.Context.Resolve<IPropertyInjectorFactory>().Create(instance.GetType());
            propertyInjector.Invoke(instance);
            e.Instance = instance;
            e.Component.RaiseActivating(e.Context, e.Parameters, ref instance);
        }

        #endregion
    }
}