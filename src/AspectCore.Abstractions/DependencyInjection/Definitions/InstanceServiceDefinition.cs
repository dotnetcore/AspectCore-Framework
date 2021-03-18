using System;
using System.Reflection;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 实例对象的服务描述对象
    /// </summary>
    public sealed class InstanceServiceDefinition : ServiceDefinition
    {
        /// <summary>
        /// 构造表示实例类型的服务描述对象的实例
        /// </summary>
        /// <param name="serviceType">暴露的服务类型</param>
        /// <param name="implementationInstance">实例</param>
        public InstanceServiceDefinition(Type serviceType, object implementationInstance) : base(serviceType, Lifetime.Singleton)
        {
            ImplementationInstance = implementationInstance ?? throw new ArgumentNullException(nameof(implementationInstance));
        }

        /// <summary>
        /// 此对象关注的实例
        /// </summary>
        public object ImplementationInstance { get; }
    }
}