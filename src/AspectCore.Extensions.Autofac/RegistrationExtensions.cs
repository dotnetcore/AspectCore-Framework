using System;
using System.Collections.Generic;
using System.Reflection;
using AspectCore.DependencyInjection;
using Autofac;
using Autofac.Builder;

namespace AspectCore.Extensions.Autofac
{
    public static class RegistrationExtensions
    {
        public static void Populate(
              this ContainerBuilder builder,
              IEnumerable<ServiceDefinition> services)
        {
            builder.RegisterType<AutofacServiceResolver>().As<IServiceProvider, IServiceResolver>().InstancePerLifetimeScope();
            builder.RegisterType<AutofacScopeResolverFactory>().As<IScopeResolverFactory>().InstancePerLifetimeScope();

            Register(builder, services);
        }

        private static void Register(ContainerBuilder builder, IEnumerable<ServiceDefinition> services)
        {
            foreach (var service in services)
            {
                if (service is TypeServiceDefinition typeService)
                {
                    var serviceTypeInfo = typeService.ServiceType.GetTypeInfo();
                    if (serviceTypeInfo.IsGenericTypeDefinition)
                    {
                        builder
                            .RegisterGeneric(typeService.ImplementationType)
                            .As(typeService.ServiceType)
                            .ConfigureLifecycle(typeService.Lifetime);
                    }
                    else
                    {
                        builder
                            .RegisterType(typeService.ImplementationType)
                            .As(typeService.ServiceType)
                            .ConfigureLifecycle(typeService.Lifetime);
                    }
                }
                else if (service is DelegateServiceDefinition delegateService)
                {
                    var registration = RegistrationBuilder.ForDelegate(delegateService.ServiceType, (context, parameters) =>
                    {
                        var resolver = context.Resolve<IServiceResolver>();
                        return delegateService.ImplementationDelegate(resolver);
                    })
                    .ConfigureLifecycle(delegateService.Lifetime)
                    .CreateRegistration();

                    builder.RegisterComponent(registration);
                }
                else
                {
                    if (service is InstanceServiceDefinition instanceService)
                    {
                        builder.
                            RegisterInstance(instanceService.ImplementationInstance).
                            As(instanceService.ServiceType).
                            ConfigureLifecycle(instanceService.Lifetime);
                    }
                }
            }
        }

        private static IRegistrationBuilder<object, TActivatorData, TRegistrationStyle> ConfigureLifecycle<TActivatorData, TRegistrationStyle>(
               this IRegistrationBuilder<object, TActivatorData, TRegistrationStyle> registrationBuilder,
               Lifetime lifecycleKind)
        {
            switch (lifecycleKind)
            {
                case Lifetime.Singleton:
                    registrationBuilder.SingleInstance();
                    break;
                case Lifetime.Scoped:
                    registrationBuilder.InstancePerLifetimeScope();
                    break;
                case Lifetime.Transient:
                    registrationBuilder.InstancePerDependency();
                    break;
            }

            return registrationBuilder;
        }
    }
}
