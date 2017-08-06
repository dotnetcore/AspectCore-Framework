using System;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.IoC.Resolves
{
    internal sealed class DelegateServiceFactory : IServiceFactory
    {
        private readonly Func<IServiceResolver, object> _implementationDelegate;

        public ServiceKey ServiceKey { get; }

        public DelegateServiceFactory(ServiceKey serviceKey, Func<IServiceResolver, object> implementationDelegate)
        {
            ServiceKey = serviceKey;
            _implementationDelegate = implementationDelegate;
        }

        public object Invoke(IServiceResolver resolver)
        {
            return _implementationDelegate(resolver);
        }
    }
}