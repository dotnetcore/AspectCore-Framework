using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.Abstractions;

namespace AspectCore.Core.Configuration
{
    public static class InterceptorFactoriesExtensions
    {
        public static ICollection<IInterceptorFactory> AddTyped(this ICollection<IInterceptorFactory> interceptorFactories, Type interceptorType, params Func<MethodInfo, bool>[] predicates)
        {
            if (interceptorFactories == null)
            {
                throw new ArgumentNullException(nameof(interceptorFactories));
            }
            if (interceptorType == null)
            {
                throw new ArgumentNullException(nameof(interceptorType));
            }
            interceptorFactories.Add(new TypeInterceptorFactory(interceptorType, null, predicates));
            return interceptorFactories;
        }

        public static ICollection<IInterceptorFactory> AddTyped(this ICollection<IInterceptorFactory> interceptorFactories, Type interceptorType, object[] args, params Func<MethodInfo, bool>[] predicates)
        {
            if (interceptorFactories == null)
            {
                throw new ArgumentNullException(nameof(interceptorFactories));
            }
            if (predicates == null)
            {
                throw new ArgumentNullException(nameof(predicates));
            }
            if (interceptorType == null)
            {
                throw new ArgumentNullException(nameof(interceptorType));
            }
            interceptorFactories.Add(new TypeInterceptorFactory(interceptorType, args, predicates));
            return interceptorFactories;
        }

        public static ICollection<IInterceptorFactory> AddTyped<TInterceptor>(this ICollection<IInterceptorFactory> interceptorFactories, params Func<MethodInfo, bool>[] predicates)
           where TInterceptor : IInterceptor
        {
            if (interceptorFactories == null)
            {
                throw new ArgumentNullException(nameof(interceptorFactories));
            }
            return AddTyped(interceptorFactories, typeof(TInterceptor), predicates);
        }

        public static ICollection<IInterceptorFactory> AddTyped<TInterceptor>(this ICollection<IInterceptorFactory> interceptorFactories, object[] args, params Func<MethodInfo, bool>[] predicates)
            where TInterceptor : IInterceptor
        {
            if (interceptorFactories == null)
            {
                throw new ArgumentNullException(nameof(interceptorFactories));
            }
            if (predicates == null)
            {
                throw new ArgumentNullException(nameof(predicates));
            }
            return AddTyped(interceptorFactories, typeof(TInterceptor), args, predicates);
        }

        public static ICollection<IInterceptorFactory> AddServiced(this ICollection<IInterceptorFactory> interceptorFactories, Type interceptorType, params Func<MethodInfo, bool>[] predicates)
        {
            if (interceptorFactories == null)
            {
                throw new ArgumentNullException(nameof(interceptorFactories));
            }
            interceptorFactories.Add(new ServiceInterceptorFactory(interceptorType, predicates));
            return interceptorFactories;
        }

        public static ICollection<IInterceptorFactory> AddServiced<TInterceptor>(this ICollection<IInterceptorFactory> interceptorFactories, params Func<MethodInfo, bool>[] predicates)
            where TInterceptor : IInterceptor
        {
            return AddServiced(interceptorFactories, typeof(TInterceptor), predicates);
        }

        public static ICollection<IInterceptorFactory> AddDelegate(this ICollection<IInterceptorFactory> interceptorFactories, Func<AspectDelegate, AspectDelegate> aspectDelegate, int order, params Func<MethodInfo, bool>[] predicates)
        {
            if (interceptorFactories == null)
            {
                throw new ArgumentNullException(nameof(interceptorFactories));
            }

            interceptorFactories.Add(new DelegateInterceptorFactory(aspectDelegate, order, predicates));

            return interceptorFactories;
        }

        public static ICollection<IInterceptorFactory> AddDelegate(this ICollection<IInterceptorFactory> interceptorFactories, Func<AspectDelegate, AspectDelegate> aspectDelegate, params Func<MethodInfo, bool>[] predicates)
        {
            return AddDelegate(interceptorFactories, aspectDelegate, 0, predicates);
        }

        public static ICollection<IInterceptorFactory> AddDelegate(this ICollection<IInterceptorFactory> interceptorFactories, Func<AspectContext, AspectDelegate, Task> aspectDelegate, int order, params Func<MethodInfo, bool>[] predicates)
        {
            return AddDelegate(interceptorFactories, next => context => aspectDelegate(context, next), order, predicates);
        }

        public static ICollection<IInterceptorFactory> AddDelegate(this ICollection<IInterceptorFactory> interceptorFactories, Func<AspectContext, AspectDelegate, Task> aspectDelegate, params Func<MethodInfo, bool>[] predicates)
        {
            return AddDelegate(interceptorFactories, aspectDelegate, 0, predicates);
        }
    }
}