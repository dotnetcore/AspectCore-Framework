using System;
using AspectCore.DynamicProxy;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 此工厂接口的实现类用于创建具有属性注入功能的对象
    /// </summary>
    [NonAspect]
    public interface IPropertyInjectorFactory
    {
        /// <summary>
        /// 创建一个具有属性注入功能的对象
        /// </summary>
        /// <param name="implementationType">针对此类型属性注入</param>
        /// <returns>具有属性注入功能的对象</returns>
        IPropertyInjector Create(Type implementationType);
    }
}