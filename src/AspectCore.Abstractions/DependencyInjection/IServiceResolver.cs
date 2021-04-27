using System;
using AspectCore.DynamicProxy;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 服务解析(类似IServiceProvider)
    /// </summary>
    [NonAspect]
    public interface IServiceResolver : IServiceProvider, IDisposable
    {
        /// <summary>
        /// 通过服务类型从容器中获取对象
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <returns>获取到的对象</returns>
        object Resolve(Type serviceType);
    }
}