using System;

namespace AspectCore.Abstractions
{
    public static class LifetimeServiceContainerExtensions
    {

        public static ILifetimeServiceContainer AddType(this ILifetimeServiceContainer lifetimeServiceContainer, Type serviceType, object key = null)
        {
            return AddType(lifetimeServiceContainer, serviceType, serviceType, key);
        }

        public static ILifetimeServiceContainer AddType(this ILifetimeServiceContainer lifetimeServiceContainer, Type serviceType, Type implementationType, object key = null)
        {
            if (lifetimeServiceContainer == null)
            {
                throw new ArgumentNullException(nameof(lifetimeServiceContainer));
            }
            lifetimeServiceContainer.Add(new TypeServiceDefinition(serviceType, implementationType, lifetimeServiceContainer.Lifetime, key));
            return lifetimeServiceContainer;
        }

        public static ILifetimeServiceContainer AddType<TService>(this ILifetimeServiceContainer lifetimeServiceContainer, object key = null)
        {
            return AddType(lifetimeServiceContainer, typeof(TService), typeof(TService), key);
        }

        public static ILifetimeServiceContainer AddType<TService, TImplementation>(this ILifetimeServiceContainer lifetimeServiceContainer, object key = null)
            where TImplementation : TService
        {
            if (lifetimeServiceContainer == null)
            {
                throw new ArgumentNullException(nameof(lifetimeServiceContainer));
            }
            lifetimeServiceContainer.Add(new TypeServiceDefinition(typeof(TService), typeof(TImplementation), lifetimeServiceContainer.Lifetime, key));
            return lifetimeServiceContainer;
        }

        public static ILifetimeServiceContainer AddInstance(this ILifetimeServiceContainer lifetimeServiceContainer, Type serviceType, object implementationInstance, object key = null)
        {
            lifetimeServiceContainer.Add(new InstanceServiceDefinition(serviceType, implementationInstance, key));
            return lifetimeServiceContainer;
        }

        public static ILifetimeServiceContainer AddInstance<TService>(this ILifetimeServiceContainer lifetimeServiceContainer, TService implementationInstance, object key = null)
        {
            lifetimeServiceContainer.Add(new InstanceServiceDefinition(typeof(TService), implementationInstance, key));
            return lifetimeServiceContainer;
        }

        public static ILifetimeServiceContainer AddDelegate(this ILifetimeServiceContainer lifetimeServiceContainer, Type serviceType, Func<IServiceResolver, object> implementationDelegate, object key = null)
        {
            lifetimeServiceContainer.Add(new DelegateServiceDefinition(serviceType, implementationDelegate, lifetimeServiceContainer.Lifetime, key));
            return lifetimeServiceContainer;
        }

        public static ILifetimeServiceContainer AddDelegate<TService>(this ILifetimeServiceContainer lifetimeServiceContainer, Func<IServiceResolver, TService> implementationDelegate, object key = null)
            where TService : class
        {
            lifetimeServiceContainer.Add(new DelegateServiceDefinition(typeof(TService), implementationDelegate, lifetimeServiceContainer.Lifetime, key));
            return lifetimeServiceContainer;
        }
    }
}