using System.Threading.Tasks;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 触发执行拦截管道
    /// </summary>
    [NonAspect]
    public interface IAspectActivator
    {
        /// <summary>
        /// 同步执行拦截管道
        /// </summary>
        /// <typeparam name="TResult">返回值的类型</typeparam>
        /// <param name="activatorContext">切面上下文</param>
        /// <returns>返回的值</returns>
        TResult Invoke<TResult>(AspectActivatorContext activatorContext);

        /// <summary>
        /// 异步执行拦截管道
        /// </summary>
        /// <typeparam name="TResult">结果类型</typeparam>
        /// <param name="activatorContext">切面上下文</param>
        /// <returns>异步结果</returns>
        Task<TResult> InvokeTask<TResult>(AspectActivatorContext activatorContext);

        /// <summary>
        /// 异步执行拦截管道
        /// </summary>
        /// <typeparam name="TResult">结果类型</typeparam>
        /// <param name="activatorContext">切面上下文</param>
        /// <returns>异步结果</returns>
        ValueTask<TResult> InvokeValueTask<TResult>(AspectActivatorContext activatorContext);
    }
}