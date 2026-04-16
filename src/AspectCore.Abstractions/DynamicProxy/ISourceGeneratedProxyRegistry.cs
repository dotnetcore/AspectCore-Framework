using System;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 由 Source Generator 生成的 registry，用于从 (serviceType, implType, kind) 查找 proxy Type。
    /// </summary>
    public interface ISourceGeneratedProxyRegistry
    {
        /// <summary>
        /// implementationType 在 interface proxy 无 target 场景可传 null。
        /// </summary>
        bool TryGetProxyType(Type serviceType, Type implementationType, SourceGeneratedProxyKind kind, out Type proxyType);
    }
}
