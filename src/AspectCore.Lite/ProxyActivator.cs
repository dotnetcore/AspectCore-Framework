using AspectCore.Lite.Common;
using AspectCore.Lite.DependencyInjection;
using AspectCore.Lite.Extensions;
using AspectCore.Lite.Internal.Generators;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace AspectCore.Lite.Abstractions.Activators
{
    public class ProxyActivator : IProxyActivator
    {
        private readonly IServiceProvider serviceProvider;

        public ProxyActivator()
            : this(ServiceCollectionHelper.CreateAspectLiteServices().BuildServiceProvider())
        {
        }

        public ProxyActivator(IServiceProvider serviceProvider)
        {
            ExceptionHelper.ThrowArgumentNull(serviceProvider , nameof(serviceProvider));
            this.serviceProvider = serviceProvider;
        }

        public object CreateClassProxy(Type serviceType , object instance , params Type[] interfaceTypes)
        {
            ExceptionHelper.ThrowArgumentNull(serviceType , nameof(serviceType));
            ExceptionHelper.ThrowArgumentNull(instance , nameof(instance));
            ExceptionHelper.ThrowArgument(() => !serviceType.GetTypeInfo().IsAssignableFrom(instance.GetType()) , $"Can not assign an instance of the {instance.GetType()} to the {serviceType}.");
            interfaceTypes?.ForEach(interfaceType => ExceptionHelper.ThrowArgument(() => !interfaceType.GetTypeInfo().IsAssignableFrom(instance.GetType()) , $"Can not assign an instance of the {instance.GetType()} to the {interfaceType}."));

            var proxyGenerator = new ClassProxyGenerator(serviceProvider , serviceType , interfaceTypes);
            var proxyType = proxyGenerator.GenerateProxyType();
            return ActivatorUtilities.CreateInstance(serviceProvider , proxyType , new object[] { serviceProvider , instance });
        }

        public object CreateInterfaceProxy(Type serviceType , object instance , params Type[] interfaceTypes)
        {
            ExceptionHelper.ThrowArgumentNull(serviceType , nameof(serviceType));
            ExceptionHelper.ThrowArgumentNull(instance , nameof(instance));
            ExceptionHelper.ThrowArgument(() => !serviceType.GetTypeInfo().IsAssignableFrom(instance.GetType()) , $"Can not assign an instance of the {instance.GetType()} to the {serviceType}.");
            interfaceTypes?.ForEach(interfaceType => ExceptionHelper.ThrowArgument(() => !interfaceType.GetTypeInfo().IsAssignableFrom(instance.GetType()) , $"Can not assign an instance of the {instance.GetType()} to the {interfaceType}."));

            var proxyGenerator = new InterfaceProxyGenerator(serviceProvider , serviceType , interfaceTypes);
            var proxyType = proxyGenerator.GenerateProxyType();
            return ActivatorUtilities.CreateInstance(serviceProvider , proxyType , new object[] { serviceProvider , instance });
        }
    }
}
