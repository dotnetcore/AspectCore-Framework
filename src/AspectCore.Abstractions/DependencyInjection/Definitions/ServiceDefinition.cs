using System;
using AspectCore.DynamicProxy;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 描述一种服务，包括该服务的类型、生存期
    /// </summary>
    [NonAspect]
    public abstract class ServiceDefinition
    {
        /// <summary>
        /// 服务类型
        /// </summary>
        public Type ServiceType { get; }

        /// <summary>
        /// 生存期
        /// </summary>
        public Lifetime Lifetime { get; }

        /// <summary>
        /// 通过服务的类型、生存期构造
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <param name="lifetime">生存期</param>
        public ServiceDefinition(Type serviceType, Lifetime lifetime)
        {
            Lifetime = lifetime;
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
        }
    }
}