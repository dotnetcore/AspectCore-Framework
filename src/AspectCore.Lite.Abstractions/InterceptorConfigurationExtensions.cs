using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions
{
    public static class InterceptorConfigurationExtensions
    {
        public static IInterceptorConfiguration Configure(this IInterceptorConfiguration configruation, Type interceptorType, params object[] args)
        {
            if (configruation == null)
            {
                throw new ArgumentNullException(nameof(configruation));
            }
            return configruation.ConfigureIf(m => true, interceptorType, args);
        }

        public static IInterceptorConfiguration Configure<TInterceptor>(this IInterceptorConfiguration configruation, params object[] args)
            where TInterceptor : IInterceptor
        {
            if (configruation == null)
            {
                throw new ArgumentNullException(nameof(configruation));
            }
            configruation.Configure(typeof(TInterceptor), args);
            return configruation;
        }

        public static IInterceptorConfiguration ConfigureIf(this IInterceptorConfiguration configruation, Func<MethodInfo, bool> predicate, Type interceptorType, params object[] args)
        {
            if (configruation == null)
            {
                throw new ArgumentNullException(nameof(configruation));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (interceptorType == null)
            {
                throw new ArgumentNullException(nameof(interceptorType));
            }

            if (!typeof(IInterceptor).GetTypeInfo().IsAssignableFrom(interceptorType))
            {
                throw new ArgumentException($"{interceptorType} not an interceptor type.", nameof(interceptorType));
            }

            configruation.Configure(method => predicate(method) ? (IInterceptor)Activator.CreateInstance(interceptorType, args) : default(IInterceptor));
            return configruation;
        }

        public static IInterceptorConfiguration ConfigureIf(this IInterceptorConfiguration configruation, Func<MethodInfo, TypeInfo, bool> predicate, Type interceptorType, params object[] args)
        {
            if (configruation == null)
            {
                throw new ArgumentNullException(nameof(configruation));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return configruation.ConfigureIf(method => predicate(method, method.DeclaringType.GetTypeInfo()), interceptorType, args);
        }

        public static IInterceptorConfiguration ConfigureIf<TInterceptor>(this IInterceptorConfiguration configruation, Func<MethodInfo, bool> predicate, params object[] args)
            where TInterceptor : IInterceptor
        {
            if (configruation == null)
            {
                throw new ArgumentNullException(nameof(configruation));
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            configruation.ConfigureIf(predicate, typeof(TInterceptor), args);
            return configruation;
        }

        public static IInterceptorConfiguration ConfigureIf<TInterceptor>(this IInterceptorConfiguration configruation, Func<MethodInfo, TypeInfo, bool> predicate, params object[] args)
            where TInterceptor : IInterceptor
        {
            if (configruation == null)
            {
                throw new ArgumentNullException(nameof(configruation));
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            configruation.ConfigureIf(predicate, typeof(TInterceptor), args);
            return configruation;
        }
    }
}
