using System;
using Autofac;
using AspectCore.Configuration;
using Autofac.Core;
using AspectCore.DynamicProxy;
using Autofac.Core.Activators.Reflection;
using AutofacNamespace = Autofac;

namespace AspectCore.Extensions.Autofac
{
    #region
    //public static class ContainerBuilderExtensions
    //{
    //    public static ContainerBuilder RegisterAspectCore(this ContainerBuilder builder)
    //    {
    //        return RegisterAspectCore(builder, null);
    //    }

    //    public static ContainerBuilder RegisterAspectCore(this ContainerBuilder builder, Action<AutofacOptions> options)
    //    {
    //        if (builder == null)
    //        {
    //            throw new ArgumentNullException(nameof(builder));
    //        }

    //        builder.RegisterType<AutofacServiceProvider>().As<IServiceProvider>();
    //        builder.RegisterType<AutofacRealServiceProvider>().As<IRealServiceProvider>().InstancePerDependency();

    //        builder.RegisterType<AspectActivator>().As<IAspectActivator>().InstancePerDependency();
    //        builder.RegisterType<AspectBuilderFactory>().As<IAspectBuilderFactory>().InstancePerDependency();
    //        builder.RegisterType<ProxyGenerator>().As<IProxyGenerator>().InstancePerDependency();

    //        builder.RegisterType<AspectContextFactory>().As<IAspectContextFactory>().InstancePerDependency();
    //        builder.RegisterType<AspectValidatorBuilder>().As<IAspectValidatorBuilder>().InstancePerDependency();
    //        builder.RegisterType<AccessibleAspectValidationHandler>().As<IAspectValidationHandler>().InstancePerDependency();
    //        builder.RegisterType<AttributeAspectValidationHandler>().As<IAspectValidationHandler>().InstancePerDependency();
    //        builder.RegisterType<CacheAspectValidationHandler>().As<IAspectValidationHandler>().InstancePerDependency();
    //        builder.RegisterType<ConfigureAspectValidationHandler>().As<IAspectValidationHandler>().InstancePerDependency();
    //        builder.RegisterType<DynamicallyAspectValidationHandler>().As<IAspectValidationHandler>().InstancePerDependency();
    //        builder.RegisterType<NonAspectValidationHandler>().As<IAspectValidationHandler>().InstancePerDependency();

    //        builder.RegisterType<InterceptorProvider>().As<IInterceptorProvider>().InstancePerDependency();
    //        builder.RegisterType<ConfigureInterceptorSelector>().As<IInterceptorSelector>().InstancePerDependency();
    //        builder.RegisterType<TypeInterceptorSelector>().As<IInterceptorSelector>().InstancePerDependency();
    //        builder.RegisterType<MethodInterceptorSelector>().As<IInterceptorSelector>().InstancePerDependency();

    //        builder.RegisterType<ReflectionInterceptorActivator>().As<IInterceptorActivator>().InstancePerDependency();

    //        builder.RegisterType<InterceptorInjectorProvider>().As<IInterceptorInjectorProvider>().InstancePerDependency();
    //        builder.RegisterType<PropertyInjectorSelector>().As<IPropertyInjectorSelector>().InstancePerDependency();

    //        var aspectCoreOptions = new AutofacOptions();
    //        options?.Invoke(aspectCoreOptions);
    //        builder.RegisterInstance<IAspectConfigureProvider>(new AspectConfigureProvider(aspectCoreOptions.InterceptorFactories, aspectCoreOptions.NonAspectPredicates)).SingleInstance();

    //        return builder;
    //    }
    //}
    #endregion


    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder RegistorDynamicProxy(this ContainerBuilder containerBuilder, Action<IAspectConfiguration> configure)
        {
            containerBuilder.RegisterCallback(registry =>
            {
                foreach (var registration in registry.Registrations)
                {
                    registration.Activating += ComponentRegistration_Activating;
                }
                registry.Registered += Registry_Registered;
            });
            return containerBuilder;
        }

        private static void Registry_Registered(object sender, ComponentRegisteredEventArgs e)
        {
            e.ComponentRegistration.Activating += ComponentRegistration_Activating;
        }

        private static void ComponentRegistration_Activating(object sender, ActivatingEventArgs<object> e)
        {
            var limitType = e.Component.Activator.LimitType;
            var aspectValidator = e.Context.Resolve<IAspectValidatorBuilder>().Build();
            if (!aspectValidator.Validate(limitType) || e.Instance.IsProxy())
            {
                return;
            }
            var proxyTypeGenerator = e.Context.Resolve<IProxyTypeGenerator>();
            var proxyType = proxyTypeGenerator.CreateClassProxyType(limitType, limitType);
            var activator = new ReflectionActivator(proxyType, new DefaultConstructorFinder(), new MostParametersConstructorSelector(), new Parameter[0], new Parameter[0]);
            
            //new DefaultConstructorFinder()
        }
    }
}