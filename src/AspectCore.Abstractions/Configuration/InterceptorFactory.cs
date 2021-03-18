using System;
using System.Reflection;
using AspectCore.DynamicProxy;

namespace AspectCore.Configuration
{
    /// <summary>
    /// 拦截器工厂的抽象基类
    /// </summary>
    public abstract class InterceptorFactory
    {
        private static readonly AspectPredicate[] EmptyPredicates = new AspectPredicate[0];
        private readonly AspectPredicate[] _predicates;

        /// <summary>
        /// 拦截条件数组
        /// </summary>
        public AspectPredicate[] Predicates
        {
            get
            {
                return _predicates;
            }
        }

        /// <summary>
        /// 构造拦截器工厂
        /// </summary>
        /// <param name="predicates">拦截条件数组</param>
        public InterceptorFactory(params AspectPredicate[] predicates)
        {
            _predicates = predicates ?? EmptyPredicates;
        }

        /// <summary>
        /// 是否可以创建针对method进行拦截的拦截器工厂对象，如果有一个拦截条件通过则返回true
        /// </summary>
        /// <param name="method">待拦截的方法</param>
        /// <returns>是否可以创建</returns>
        public bool CanCreated(MethodInfo method)
        {
            if (_predicates.Length == 0)
            {
                return true;
            }
            foreach (var predicate in _predicates)
            {
                if (predicate(method)) return true;
            }
            return false;
        }

        /// <summary>
        /// 创建拦截器
        /// </summary>
        /// <param name="serviceProvider">服务容器</param>
        /// <returns>拦截器</returns>
        public abstract IInterceptor CreateInstance(IServiceProvider serviceProvider);
    }
}
