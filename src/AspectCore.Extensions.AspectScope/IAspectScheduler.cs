using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.AspectScope
{
    /// <summary>
    /// 具有作用域范围的拦截上下文的调度器
    /// </summary>
    [NonAspect]
    public interface IAspectScheduler
    {
        /// <summary>
        /// 尝试进入作用域范围
        /// </summary>
        /// <param name="context">拦截作用域</param>
        /// <returns>是否进入作用域范围</returns>
        bool TryEnter(AspectContext context);

        /// <summary>
        /// 释放作用域
        /// </summary>
        /// <param name="context">拦截上下文</param>
        void Release(AspectContext context);

        bool TryRelate(AspectContext context, IInterceptor interceptor);

        AspectContext[] GetCurrentContexts();
    }
}
