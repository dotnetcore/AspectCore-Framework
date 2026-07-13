using System;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 触发 AspectCore Source Generator 生成代理。
    /// 
    /// 本节点支持：标注在 class/interface 上的无参形式。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public sealed class AspectCoreGenerateProxyAttribute : Attribute
    {
        public AspectCoreGenerateProxyAttribute() { }

        public AspectCoreGenerateProxyAttribute(Type serviceType, Type implementationType, SourceGeneratedProxyKind kind)
        {
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            ImplementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
            Kind = kind;
        }

        /// <summary>
        /// 用于 interface proxy with target 场景，指定实现类型。
        /// </summary>
        public AspectCoreGenerateProxyAttribute(Type implementationType)
        {
            ImplementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
        }

        // 无参构造用于 type-level 触发：此时不会携带 mapping 信息。
        public Type ServiceType { get; } = null;
        public Type ImplementationType { get; } = null;
        public SourceGeneratedProxyKind? Kind { get; }
    }
}
