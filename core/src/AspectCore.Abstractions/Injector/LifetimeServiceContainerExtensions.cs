using System;

namespace AspectCore.Injector
{
    public static class LifetimeServiceContainerExtensions
    {
        public static ILifetimeServiceContainer AddType(this ILifetimeServiceContainer lifetimeServiceContainer, Type serviceType)
        {
            return AddType(lifetimeServiceContainer, serviceType, serviceType);
        }

        public static ILifetimeServiceContainer AddType(this ILifetimeServiceContainer lifetimeServiceContainer, Type serviceType, Type implementationType)
        {
            if (lifetimeServiceContainer == null)
            {
                throw new ArgumentNullException(nameof(lifetimeServiceContainer));
            }
            lifetimeServiceContainer.Add(new TypeServiceDefinition(serviceType, implementationType, lifetimeServiceContainer.Lifetime));
            return lifetimeServiceContainer;
        }

        public static ILifetimeServiceContainer AddType<TService>(this ILifetimeServiceContainer lifetimeServiceContainer)
        {
            return AddType(lifetimeServiceContainer, typeof(TService), typeof(TService));
        }

        public static ILifetimeServiceContainer AddType<TService, TImplementation>(this ILifetimeServiceContainer lifetimeServiceContainer)
            where TImplementation : TService
        {
            if (lifetimeServiceContainer == null)
            {
                throw new ArgumentNullException(nameof(lifetimeServiceContainer));
            }
            lifetimeServiceContainer.Add(new TypeServiceDefinition(typeof(TService), typeof(TImplementation), lifetimeServiceContainer.Lifetime));
            return lifetimeServiceContainer;
        }

        public static ILifetimeServiceContainer AddInstance(this ILifetimeServiceContainer lifetimeServiceContainer, Type serviceType, object implementationInstance)
        {
            lifetimeServiceContainer.Add(new InstanceServiceDefinition(serviceType, implementationInstance));
            return lifetimeServiceContainer;
        }

        public static ILifetimeServiceContainer AddInstance<TService>(this ILifetimeServiceContainer lifetimeServiceContainer, TService implementationInstance)
        {
            lifetimeServiceContainer.Add(new InstanceServiceDefinition(typeof(TService), implementationInstance));
            return lifetimeServiceContainer;
        }

        public static ILifetimeServiceContainer AddDelegate(this ILifetimeServiceContainer lifetimeServiceContainer, Type serviceType, Func<IServiceResolver, object> implementationDelegate)
        {
            lifetimeServiceContainer.Add(new DelegateServiceDefinition(serviceType, implementationDelegate, lifetimeServiceContainer.Lifetime));
            return lifetimeServiceContainer;
        }

        public static ILifetimeServiceContainer AddDelegate<TService, TImplementation>(this ILifetimeServiceContainer lifetimeServiceContainer, Func<IServiceResolver, TImplementation> implementationDelegate)
            where TService : class
            where TImplementation : class, TService
        {
            lifetimeServiceContainer.Add(new DelegateServiceDefinition(typeof(TService), implementationDelegate, lifetimeServiceContainer.Lifetime));
            return lifetimeServiceContainer;
        }

        public static ILifetimeServiceContainer AddDelegate<TService>(this ILifetimeServiceContainer lifetimeServiceContainer, Func<IServiceResolver, TService> implementationDelegate)
           where TService : class
        {
            lifetimeServiceContainer.Add(new DelegateServiceDefinition(typeof(TService), implementationDelegate, lifetimeServiceContainer.Lifetime));
            return lifetimeServiceContainer;
        }
    }
}