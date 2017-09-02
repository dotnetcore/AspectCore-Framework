using System;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;

namespace AspectCore.Configuration
{
    public static class InterceptorCollectionExtensions
    {
        public static InterceptorCollection AddTyped(this InterceptorCollection interceptorCollection, Type interceptorType, params AspectPredicate[] predicates)
        {
            if (interceptorCollection == null)
            {
                throw new ArgumentNullException(nameof(interceptorCollection));
            }
            if (interceptorType == null)
            {
                throw new ArgumentNullException(nameof(interceptorType));
            }
            interceptorCollection.Add(new TypeInterceptorFactory(interceptorType, null, predicates));
            return interceptorCollection;
        }

        public static InterceptorCollection AddTyped(this InterceptorCollection interceptorCollection, Type interceptorType, object[] args, params AspectPredicate[] predicates)
        {
            if (interceptorCollection == null)
            {
                throw new ArgumentNullException(nameof(interceptorCollection));
            }
            if (predicates == null)
            {
                throw new ArgumentNullException(nameof(predicates));
            }
            if (interceptorType == null)
            {
                throw new ArgumentNullException(nameof(interceptorType));
            }
            interceptorCollection.Add(new TypeInterceptorFactory(interceptorType, args, predicates));
            return interceptorCollection;
        }

        public static InterceptorCollection AddTyped<TInterceptor>(this InterceptorCollection interceptorCollection, params AspectPredicate[] predicates)
           where TInterceptor : IInterceptor
        {
            if (interceptorCollection == null)
            {
                throw new ArgumentNullException(nameof(interceptorCollection));
            }
            return AddTyped(interceptorCollection, typeof(TInterceptor), predicates);
        }

        public static InterceptorCollection AddTyped<TInterceptor>(this InterceptorCollection interceptorCollection, object[] args, params AspectPredicate[] predicates)
            where TInterceptor : IInterceptor
        {
            if (interceptorCollection == null)
            {
                throw new ArgumentNullException(nameof(interceptorCollection));
            }
            if (predicates == null)
            {
                throw new ArgumentNullException(nameof(predicates));
            }
            return AddTyped(interceptorCollection, typeof(TInterceptor), args, predicates);
        }

        public static InterceptorCollection AddServiced(this InterceptorCollection interceptorCollection, Type interceptorType, params AspectPredicate[] predicates)
        {
            if (interceptorCollection == null)
            {
                throw new ArgumentNullException(nameof(interceptorCollection));
            }
            interceptorCollection.Add(new ServiceInterceptorFactory(interceptorType, predicates));
            return interceptorCollection;
        }

        public static InterceptorCollection AddServiced<TInterceptor>(this InterceptorCollection interceptorCollection, params AspectPredicate[] predicates)
            where TInterceptor : IInterceptor
        {
            return AddServiced(interceptorCollection, typeof(TInterceptor), predicates);
        }

        public static InterceptorCollection AddDelegate(this InterceptorCollection interceptorCollection, Func<AspectDelegate, AspectDelegate> aspectDelegate, int order, params AspectPredicate[] predicates)
        {
            if (interceptorCollection == null)
            {
                throw new ArgumentNullException(nameof(interceptorCollection));
            }

            interceptorCollection.Add(new DelegateInterceptorFactory(aspectDelegate, order, predicates));

            return interceptorCollection;
        }

        public static InterceptorCollection AddDelegate(this InterceptorCollection interceptorCollection, Func<AspectDelegate, AspectDelegate> aspectDelegate, params AspectPredicate[] predicates)
        {
            return AddDelegate(interceptorCollection, aspectDelegate, 0, predicates);
        }

        public static InterceptorCollection AddDelegate(this InterceptorCollection interceptorCollection, Func<AspectContext, AspectDelegate, Task> aspectDelegate, int order, params AspectPredicate[] predicates)
        {
            return AddDelegate(interceptorCollection, next => context => aspectDelegate(context, next), order, predicates);
        }

        public static InterceptorCollection AddDelegate(this InterceptorCollection interceptorCollection, Func<AspectContext, AspectDelegate, Task> aspectDelegate, params AspectPredicate[] predicates)
        {
            return AddDelegate(interceptorCollection, aspectDelegate, 0, predicates);
        }
    }
}