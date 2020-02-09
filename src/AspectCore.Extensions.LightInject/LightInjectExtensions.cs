using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using LightInject;

namespace AspectCore.Extensions.LightInject
{
    internal static class LightInjectExtensions
    {
        public static IServiceRegistry AddSingleton<T, TImpl>(this IServiceRegistry services, string name = default)
            where T : class
            where TImpl : class, T
        {
            return services.Register<T, TImpl>(name ?? string.Empty, new PerContainerLifetime());
        }

        public static IServiceRegistry AddSingleton<T>(this IServiceRegistry services, T instance)
            where T : class
        {
            return services.RegisterInstance<T>(instance);
        }

        public static IServiceRegistry AddTransient(this IServiceRegistry services, Type service, Type impl)
        {
            // The default behavior in LightInject is to treat all objects as transients unless otherwise specified.
            return services.Register(service, impl);
        }
    }
}
