using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Abstractions.Resolution;
using System;
using System.Collections.Generic;

namespace AspectCore.Lite.DynamicProxy
{
    internal sealed class ServiceProvider : IServiceProvider
    {
        private readonly IDictionary<Type, Func<object>> container;

        public ServiceProvider(IAspectConfiguration configuration)
        {
            container = new Dictionary<Type, Func<object>>();
            container.Add(typeof(IServiceProvider), () => this);
            container.Add(typeof(IAspectValidator), () => new AspectValidator(configuration));
            container.Add(typeof(IProxyGenerator), () => new ProxyGenerator(new AspectValidator(configuration)));
            container.Add(typeof(IAspectActivator), () => new AspectActivator(null, new AspectBuilder(), new InterceptorMatcher(configuration), new EmptyInterceptorInjector()));
            container.Add(typeof(IAspectConfiguration), () => configuration);
            container.Add(typeof(IAspectBuilder), () => new AspectBuilder());
            container.Add(typeof(IInterceptorMatcher), () => new InterceptorMatcher(configuration));
            container.Add(typeof(IInterceptorInjector), () => new EmptyInterceptorInjector());
        }

        public object GetService(Type serviceType)
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
