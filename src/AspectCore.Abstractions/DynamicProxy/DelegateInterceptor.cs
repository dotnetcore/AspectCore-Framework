using System;
using System.Threading.Tasks;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 委托类型的拦截器
    /// </summary>
    [NonAspect]
    public class DelegateInterceptor : IInterceptor
    {
        /// <summary>
        /// 拦截处理中间件
        /// </summary>
        private readonly Func<AspectDelegate, AspectDelegate> _aspectDelegate;

        /// <summary>
        /// 是否可多使用
        /// </summary>
        public bool AllowMultiple => true;

        /// <summary>
        /// 是否可继承
        /// </summary>
        public bool Inherited { get; set; } = false;

        /// <summary>
        /// 排序号,标识拦截顺序
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 构造一个委托类型的拦截器
        /// </summary>
        /// <param name="aspectDelegate">拦截处理中间件</param>
        /// <param name="order">排序号,标识拦截顺序</param>
        public DelegateInterceptor(Func<AspectDelegate, AspectDelegate> aspectDelegate, int order = 0)
        {
            _aspectDelegate = aspectDelegate ?? throw new ArgumentNullException(nameof(aspectDelegate));
            Order = order;
        }

        /// <summary>
        /// 执行拦截管道中的拦截委托
        /// </summary>
        /// <param name="context">拦截上下文</param>
        /// <param name="next">后续的处理拦截上下文的委托对象</param>
        /// <returns>异步任务</returns>
        public Task Invoke(AspectContext context, AspectDelegate next)
        {
            return _aspectDelegate(next)(context);
        }
    }
}