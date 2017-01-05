using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Abstractions.Common;
using AspectCore.Lite.Abstractions.Resolution;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspectCore.Lite.DynamicProxy
{
    public class NanoContainer : IProxyFactory, IServiceProvider
    {
        private readonly IDictionary<Type, Func<object>> container;

        public NanoContainer()
            : this(null)
        {
        }

        public NanoContainer(Action<IConfigurationOption<IInterceptor>, IConfigurationOption<bool>> configure)
        {
            container = new Dictionary<Type, Func<object>>();
            var configuration = new AspectConfiguration();
            configure?.Invoke(configuration.GetConfiguration<IInterceptor>(), configuration.GetConfiguration<bool>());
            container.Add(typeof(IServiceProvider), () => this);
            container.Add(typeof(IAspectValidator), () => new AspectValidator(configuration));
            container.Add(typeof(IAspectActivator), () => new AspectActivator(this, new AspectBuilder(), new InterceptorMatcher(configuration), new NanoInterceptorInjector()));
            container.Add(typeof(IAspectConfiguration), () => configuration);
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
                var proxyType = CreateProxyType(serviceType, implementationType);
                var supportOriginalService = new NanoSupportOriginalService(implementationInstance);
                return Activator.CreateInstance(proxyType, args.Concat(new object[] { this, supportOriginalService }).ToArray());
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException($"Failed to create proxy type for {implementationType}.", exception);
            }
        }

        private Type CreateProxyType(Type serviceType, Type implementationType)
        {
            var provider = (IServiceProvider)this;
            var aspectValidator = (IAspectValidator)provider.GetService(typeof(IAspectValidator));
            return new TypeGeneratorWrapper().CreateType(serviceType, implementationType, aspectValidator);
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            Func<object> factory = default(Func<object>);

            if (!container.TryGetValue(serviceType, out factory))
            {
                throw new InvalidOperationException($"The type '{serviceType.FullName}' is not registered in the container.");
            }

            return factory();
        }

    }
}
