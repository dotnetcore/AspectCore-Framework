using System;
using AspectCore.DynamicProxy;

namespace AspectCore.Configuration
{
    /// <summary>
    /// 此工厂创建DelegateInterceptor类型的拦截器
    /// </summary>
    public sealed class DelegateInterceptorFactory : InterceptorFactory
    {
        private readonly Func<AspectDelegate, AspectDelegate> _aspectDelegate;
        private readonly int _order;

        /// <summary>
        /// 构造委托类型拦截器工厂
        /// </summary>
        /// <param name="aspectDelegate">拦截器中间件</param>
        /// <param name="order">排序号</param>
        /// <param name="predicates">拦截条件数组（注：为CanCreated方法提供支持以判断是否可以创建此拦截器）</param>
        public DelegateInterceptorFactory(Func<AspectDelegate, AspectDelegate> aspectDelegate, int order, params AspectPredicate[] predicates)
            : base(predicates)
        {
            _aspectDelegate = aspectDelegate ?? throw new ArgumentNullException(nameof(aspectDelegate));
            _order = order;
        }

        /// <summary>
        /// 创建一个委托类型拦截器
        /// </summary>
        /// <param name="serviceProvider">服务提供者</param>
        /// <returns>创建的拦截器</returns>
        public override IInterceptor CreateInstance(IServiceProvider serviceProvider)
        {
            return new DelegateInterceptor(_aspectDelegate, _order);
        }
    }
}
