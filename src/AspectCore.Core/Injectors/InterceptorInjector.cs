using System;
using System.Collections.Generic;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    [NonAspect]
    internal sealed class InterceptorInjector : IInterceptorInjector
    {
        private readonly IRealServiceProvider _serviceProvider;
        private readonly IEnumerable<IPropertyInjector> _propertyInjectors;

        public InterceptorInjector(
            IRealServiceProvider serviceProvider,
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
            _serviceProvider = serviceProvider;
            _propertyInjectors = propertyInjectors;
        }

        public void Inject(IInterceptor interceptor)
        {
            if (interceptor == null)
            {
                throw new ArgumentNullException(nameof(interceptor));
            }

            foreach(var propertyInjector in _propertyInjectors)
            {
                propertyInjector.Invoke(_serviceProvider, interceptor);
            }
        }
    }
}
