using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 拦截器收集器,提供方法获取服务和实例上关联的所有拦截器
    /// </summary>
    [NonAspect]
    public interface IInterceptorCollector
    {
        /// <summary>
        /// 获取服务和实例上关联的所有拦截器
        /// </summary>
        /// <param name="serviceMethod">服务方法</param>
        /// <param name="implementationMethod">目标方法</param>
        /// <returns>拦截器集合</returns>
        IEnumerable<IInterceptor> Collect(MethodInfo serviceMethod, MethodInfo implementationMethod);
    }
}
