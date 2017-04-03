using System;

namespace AspectCore.Abstractions.Internal
{
    public sealed class InterceptorInjectorProvider : IInterceptorInjectorProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IPropertyInjectorSelector _propertyInjectorSelector;

        public InterceptorInjectorProvider(
            IServiceProvider serviceProvider,
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
            this._serviceProvider = serviceProvider;
            this._propertyInjectorSelector = propertyInjectorSelector;
        }

        public IInterceptorInjector GetInjector(Type interceptorType)
        {
            var propertyInjectors = _propertyInjectorSelector.SelectPropertyInjector(interceptorType);
            return new InterceptorInjector(_serviceProvider, propertyInjectors);
        }
    }
}
