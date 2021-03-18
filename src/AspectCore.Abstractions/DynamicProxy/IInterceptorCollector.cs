using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 提供方法获取服务和实例上关联的拦截器
    /// </summary>
    [NonAspect]
    public interface IInterceptorCollector
    {
        /// <summary>
        /// 获取服务和实例上关联的拦截器
        /// </summary>
        /// <param name="serviceMethod">服务方法</param>
        /// <param name="implementationMethod">实现方法</param>
        /// <returns>获取到的拦截器</returns>
        IEnumerable<IInterceptor> Collect(MethodInfo serviceMethod, MethodInfo implementationMethod);
    }
}
