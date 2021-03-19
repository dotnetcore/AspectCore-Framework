using System;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;

namespace AspectCore.Configuration
{
    /// <summary>
    /// InterceptorCollection的扩展
    /// </summary>
    public static class InterceptorCollectionExtensions
    {
        /// <summary>
        /// 添加拦截器工厂
        /// </summary>
        /// <param name="interceptorCollection">拦截器工厂集合对象</param>
        /// <param name="interceptorType">拦截器工厂类型</param>
        /// <param name="predicates">一些拦截条件</param>
        /// <returns>拦截器工厂集合对象</returns>
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

        /// <summary>
        /// 添加拦截器工厂
        /// </summary>
        /// <param name="interceptorCollection">拦截器工厂集合对象</param>
        /// <param name="interceptorType">拦截器类型</param>
        /// <param name="args">拦截器构造参数</param>
        /// <param name="predicates">一些拦截条件</param>
        /// <returns>拦截器工厂集合对象</returns>
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

        /// <summary>
        /// 添加拦截器
        /// </summary>
        /// <typeparam name="TInterceptor">拦截器类型</typeparam>
        /// <param name="interceptorCollection">拦截器工厂集合对象</param>
        /// <param name="predicates">一些拦截条件</param>
        /// <returns>拦截器工厂集合对象</returns>
        public static InterceptorCollection AddTyped<TInterceptor>(this InterceptorCollection interceptorCollection, params AspectPredicate[] predicates)
           where TInterceptor : IInterceptor
        {
            if (interceptorCollection == null)
            {
                throw new ArgumentNullException(nameof(interceptorCollection));
            }
            return AddTyped(interceptorCollection, typeof(TInterceptor), predicates);
        }

        /// <summary>
        /// 添加拦截器
        /// </summary>
        /// <typeparam name="TInterceptor">拦截器类型</typeparam>
        /// <param name="interceptorCollection">拦截器工厂集合对象</param>
        /// <param name="args">拦截器的构造参数</param>
        /// <param name="predicates">一些拦截条件</param>
        /// <returns>拦截器工厂集合对象</returns>
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

        /// <summary>
        /// 添加拦截器服务
        /// </summary>
        /// <param name="interceptorCollection">拦截器工厂集合对象</param>
        /// <param name="interceptorType">拦截器类型</param>
        /// <param name="predicates">一些拦截条件</param>
        /// <returns>拦截器工厂集合对象</returns>
        public static InterceptorCollection AddServiced(this InterceptorCollection interceptorCollection, Type interceptorType, params AspectPredicate[] predicates)
        {
            if (interceptorCollection == null)
            {
                throw new ArgumentNullException(nameof(interceptorCollection));
            }
            interceptorCollection.Add(new ServiceInterceptorFactory(interceptorType, predicates));
            return interceptorCollection;
        }

        /// <summary>
        /// 添加拦截器服务
        /// </summary>
        /// <typeparam name="TInterceptor">拦截器类型</typeparam>
        /// <param name="interceptorCollection">拦截器工厂集合对象</param>
        /// <param name="predicates">一些拦截条件</param>
        /// <returns>拦截器工厂集合对象</returns>
        public static InterceptorCollection AddServiced<TInterceptor>(this InterceptorCollection interceptorCollection, params AspectPredicate[] predicates)
            where TInterceptor : IInterceptor
        {
            return AddServiced(interceptorCollection, typeof(TInterceptor), predicates);
        }

        /// <summary>
        /// 添加拦截器服务到拦截管道中
        /// </summary>
        /// <param name="interceptorCollection">拦截器工厂集合对象</param>
        /// <param name="aspectDelegate">处理拦截的aspect中间件</param>
        /// <param name="order">排序号</param>
        /// <param name="predicates">一些拦截条件</param>
        /// <returns>拦截器工厂集合对象</returns>
        public static InterceptorCollection AddDelegate(this InterceptorCollection interceptorCollection, Func<AspectDelegate, AspectDelegate> aspectDelegate, int order, params AspectPredicate[] predicates)
        {
            if (interceptorCollection == null)
            {
                throw new ArgumentNullException(nameof(interceptorCollection));
            }

            interceptorCollection.Add(new DelegateInterceptorFactory(aspectDelegate, order, predicates));

            return interceptorCollection;
        }

        /// <summary>
        /// 添加拦截器服务到拦截管道中
        /// </summary>
        /// <param name="interceptorCollection">拦截器工厂集合对象</param>
        /// <param name="aspectDelegate">处理拦截的aspect中间件</param>
        /// <param name="predicates">一些拦截条件</param>
        /// <returns>拦截器工厂集合对象</returns>
        public static InterceptorCollection AddDelegate(this InterceptorCollection interceptorCollection, Func<AspectDelegate, AspectDelegate> aspectDelegate, params AspectPredicate[] predicates)
        {
            return AddDelegate(interceptorCollection, aspectDelegate, 0, predicates);
        }

        /// <summary>
        /// 添加拦截器服务到拦截管道中
        /// </summary>
        /// <param name="interceptorCollection">拦截器工厂集合对象</param>
        /// <param name="aspectDelegate">处理aspect上下文，并返回异步任务的委托，处理拦截的过程将组合后续拦截器的处理过程。
        ///     <summary>
        ///     func委托的泛型参数说明:<br/> 
        ///     1.AspectContext 上下文对象<br/>
        ///     2.AspectDelegate 后续的拦截器组成的处理aspect上下文的委托
        ///     </summary>
        /// </param>
        /// <param name="order">排序号</param>
        /// <param name="predicates">一些拦截条件</param>
        /// <returns>拦截器工厂集合对象</returns>
        public static InterceptorCollection AddDelegate(this InterceptorCollection interceptorCollection, Func<AspectContext, AspectDelegate, Task> aspectDelegate, int order, params AspectPredicate[] predicates)
        {
            return AddDelegate(interceptorCollection, next => context => aspectDelegate(context, next), order, predicates);
        }

        /// <summary>
        /// 添加拦截器服务到拦截管道中
        /// </summary>
        /// <param name="interceptorCollection">拦截器工厂集合对象</param>
        /// <param name="aspectDelegate">处理aspect上下文，并返回异步任务的委托，处理拦截的过程将组合后续拦截器的处理过程。</param>
        /// <param name="predicates">一些拦截条件</param>
        /// <returns>拦截器工厂集合对象</returns>
        public static InterceptorCollection AddDelegate(this InterceptorCollection interceptorCollection, Func<AspectContext, AspectDelegate, Task> aspectDelegate, params AspectPredicate[] predicates)
        {
            return AddDelegate(interceptorCollection, aspectDelegate, 0, predicates);
        }
    }
}