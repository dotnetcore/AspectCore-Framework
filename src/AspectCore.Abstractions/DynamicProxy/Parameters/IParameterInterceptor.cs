using System.Threading.Tasks;

namespace AspectCore.DynamicProxy.Parameters
{
    /// <summary>
    /// 参数拦截器接口,拦截方法参数做增强逻辑
    /// </summary>
    [NonAspect]
    public interface IParameterInterceptor
    {
        /// <summary>
        /// 具体的增强逻辑在此方法中实现
        /// </summary>
        /// <param name="context">参数拦截上下文</param>
        /// <param name="next">后续的参数拦截委托</param>
        /// <returns>异步任务</returns>
        Task Invoke(ParameterAspectContext context, ParameterAspectDelegate next);
    }
}