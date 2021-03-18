using System;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 生成代理类型
    /// </summary>
    [NonAspect]
    public interface IProxyTypeGenerator
    {
        /// <summary>
        /// 创建接口代理类型
        /// </summary>
        /// <param name="serviceType">暴露的服务类型</param>
        /// <returns>由接口代理方式实现的代理类的类型</returns>
        Type CreateInterfaceProxyType(Type serviceType);

        /// <summary>
        /// 创建接口代理类型
        /// </summary>
        /// <param name="serviceType">暴露的服务类型</param>
        /// <param name="implementationType">实现类型</param>
        /// <returns>由接口代理方式实现的代理类的类型</returns>
        Type CreateInterfaceProxyType(Type serviceType, Type implementationType);

        /// <summary>
        /// 通过子类代理方式创建代理类型
        /// </summary>
        /// <param name="serviceType">暴露的服务类型</param>
        /// <param name="implementationType">实现类型</param>
        /// <returns>由子类方式实现的代理类的类型</returns>
        Type CreateClassProxyType(Type serviceType, Type implementationType);
    }
}