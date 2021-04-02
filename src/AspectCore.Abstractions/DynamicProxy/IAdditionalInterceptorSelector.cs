using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 用于查询继承树上相关的拦截器
    /// </summary>
    [NonAspect]
    public interface IAdditionalInterceptorSelector
    {
        /// <summary>
        /// 查询继承树上相关的拦截器
        /// </summary>
        /// <param name="serviceMethod">暴露的服务方法</param>
        /// <param name="implementationMethod">目标方法</param>
        /// <returns>拦截器集合</returns>
        IEnumerable<IInterceptor> Select(MethodInfo serviceMethod, MethodInfo implementationMethod);
    }
}
