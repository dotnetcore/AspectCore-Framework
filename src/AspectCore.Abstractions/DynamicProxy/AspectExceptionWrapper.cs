using System;
using AspectCore.Configuration;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 拦截异常包装器
    /// </summary>
    [NonAspect]
    public class AspectExceptionWrapper : IAspectExceptionWrapper
    {
        private readonly IAspectConfiguration _configuration;

        /// <summary>
        /// 构造拦截异常包装器
        /// </summary>
        /// <param name="configuration">拦截配置</param>
        public AspectExceptionWrapper(IAspectConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// 包装异常对象
        /// </summary>
        /// <param name="aspectContext">拦截上下文</param>
        /// <param name="exception">异常对象</param>
        /// <returns>包装后的异常对象</returns>
        public Exception Wrap(AspectContext aspectContext, Exception exception)
        {
            if (!_configuration.ThrowAspectException)
            {
                return exception;
            }

            if (exception is AspectInvocationException aspectInvocationException)
            {
                return aspectInvocationException;
            }

            return new AspectInvocationException(aspectContext, exception);
        }
    }
}