using System;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 委托类型的服务描述对象
    /// </summary>
    public class DelegateServiceDefinition : ServiceDefinition
    {
        /// <summary>
        /// 构造表示委托类型的服务描述对象的实例
        /// </summary>
        /// <param name="serviceType">暴露的服务类型</param>
        /// <param name="implementationDelegate">一个委托类型</param>
        /// <param name="lifetime">生命周期</param>
        public DelegateServiceDefinition(Type serviceType, Func<IServiceResolver, object> implementationDelegate, Lifetime lifetime) : base(serviceType, lifetime)
        {
            ImplementationDelegate = implementationDelegate ?? throw new ArgumentNullException(nameof(implementationDelegate));
        }

        /// <summary>
        /// 此对象关注的委托
        /// </summary>
        public Func<IServiceResolver, object> ImplementationDelegate { get; }
    }
}