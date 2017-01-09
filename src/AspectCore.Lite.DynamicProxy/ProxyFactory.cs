using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Abstractions.Common;
using System;
using System.Linq;

namespace AspectCore.Lite.DynamicProxy
{
    internal class ProxyFactory : IProxyFactory
    {
        private readonly IServiceProvider serviceProvider;

        public ProxyFactory(IServiceProvider provider, IAspectConfiguration configuration)
        {
            serviceProvider = provider ?? new ServiceProvider(configuration);
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
                var proxyType = serviceProvider.GetService<IProxyGenerator>().CreateType(serviceType, implementationType);
                var supportOriginalService = new SupportOriginalService(implementationInstance);
                return Activator.CreateInstance(proxyType, args.Concat(new object[] { serviceProvider, supportOriginalService }).ToArray());
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException($"Failed to create proxy type for {implementationType}.", exception);
            }
        }
    }
}
