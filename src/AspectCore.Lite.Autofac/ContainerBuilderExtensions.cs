using AspectCore.Lite.DependencyInjection;
using AspectCore.Lite.Extensions;
using AspectCore.Lite.Generators;
using Autofac;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Lite.Autofac
{
    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder RegisterAspectLite(this ContainerBuilder containerBuilder)
        {
            if (containerBuilder == null)
            {
                throw new ArgumentNullException(nameof(containerBuilder));
            }

            containerBuilder.Populate(ServiceCollectionHelper.CreateAspectLiteServices());
            containerBuilder.RegisterCallback(registry => registry.Registered += Registry_Registered);

            return containerBuilder;
        }

        private static void Registry_Registered(object sender , ComponentRegisteredEventArgs args)
        {
            args.ComponentRegistration.Activating += ComponentRegistration_Activating;
        }

        private static void ComponentRegistration_Activating(object sender, ActivatingEventArgs<object> args)
        {
            if (!(args.Component.Activator is ReflectionActivator))
            {
                return;
            }

            var serviceProvider = args.Context.Resolve<IServiceProvider>();

            var serviceTypes = args.Component.Services.OfType<IServiceWithType>().Select(s => s.ServiceType);
            if (serviceTypes.All(t => !t.GetTypeInfo().CanProxy(serviceProvider)))
            {
                return;
            }

           
            var interfaceTypes = serviceTypes.Where(type => type.GetTypeInfo().IsInterface);
            var classCount = serviceTypes.Count(type => type.GetTypeInfo().IsClass);
            var instanceType = args.Instance.GetType();
            Type proxyType = null;

            switch (classCount)
            {
                case 0:
                    var interfaceProxyGenerator = new InterfaceProxyGenerator(serviceProvider, interfaceTypes.First(), interfaceTypes.Skip(1).ToArray());
                    proxyType = interfaceProxyGenerator.GenerateProxyType();
                    break;

                case 1:
                    var serviceType = serviceTypes.Single(type => type.GetTypeInfo().IsClass);
                    if (!serviceType.GetTypeInfo().IsAssignableFrom(instanceType))
                    {
                        throw new InvalidOperationException($"Not found base type of {instanceType} in registered services.");
                    }   
                    var classProxyGenerator = new ClassProxyGenerator(serviceProvider, serviceType, interfaceTypes.ToArray());
                    proxyType = classProxyGenerator.GenerateProxyType();
                    break;

                default:
                    var canProxyTypes = serviceTypes.Where(type => type.GetTypeInfo().IsClass && type.GetTypeInfo().CanProxy(serviceProvider)).ToArray();
                    if (canProxyTypes.Length != 1)
                    {
                        throw new InvalidOperationException($"Can not determine the {instanceType} type of inheritance in registered services.");
                    }
                    var parentProxyGenerator = new ClassProxyGenerator(serviceProvider, canProxyTypes[0], interfaceTypes.ToArray());
                    proxyType = parentProxyGenerator.GenerateProxyType();
                    break;
                    
            }

            var proxyInstance = ActivatorUtilities.CreateInstance(serviceProvider, proxyType, serviceProvider, args.Instance);
            args.Instance = proxyInstance;
        }
    }
}
