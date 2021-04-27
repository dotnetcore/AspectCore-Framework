using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 拦截器的查询器
    /// </summary>
    [NonAspect]
    public interface IInterceptorSelector
    {
        /// <summary>
        /// 通过方法查询所关联的拦截器
        /// </summary>
        /// <param name="method">待查询的方法</param>
        /// <returns>查询到的拦截器</returns>
        IEnumerable<IInterceptor> Select(MethodInfo method);
    }
}