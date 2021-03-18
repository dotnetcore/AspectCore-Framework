using System;
using AspectCore.DynamicProxy;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 服务解析回调接口
    /// </summary>
    [NonAspect, NonCallback]
    public interface IServiceResolveCallback
    {
        /// <summary>
        /// 提供instance实例的服务解析器和服务描述对象，以使你在方法中执行相关回调处理逻辑
        /// </summary>
        /// <param name="resolver">instance实例的服务解析器</param>
        /// <param name="instance">实例</param>
        /// <param name="service">instance实例的服务描述对象</param>
        /// <returns>回调处理后产生的结果对象</returns>
        object Invoke(IServiceResolver resolver, object instance, ServiceDefinition service);
    }

    /// <summary>
    /// 标注此特性代表不执行回调处理
    /// </summary>
    public sealed class NonCallback : Attribute
    {
    }
}