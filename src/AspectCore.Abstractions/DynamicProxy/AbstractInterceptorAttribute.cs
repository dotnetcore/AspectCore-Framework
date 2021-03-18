using System;
using System.Threading.Tasks;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 自定义的拦截特性可继承此抽象拦截特性类,重写Invoke方法实现具体拦截逻辑
    /// </summary>
    [NonAspect]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Interface, Inherited = false)]
    public abstract class AbstractInterceptorAttribute : Attribute, IInterceptor
    {
        /// <summary>
        /// 提供一个布尔值。如果为 true，则该特性可多次使用, false（单用的）。
        /// </summary>
        public virtual bool AllowMultiple { get; } = false;

        /// <summary>
        /// 排序号,用以指定拦截顺序
        /// </summary>
        public virtual int Order { get; set; } = 0;

        /// <summary>
        /// 是否可继承
        /// </summary>
        public bool Inherited { get; set; } = false;

        /// <summary>
        /// 增强的具体业务逻辑
        /// </summary>
        /// <param name="context">拦截上下文</param>
        /// <param name="next">后续处理拦截上下文的委托对象</param>
        /// <returns>异步任务</returns>
        public abstract Task Invoke(AspectContext context, AspectDelegate next);
    }
}