using System;
using System.Collections.Generic;
using System.Text;

namespace AspectCore.DependencyInjection
{
    public static class ServiceProviderExtensions
    {
        public static T Resolve<T>(this IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }
            if(serviceProvider is IServiceResolver resolver)
            {
                return resolver.Resolve<T>();
            }
            resolver = serviceProvider.GetService(typeof(IServiceResolver)) as IServiceResolver;
            return resolver.Resolve<T>();
        }

        public static T ResolveRequired<T>(this IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }
            if (serviceProvider is IServiceResolver resolver)
            {
                return resolver.ResolveRequired<T>();
            }
            resolver = serviceProvider.GetService(typeof(IServiceResolver)) as IServiceResolver;
            return resolver.ResolveRequired<T>();
        }

        public static IEnumerable<T> ResolveMany<T>(this IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }
            if (serviceProvider is IServiceResolver resolver)
            {
                return resolver.ResolveMany<T>();
            }
            resolver = serviceProvider.GetService(typeof(IServiceResolver)) as IServiceResolver;
            return resolver.ResolveMany<T>();
        }


    }
}
