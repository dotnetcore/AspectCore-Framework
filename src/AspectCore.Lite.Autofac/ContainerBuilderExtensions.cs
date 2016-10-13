using AspectCore.Lite.Generators;
using Autofac;
using Autofac.Core;
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
        public static ContainerBuilder RegisterAspectLite(this ContainerBuilder containerBuilder , IEnumerable<ServiceDescriptor> aspectServices)
        {
            if (containerBuilder == null)
            {
                throw new ArgumentNullException(nameof(containerBuilder));
            }

            containerBuilder.Populate(aspectServices);
            containerBuilder.RegisterCallback(registry => registry.Registered += Registry_Registered);

            return containerBuilder;
        }

        private static void Registry_Registered(object sender , ComponentRegisteredEventArgs args)
        {
            args.ComponentRegistration.Activating += ComponentRegistration_Activating;
        }

        private static void ComponentRegistration_Activating(object sender , ActivatingEventArgs<object> args)
        {
            var serviceProvider = args.Context.Resolve<IServiceProvider>();
            var serviceTypes = args.Component.Services.OfType<IServiceWithType>().Select(s => s.ServiceType);
            var interfaceTypes = serviceTypes.Where(type => type.GetTypeInfo().IsInterface);
            var classCount = serviceTypes.Count(type => type.GetTypeInfo().IsClass);
            if (classCount == 0)
            {

                var interfaceProxyGenerator = new InterfaceProxyGenerator(serviceProvider , interfaceTypes.First() , interfaceTypes.Skip(1).ToArray());
                var proxyType = interfaceProxyGenerator.GenerateProxyType();
                var proxyInstance = ActivatorUtilities.CreateInstance(serviceProvider , proxyType , serviceProvider , args.Instance);
                args.Instance = proxyInstance;
            }
        }
    }
}
