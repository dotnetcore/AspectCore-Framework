using System;
using AspectCore.DynamicProxy;

namespace AspectCore.DependencyInjection
{
    [NonAspect, NonCallback]
    public interface IServiceResolveCallback
    {
        /// <summary>
        /// 提供实例的服务解析器和服务描述对象，以使你在方法中执行相关回调处理逻辑
        /// </summary>
        /// <param name="resolver">IServiceResolver</param>
        /// <param name="instance">实例</param>
        /// <param name="service">实例的服务描述对象</param>
        /// <returns>回调处理后的结果</returns>
        object Invoke(IServiceResolver resolver, object instance, ServiceDefinition service);
    }

    /// <summary>
    /// 标注以不执行回调
    /// </summary>
    public sealed class NonCallback : Attribute
    {
    }
}