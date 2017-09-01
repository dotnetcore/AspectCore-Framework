using System;
using AspectCore.Abstractions;

namespace AspectCore.Core.Injector
{
    public static class ServiceResolverExtensions
    {
        public static object Resolve(this IServiceResolver serviceResolver, Type serviceType)
        {
            return serviceResolver?.Resolve(serviceType);
        }

        public static T Resolve<T>(this IServiceResolver serviceResolver)
        {
            return (T)serviceResolver?.Resolve(typeof(T));
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
    }
}