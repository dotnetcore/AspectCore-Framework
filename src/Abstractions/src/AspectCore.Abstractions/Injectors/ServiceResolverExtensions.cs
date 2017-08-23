using System;

namespace AspectCore.Abstractions
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
    }
}
