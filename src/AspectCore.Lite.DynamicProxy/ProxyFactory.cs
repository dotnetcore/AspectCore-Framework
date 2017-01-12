using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Abstractions.Common;
using System;
using System.Linq;

namespace AspectCore.Lite.DynamicProxy
{
    internal class ProxyFactory : IProxyFactory
    {
        public IServiceProvider ServiceProvider { get; }

        internal ProxyFactory(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            ServiceProvider = serviceProvider;
        }

        public object CreateProxy(Type serviceType, Type implementationType, object implementationInstance, params object[] args)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (implementationType == null)
            {
                throw new ArgumentNullException(nameof(implementationType));
            }

            if (implementationInstance == null)
            {
                throw new ArgumentNullException(nameof(implementationInstance));
            }

            return TryCreateProxy(serviceType, implementationType, implementationInstance, args ?? EmptyArray<object>.Value);
        }

        private object TryCreateProxy(Type serviceType, Type implementationType, object implementationInstance, params object[] args)
        {
            try
            {
                var proxyType = ServiceProvider.GetService<IProxyGenerator>().CreateType(serviceType, implementationType);
                var supportOriginalService = new SupportOriginalService(implementationInstance);
                return Activator.CreateInstance(proxyType, args.Concat(new object[] { ServiceProvider, supportOriginalService }).ToArray());
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException($"Failed to create proxy type for {implementationType}.", exception);
            }
        }
    }
}
