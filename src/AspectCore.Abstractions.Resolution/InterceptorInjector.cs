using System;
using System.Linq;
using System.Reflection;

namespace AspectCore.Abstractions.Resolution
{
    public sealed class InterceptorInjector : IInterceptorInjector
    {
        private readonly IServiceProvider serviceProvider;

        public InterceptorInjector(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }
            this.serviceProvider = serviceProvider;
        }

        public void Inject(IInterceptor interceptor)
        {
            if (interceptor == null)
            {
                throw new ArgumentNullException(nameof(interceptor));
            }

            var properties = interceptor.GetType().GetTypeInfo().DeclaredProperties.Where(x => x.CanWrite && x.IsDefined(typeof(FromServicesAttribute))).ToArray();

            if (!properties.Any())
            {
                return;
            }

            foreach (var property in properties)
            {
                property.SetValue(interceptor, serviceProvider.GetService(property.PropertyType));
            }
        }
    }
}
