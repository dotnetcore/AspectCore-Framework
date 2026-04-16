using System;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 标记程序集包含 Source Generator 生成的 proxy registry。
    /// 运行时通过扫描该 attribute 进行 registry 发现。
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public sealed class AspectCoreSourceGeneratedProxyRegistryAttribute : Attribute
    {
        public AspectCoreSourceGeneratedProxyRegistryAttribute(Type registryType)
        {
            RegistryType = registryType ?? throw new ArgumentNullException(nameof(registryType));
        }

        public Type RegistryType { get; }
    }
}

