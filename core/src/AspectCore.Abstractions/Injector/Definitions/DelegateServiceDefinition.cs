using System;

namespace AspectCore.Injector
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