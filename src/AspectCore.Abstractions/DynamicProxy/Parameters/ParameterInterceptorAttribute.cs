using System;
using System.Threading.Tasks;

namespace AspectCore.DynamicProxy.Parameters
{
    /// <summary>
    /// 标注在参数上，则拦截器会对参数进行拦截处理
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public abstract class ParameterInterceptorAttribute : Attribute, IParameterInterceptor
    {
        /// <summary>
        /// 拦截参数后，进行如何处理的逻辑
        /// </summary>
        /// <param name="context">参数拦截上下文</param>
        /// <param name="next">后续的参数拦截委托</param>
        /// <returns>异步任务</returns>
        public abstract Task Invoke(ParameterAspectContext context, ParameterAspectDelegate next);
    }
}