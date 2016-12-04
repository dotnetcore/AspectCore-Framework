using AspectCore.Lite.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AspectCore.Lite.DynamicProxy.Implementation
{
    internal sealed class InterceptorInjector : IInterceptorInjector
    {
        private static readonly ConcurrentDictionary<PropertyInfo, Action<IInterceptor, object>> PropertySetterCache =
            new ConcurrentDictionary<PropertyInfo, Action<IInterceptor, object>>();

        private readonly IServiceProvider serviceProvider;

        public InterceptorInjector(IServiceProvider serviceProvider)
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
                PropertySetterCache.GetOrAdd(property, key => SetValue(key))(interceptor, serviceProvider.GetService(property.PropertyType));
            }
        }

        private Action<IInterceptor, object> SetValue(PropertyInfo propertyInfo)
        {
            var instance = Expression.Parameter(typeof(IInterceptor));
            var value = Expression.Parameter(typeof(object));
            var caseInstance = Expression.Convert(instance, propertyInfo.DeclaringType);
            var castValue = Expression.Convert(value, propertyInfo.PropertyType);
            var assignOprerator = Expression.Call(instance, propertyInfo.SetMethod, castValue);
            return Expression.Lambda<Action<IInterceptor, object>>(assignOprerator, instance, value).Compile();
        }
    }
}
