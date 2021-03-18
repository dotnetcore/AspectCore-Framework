using System;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 拦截调用异常,此异常信息中包含拦截上下文信息
    /// </summary>
    public class AspectInvocationException : Exception
    {
        /// <summary>
        /// 拦截上下文
        /// </summary>
        public AspectContext AspectContext { get; }

        /// <summary>
        /// 构造拦截调用异常
        /// </summary>
        /// <param name="aspectContext">拦截上下文</param>
        /// <param name="innerException">内部异常</param>
        public AspectInvocationException(AspectContext aspectContext, Exception innerException)
            : base($"Exception has been thrown by the aspect of an invocation. ---> {innerException?.Message}.", innerException)
        {
            AspectContext = aspectContext;
        }
    }
}