using System;
using AspectCore.Abstractions;
using AspectCore.Extensions.IoC.Definitions;

namespace AspectCore.Extensions.IoC
{
    public static class LifetimeServiceContainerExtensions
    {
        public static ILifetimeServiceContainer AddType(this ILifetimeServiceContainer lifetimeServiceContainer, Type serviceType, Type implementationType, object key)
        {
            lifetimeServiceContainer.Add(new TypeServiceDefinition(serviceType, implementationType, lifetimeServiceContainer.Lifetime, key));
            return lifetimeServiceContainer;
        }

        public static ILifetimeServiceContainer AddInstance(this ILifetimeServiceContainer lifetimeServiceContainer, Type serviceType, object implementationInstance, object key)
        {
            lifetimeServiceContainer.Add(new InstanceServiceDefinition(serviceType, implementationInstance, lifetimeServiceContainer.Lifetime, key));
            return lifetimeServiceContainer;
        }

        public static ILifetimeServiceContainer AddDelegate(this ILifetimeServiceContainer lifetimeServiceContainer, Type serviceType, Func<IServiceResolver, object> implementationDelegate, object key)
        {
            lifetimeServiceContainer.Add(new DelegateServiceDefinition(serviceType, implementationDelegate, lifetimeServiceContainer.Lifetime, key));
            return lifetimeServiceContainer;
        }
    }
}