using System;
using System.Threading.Tasks;

namespace AspectCore.DynamicProxy.Parameters
{
    /// <summary>
    /// 参数拦截特性,如果参数为null则抛出异常
    /// </summary>
    public class NotNullAttribute : ParameterInterceptorAttribute
    {
        /// <summary>
        /// 异常消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 拦截参数后，进行如何处理的逻辑
        /// </summary>
        /// <param name="context">参数拦截上下文</param>
        /// <param name="next">后续的参数拦截委托</param>
        /// <returns>异步任务</returns>
        public override Task Invoke(ParameterAspectContext context, ParameterAspectDelegate next)
        {
            if (context.Parameter.Value == null)
            {
                throw new ArgumentNullException(context.Parameter.Name, Message);
            }
            return next(context);
        }
    }
}