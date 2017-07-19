using System;
using System.Reflection;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.Configuration
{
    public static class InterceptorFactoryCollectionExtensions
    {
        public static InterceptorFactoryCollection AddTyped(this AspectCoreOptions options, Type interceptorType, params Predicate<MethodInfo>[] predicates)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }
            if (interceptorType == null)
            {
                throw new ArgumentNullException(nameof(interceptorType));
            }
            collection.Add(new TypedInterceptorFactory(interceptorType));
            return collection;
        }

        public static InterceptorFactoryCollection AddTyped(this InterceptorFactoryCollection collection, Type interceptorType, object[] args, params Predicate<MethodInfo>[] predicates)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }
            if (predicates == null)
            {
                throw new ArgumentNullException(nameof(predicates));
            }
            if (interceptorType == null)
            {
                throw new ArgumentNullException(nameof(interceptorType));
            }
            collection.Add(new TypedInterceptorFactory(interceptorType, args, predicates));
            return collection;
        }

        public static InterceptorFactoryCollection AddTyped<TInterceptor>(this InterceptorFactoryCollection collection, params Predicate<MethodInfo>[] predicates)
           where TInterceptor : IInterceptor
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }
            return AddTyped(collection, typeof(TInterceptor), predicates);
        }

        public static InterceptorFactoryCollection AddTyped<TInterceptor>(this InterceptorFactoryCollection collection, object[] args, params Predicate<MethodInfo>[] predicates)
            where TInterceptor : IInterceptor
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }
            if (predicates == null)
            {
                throw new ArgumentNullException(nameof(predicates));
            }
            return AddTyped(collection, typeof(TInterceptor), args, predicates);
        }

        public static InterceptorFactoryCollection AddServiced(this InterceptorFactoryCollection collection, Type interceptorType, params Predicate<MethodInfo>[] predicates)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }
            collection.Add(new ServiceInterceptorFactory(interceptorType, predicates));
            return collection;
        }

        public static InterceptorFactoryCollection AddServiced<TInterceptor>(this InterceptorFactoryCollection collection, params Predicate<MethodInfo>[] predicates)
            where TInterceptor : IInterceptor
        {
            return AddServiced(collection, typeof(TInterceptor), predicates);
        }

    }
}