using System;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    public sealed class InterceptorInjectorProvider : IInterceptorInjectorProvider
    {
        private readonly IRealServiceProvider _serviceProvider;
        private readonly IPropertyInjectorSelector _propertyInjectorSelector;

        public InterceptorInjectorProvider(
            IRealServiceProvider serviceProvider,
            IPropertyInjectorSelector propertyInjectorSelector)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }
            if (propertyInjectorSelector == null)
            {
                throw new ArgumentNullException(nameof(propertyInjectorSelector));
            }
            _serviceProvider = serviceProvider;
            _propertyInjectorSelector = propertyInjectorSelector;
        }

        public IInterceptorInjector GetInjector(Type interceptorType)
        {
            var propertyInjectors = _propertyInjectorSelector.SelectPropertyInjector(interceptorType);
            return new InterceptorInjector(_serviceProvider, propertyInjectors);
        }
    }
}
