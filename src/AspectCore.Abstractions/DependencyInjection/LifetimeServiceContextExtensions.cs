using System;

namespace AspectCore.DependencyInjection
{
    public static class LifetimeServiceContextExtensions
    {
        public static ILifetimeServiceContext AddType(this ILifetimeServiceContext lifetimeServiceContext, Type serviceType)
        {
            return AddType(lifetimeServiceContext, serviceType, serviceType);
        }

        public static ILifetimeServiceContext AddType(this ILifetimeServiceContext lifetimeServiceContext, Type serviceType, Type implementationType)
        {
            if (lifetimeServiceContext == null)
            {
                throw new ArgumentNullException(nameof(lifetimeServiceContext));
            }
            lifetimeServiceContext.Add(new TypeServiceDefinition(serviceType, implementationType, lifetimeServiceContext.Lifetime));
            return lifetimeServiceContext;
        }

        public static ILifetimeServiceContext AddType<TService>(this ILifetimeServiceContext lifetimeServiceContext)
        {
            return AddType(lifetimeServiceContext, typeof(TService), typeof(TService));
        }

        public static ILifetimeServiceContext AddType<TService, TImplementation>(this ILifetimeServiceContext lifetimeServiceContext)
            where TImplementation : TService
        {
            if (lifetimeServiceContext == null)
            {
                throw new ArgumentNullException(nameof(lifetimeServiceContext));
            }
            lifetimeServiceContext.Add(new TypeServiceDefinition(typeof(TService), typeof(TImplementation), lifetimeServiceContext.Lifetime));
            return lifetimeServiceContext;
        }

        public static ILifetimeServiceContext AddInstance(this ILifetimeServiceContext lifetimeServiceContext, Type serviceType, object implementationInstance)
        {
            lifetimeServiceContext.Add(new InstanceServiceDefinition(serviceType, implementationInstance));
            return lifetimeServiceContext;
        }

        public static ILifetimeServiceContext AddInstance<TService>(this ILifetimeServiceContext lifetimeServiceContext, TService implementationInstance)
        {
            lifetimeServiceContext.Add(new InstanceServiceDefinition(typeof(TService), implementationInstance));
            return lifetimeServiceContext;
        }

        public static ILifetimeServiceContext AddDelegate(this ILifetimeServiceContext lifetimeServiceContext, Type serviceType, Func<IServiceResolver, object> implementationDelegate)
        {
            lifetimeServiceContext.Add(new DelegateServiceDefinition(serviceType, implementationDelegate, lifetimeServiceContext.Lifetime));
            return lifetimeServiceContext;
        }

        public static ILifetimeServiceContext AddDelegate<TService, TImplementation>(this ILifetimeServiceContext lifetimeServiceContext, Func<IServiceResolver, TImplementation> implementationDelegate)
            where TService : class
            where TImplementation : class, TService
        {
            lifetimeServiceContext.Add(new DelegateServiceDefinition(typeof(TService), implementationDelegate, lifetimeServiceContext.Lifetime));
            return lifetimeServiceContext;
        }

        public static ILifetimeServiceContext AddDelegate<TService>(this ILifetimeServiceContext lifetimeServiceContext, Func<IServiceResolver, TService> implementationDelegate)
           where TService : class
        {
            lifetimeServiceContext.Add(new DelegateServiceDefinition(typeof(TService), implementationDelegate, lifetimeServiceContext.Lifetime));
            return lifetimeServiceContext;
        }
    }
}