using System;

namespace AspectCore.DependencyInjection
{
    public class DelegateServiceDefinition : ServiceDefinition
    {
        public DelegateServiceDefinition(Type serviceType, Func<IServiceResolver, object> implementationDelegate, Lifetime lifetime) : base(serviceType, lifetime)
        {
            ImplementationDelegate = implementationDelegate ?? throw new ArgumentNullException(nameof(implementationDelegate));
        }

        public Func<IServiceResolver, object> ImplementationDelegate { get; }
    }
}