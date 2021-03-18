using System;
using System.Reflection;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 通过暴露的服务类型，实现类型，生存期来构造服务描述对象
    /// </summary>
    public sealed class TypeServiceDefinition : ServiceDefinition
    {
        /// <summary>
        /// 通过暴露的服务类型，实现类型，生存期来构造服务描述对象
        /// </summary>
        /// <param name="serviceType">暴露的服务类型</param>
        /// <param name="implementationType">实现类型</param>
        /// <param name="lifetime">生命周期</param>
        public TypeServiceDefinition(Type serviceType, Type implementationType, Lifetime lifetime) : base(serviceType, lifetime)
        {
            ImplementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
        }

        /// <summary>
        /// 实现类型
        /// </summary>
        public Type ImplementationType { get; }
    }
}