using System;

namespace AspectCore.Injector
{
    public static class ServiceResolverExtensions
    {
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

        public static object ResolveRequired(this IServiceResolver serviceResolver, Type serviceType)
        {
            return null;
        }
    }
}