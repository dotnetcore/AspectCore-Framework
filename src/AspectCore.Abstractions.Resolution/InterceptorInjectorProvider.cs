using AspectCore.Abstractions.Resolution.Internal;
using System;

namespace AspectCore.Abstractions.Resolution
{
    public sealed class InterceptorInjectorProvider : IInterceptorInjectorProvider
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IPropertyInjectorSelector propertyInjectorSelector;

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
            this.serviceProvider = serviceProvider;
            this.propertyInjectorSelector = propertyInjectorSelector;
        }

        public IInterceptorInjector GetInjector(Type interceptorType)
        {
            var propertyInjectors = propertyInjectorSelector.SelectPropertyInjector(interceptorType);
            return new InterceptorInjector(serviceProvider, propertyInjectors);
        }
    }
}
