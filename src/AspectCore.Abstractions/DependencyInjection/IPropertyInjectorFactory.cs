using System;
using AspectCore.DynamicProxy;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 生产IPropertyInjector对象的工厂
    /// </summary>
    [NonAspect]
    public interface IPropertyInjectorFactory
    {
        /// <summary>
        /// 创建一个具有属性注入功能的对象
        /// </summary>
        /// <param name="implementationType">操作的对象</param>
        /// <returns>IPropertyInjector</returns>
        IPropertyInjector Create(Type implementationType);
    }
}