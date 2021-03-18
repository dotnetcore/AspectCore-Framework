using System;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 拦截异常包装器接口
    /// </summary>
    public interface IAspectExceptionWrapper
    {
        /// <summary>
        /// 包装异常对象
        /// </summary>
        /// <param name="aspectContext">拦截上下文</param>
        /// <param name="exception">异常对象</param>
        /// <returns>包装后的异常对象</returns>
        Exception Wrap(AspectContext aspectContext, Exception exception);
    }
}