using System;
using AspectCore.Abstractions;
using AspectCore.Extensions.Reflection;

namespace AspectCore.Core
{
    [NonAspect]
    internal sealed class PropertyInjector : IPropertyInjector
    {
        private readonly Func<IServiceProvider, object> _propertyFactory;
        private readonly PropertyReflector _propertyReflector;

        public PropertyInjector(Func<IServiceProvider, object> propertyFactory, PropertyReflector propertyReflector)
        {
            if (propertyFactory == null)
            {
                throw new ArgumentNullException(nameof(propertyFactory));
            }
            if (propertyReflector == null)
            {
                throw new ArgumentNullException(nameof(propertyReflector));
            }

            _propertyFactory = propertyFactory;
            _propertyReflector = propertyReflector;
        }

        public void Invoke(IServiceProvider serviceProvider, IInterceptor interceptor)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }
            if (interceptor == null)
            {
                throw new ArgumentNullException(nameof(interceptor));
            }

            _propertyReflector.SetValue(interceptor, _propertyFactory(serviceProvider));
        }
    }
}
