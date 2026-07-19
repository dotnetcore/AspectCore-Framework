using System;

namespace AspectCore.DependencyInjection
{
    public class DelegateServiceDefinition : ServiceDefinition
    {
        public DelegateServiceDefinition(Type serviceType, Func<IServiceResolver, object> implementationDelegate, Lifetime lifetime, object serviceKey = null) : base(serviceType, lifetime, serviceKey)
        {
            ImplementationDelegate = implementationDelegate ?? throw new ArgumentNullException(nameof(implementationDelegate));
        }

        public Func<IServiceResolver, object> ImplementationDelegate { get; }
    }
}