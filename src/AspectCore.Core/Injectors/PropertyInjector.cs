using System;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    public sealed class PropertyInjector : IPropertyInjector
    {
        private readonly Func<IServiceProvider, object> _propertyFactory;
        private readonly Action<object, object> _setter;

        public PropertyInjector(Func<IServiceProvider, object> propertyFactory, Action<object, object> setter)
        {
            if (propertyFactory == null)
            {
                throw new ArgumentNullException(nameof(propertyFactory));
            }
            if (setter == null)
            {
                throw new ArgumentNullException(nameof(setter));
            }

            _propertyFactory = propertyFactory;
            _setter = setter;
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

            _setter(interceptor, _propertyFactory(serviceProvider));
        }
    }
}
