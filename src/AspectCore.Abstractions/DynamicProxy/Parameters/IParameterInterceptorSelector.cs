using System.Reflection;

namespace AspectCore.DynamicProxy.Parameters
{
    /// <summary>
    /// 参数拦截器查询器接口
    /// </summary>
    public interface IParameterInterceptorSelector
    {
        /// <summary>
        /// 查询参数parameter关联的参数拦截器
        /// </summary>
        /// <param name="parameter">参数</param>
        /// <returns>关联的参数拦截器数组</returns>
        IParameterInterceptor[] Select(ParameterInfo parameter);
    }
}