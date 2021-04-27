using System;
using System.Threading.Tasks;

namespace AspectCore.DynamicProxy.Parameters
{
    /// <summary>
    /// 标注在返回值上,则拦截器会对返回值进行拦截处理
    /// </summary>
    [AttributeUsage(AttributeTargets.ReturnValue, AllowMultiple = false, Inherited = false)]
    public abstract class ReturnParameterInterceptorAttribute : Attribute, IParameterInterceptor
    {
        /// <summary>
        /// 拦截返回值后，进行如何处理的逻辑
        /// </summary>
        /// <param name="context">参数拦截上下文</param>
        /// <param name="next">后续的参数拦截委托</param>
        /// <returns>异步任务</returns>
        public abstract Task Invoke(ParameterAspectContext context, ParameterAspectDelegate next);
    }
}