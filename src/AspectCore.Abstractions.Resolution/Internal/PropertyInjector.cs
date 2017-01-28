using System;

namespace AspectCore.Abstractions.Resolution.Internal
{
    public sealed class PropertyInjector : IPropertyInjector
    {
        private readonly Func<IServiceProvider, object> propertyFactory;
        private readonly Action<object, object> setter;

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

            this.propertyFactory = propertyFactory;
            this.setter = setter;
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

            setter(interceptor, propertyFactory(serviceProvider));
        }
    }
}
