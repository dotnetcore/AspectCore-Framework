using System;

namespace AspectCore.Injector
{
    public static class ServiceContainerExtensions
    {
        public static IServiceContainer AddType(this IServiceContainer serviceContainer, Type serviceType, Lifetime lifetime = Lifetime.Transient)
        {
            return AddType(serviceContainer, serviceType, serviceType, lifetime);
        }

        public static IServiceContainer AddType(this IServiceContainer serviceContainer, Type serviceType, Type implementationType, Lifetime lifetime = Lifetime.Transient)
        {
            if (serviceContainer == null)
            {
                throw new ArgumentNullException(nameof(serviceContainer));
            }
            serviceContainer.Add(new TypeServiceDefinition(serviceType, implementationType, lifetime));
            return serviceContainer;
        }

        public static IServiceContainer AddType<TService>(this IServiceContainer serviceContainer, Lifetime lifetime = Lifetime.Transient)
        {
            return AddType(serviceContainer, typeof(TService), lifetime);
        }

        public static IServiceContainer AddType<TService, TImplementation>(this IServiceContainer serviceContainer, Lifetime lifetime = Lifetime.Transient)
            where TImplementation : TService
        {
            return AddType(serviceContainer, typeof(TService), typeof(TImplementation), lifetime);
        }

        public static IServiceContainer AddInstance(this IServiceContainer serviceContainer, Type serviceType, object implementationInstance)
        {
            serviceContainer.Add(new InstanceServiceDefinition(serviceType, implementationInstance));
            return serviceContainer;
        }

        public static IServiceContainer AddInstance<TService>(this IServiceContainer serviceContainer, TService implementationInstance)
        {
            serviceContainer.Add(new InstanceServiceDefinition(typeof(TService), implementationInstance));
            return serviceContainer;
        }

        public static IServiceContainer AddDelegate(this IServiceContainer serviceContainer, Type serviceType, Func<IServiceResolver, object> implementationDelegate, Lifetime lifetime = Lifetime.Transient)
        {
            serviceContainer.Add(new DelegateServiceDefinition(serviceType, implementationDelegate, lifetime));
            return serviceContainer;
        }

        public static IServiceContainer AddDelegate<TService>(this IServiceContainer serviceContainer, Func<IServiceResolver, TService> implementationDelegate, Lifetime lifetime = Lifetime.Transient)
            where TService : class
        {
            serviceContainer.Add(new DelegateServiceDefinition(typeof(TService), implementationDelegate, lifetime));
            return serviceContainer;
        }
    }
}
