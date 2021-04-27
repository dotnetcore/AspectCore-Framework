using System;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 拦截异常包装器接口
    /// </summary>
    public interface IAspectExceptionWrapper
    {
         /// <summary>
        /// 包装异常为拦截异常
        /// </summary>
        /// <param name="aspectContext">拦截上下文</param>
        /// <param name="exception">异常</param>
        /// <returns>包装后的异常对象</returns>
        Exception Wrap(AspectContext aspectContext, Exception exception);
    }
}