using System;
using System.Collections.Generic;

namespace AspectCore.Abstractions.Internal
{
    public sealed class InterceptorInjector : IInterceptorInjector
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IEnumerable<IPropertyInjector> propertyInjectors;

        public InterceptorInjector(
            IServiceProvider serviceProvider,
            IEnumerable<IPropertyInjector> propertyInjectors)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }
            if (propertyInjectors == null)
            {
                throw new ArgumentNullException(nameof(propertyInjectors));
            }
            this.serviceProvider = serviceProvider;
            this.propertyInjectors = propertyInjectors;
        }

        public void Inject(IInterceptor interceptor)
        {
            if (interceptor == null)
            {
                throw new ArgumentNullException(nameof(interceptor));
            }

            foreach(var propertyInjector in propertyInjectors)
            {
                propertyInjector.Invoke(serviceProvider, interceptor);
            }
        }
    }
}
