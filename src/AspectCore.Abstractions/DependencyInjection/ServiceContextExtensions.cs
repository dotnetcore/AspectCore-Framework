using System;
using System.Collections.Generic;

namespace AspectCore.DependencyInjection
{
    public static class ServiceContextExtensions
    {
        public static IServiceContext AddType(this IServiceContext serviceContext, Type serviceType, Lifetime lifetime = Lifetime.Transient)
        {
            return AddType(serviceContext, serviceType, serviceType, lifetime);
        }

        public static IServiceContext AddType(this IServiceContext serviceContext, Type serviceType, Type implementationType, Lifetime lifetime = Lifetime.Transient)
        {
            if (serviceContext == null)
            {
                throw new ArgumentNullException(nameof(serviceContext));
            }
            serviceContext.Add(new TypeServiceDefinition(serviceType, implementationType, lifetime));
            return serviceContext;
        }

        public static IServiceContext AddType<TService>(this IServiceContext serviceContext, Lifetime lifetime = Lifetime.Transient)
        {
            return AddType(serviceContext, typeof(TService), lifetime);
        }

        public static IServiceContext AddType<TService, TImplementation>(this IServiceContext serviceContext, Lifetime lifetime = Lifetime.Transient)
            where TImplementation : TService
        {
            return AddType(serviceContext, typeof(TService), typeof(TImplementation), lifetime);
        }

        public static IServiceContext AddInstance(this IServiceContext serviceContext, Type serviceType, object implementationInstance)
        {
            serviceContext.Add(new InstanceServiceDefinition(serviceType, implementationInstance));
            return serviceContext;
        }

        public static IServiceContext AddInstance<TService>(this IServiceContext serviceContext, TService implementationInstance)
        {
            serviceContext.Add(new InstanceServiceDefinition(typeof(TService), implementationInstance));
            return serviceContext;
        }

        public static IServiceContext AddDelegate(this IServiceContext serviceContext, Type serviceType, Func<IServiceResolver, object> implementationDelegate, Lifetime lifetime = Lifetime.Transient)
        {
            serviceContext.Add(new DelegateServiceDefinition(serviceType, implementationDelegate, lifetime));
            return serviceContext;
        }

        public static IServiceContext AddDelegate<TService, TImplementation>(this IServiceContext serviceContext, Func<IServiceResolver, TImplementation> implementationDelegate, Lifetime lifetime = Lifetime.Transient)
            where TService : class
            where TImplementation : class, TService
        {
            serviceContext.Add(new DelegateServiceDefinition(typeof(TService), implementationDelegate, lifetime));
            return serviceContext;
        }

        public static IServiceContext AddDelegate<TService>(this IServiceContext serviceContext, Func<IServiceResolver, TService> implementationDelegate, Lifetime lifetime = Lifetime.Transient)
           where TService : class
        {
            serviceContext.Add(new DelegateServiceDefinition(typeof(TService), implementationDelegate, lifetime));
            return serviceContext;
        }

        public static IServiceContext RemoveAll<TService>(this IServiceContext serviceContext) where TService : class
        {
            return RemoveAll(serviceContext, typeof(TService));
        }

        public static IServiceContext RemoveAll(this IServiceContext serviceContext, Type serviceType)
        {
            var serviceDefinitions = new List<ServiceDefinition>();
            foreach (var serviceDefinition in serviceContext)
            {
                if (serviceDefinition.ServiceType == serviceType)
                {
                    serviceDefinitions.Add(serviceDefinition);
                }
            }

            serviceDefinitions.ForEach(t => serviceContext.Remove(t));
            return serviceContext;
        }

    }
}
