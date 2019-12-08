using System;
using System.Collections.Generic;

namespace AspectCore.DependencyInjection
{
    public static class ServiceResolverExtensions
    {
        public static T Resolve<T>(this IServiceResolver serviceResolver)
        {
            if (serviceResolver == null)
            {
                throw new ArgumentNullException(nameof(serviceResolver));
            }
            return (T)serviceResolver.Resolve(typeof(T));
        }

        public static IServiceResolver CreateScope(this IServiceResolver serviceResolver)
        {
            if (serviceResolver == null)
            {
                throw new ArgumentNullException(nameof(serviceResolver));
            }
            var factory = serviceResolver.Resolve<IScopeResolverFactory>();
            return factory.CreateScope();
        }

        public static object ResolveRequired(this IServiceResolver serviceResolver, Type serviceType)
        {
            if (serviceResolver == null)
            {
                throw new ArgumentNullException(nameof(serviceResolver));
            }
            var instance = serviceResolver.Resolve(serviceType);
            if (instance == null)
            {
                throw new InvalidOperationException($"No instance for type '{serviceType}' has been resolved.");
            }
            return instance;
        }

        public static T ResolveRequired<T>(this IServiceResolver serviceResolver)
        {
            if (serviceResolver == null)
            {
                throw new ArgumentNullException(nameof(serviceResolver));
            }
            return (T)serviceResolver.ResolveRequired(typeof(T));
        }

        public static IEnumerable<object> ResolveMany(this IServiceResolver serviceResolver, Type serviceType)
        {
            if (serviceResolver == null)
            {
                throw new ArgumentNullException(nameof(serviceResolver));
            }

            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            var genericEnumerable = typeof(IManyEnumerable<>).MakeGenericType(serviceType);
            return (IManyEnumerable<object>)serviceResolver.ResolveRequired(genericEnumerable);
        }

        public static IEnumerable<T> ResolveMany<T>(this IServiceResolver serviceResolver)
        {
            if (serviceResolver == null)
            {
                throw new ArgumentNullException(nameof(serviceResolver));
            }

            return serviceResolver.ResolveRequired<IManyEnumerable<T>>();
        }
    }
}