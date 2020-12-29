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
using Autofac.Core.Activators;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Resolving.Pipeline;
using AParameter = Autofac.Core.Parameter;

namespace AspectCore.Extensions.Autofac
{
    public static class ContainerBuilderExtensions
    {
        #region RegisterDynamicProxy

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

            //全局注册中间件
            containerBuilder.ComponentRegistryBuilder.Registered += (sender, args) =>
            {
                // The PipelineBuilding event fires just before the pipeline is built, and
                // middleware can be added inside it.
                args.ComponentRegistration.PipelineBuilding += (_, pipeline) =>
                {
                    pipeline.Use(ActivationResolveMiddleware.Instance,MiddlewareInsertionMode.StartOfPhase);
                };
            };

            return containerBuilder;
        }
        #endregion
    }
}