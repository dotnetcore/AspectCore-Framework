using AspectCore.Lite.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace AspectCore.Lite.Internal
{
    internal sealed class DefaultInterceptorInjector : IInterceptorInjector
    {
        private static readonly ConcurrentDictionary<PropertyInfo, Action<IInterceptor, object>> PropertySetterCache =
            new ConcurrentDictionary<PropertyInfo, Action<IInterceptor, object>>();

        private readonly IServiceProvider serviceProvider;

        public DefaultInterceptorInjector(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void Inject(IInterceptor interceptor)
        {
            if (interceptor == null)
            {
                throw new ArgumentNullException(nameof(interceptor));
            }

            var properties = interceptor.GetType().GetTypeInfo().DeclaredProperties.Where(x => x.CanWrite && x.IsDefined(typeof(InjectedAttribute)));

            if (!properties.Any())
            {
                return;
            }

            foreach (var property in properties)
            {
                PropertySetterCache.GetOrAdd(property, key => InternalHelper.SetPropertyVaule(key))(interceptor, serviceProvider.GetService(property.PropertyType));
            }
        }
    }
}
