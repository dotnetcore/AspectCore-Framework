using System;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 提供方法，生成代理
    /// </summary>
    [NonAspect]
    public interface IProxyGenerator : IDisposable
    {
        IProxyTypeGenerator TypeGenerator { get; }

        /// <summary>
        /// 生成接口代理
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <returns>生成的代理对象</returns>
        object CreateInterfaceProxy(Type serviceType);

        /// <summary>
        /// 生成接口代理
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <param name="implementationInstance">实现实例</param>
        /// <returns>生成的代理对象</returns>
        object CreateInterfaceProxy(Type serviceType, object implementationInstance);

        /// <summary>
        /// 生成类代理（继承方式实现代理）
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <param name="implementationType">实现实例</param>
        /// <param name="args">构造器参数</param>
        /// <returns>代理对象</returns>
        object CreateClassProxy(Type serviceType, Type implementationType, object[] args);
    }
}