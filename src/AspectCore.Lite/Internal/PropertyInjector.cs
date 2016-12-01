using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Common;
using AspectCore.Lite.Extensions;
using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace AspectCore.Lite.Internal
{
    internal sealed class PropertyInjector : IPropertyInjector
    {
        private static readonly ConcurrentDictionary<PropertyInfo, Action<IInterceptor, object>> PropertySetterCache =
            new ConcurrentDictionary<PropertyInfo, Action<IInterceptor, object>>();
        private readonly IServiceProvider serviceProvider;
        private readonly IInjectedPropertyMatcher injectedPropertyMatcher;

        public PropertyInjector(IInjectedPropertyMatcher injectedPropertyMatcher, IServiceProvider serviceProvider)
        {
            this.injectedPropertyMatcher = injectedPropertyMatcher;
            this.serviceProvider = serviceProvider;
        }

        public void Injection(IInterceptor interceptor)
        {
            ExceptionHelper.ThrowArgumentNull(interceptor, nameof(interceptor));
            var properties = injectedPropertyMatcher.Match(interceptor);
            properties.ForEach(property =>
            {
                var setter = PropertySetterCache.GetOrAdd(property, key => GetSetter(key));
                setter(interceptor, serviceProvider.GetService(property.PropertyType));
            });
        }

        private Action<IInterceptor, object> GetSetter(PropertyInfo propertyInfo)
        {
            var instance = Expression.Parameter(typeof(IInterceptor));
            var value = Expression.Parameter(typeof(object));
            var property = Expression.Property(instance, propertyInfo);
            var assign = Expression.Assign(property, value);
            return Expression.Lambda<Action<IInterceptor, object>>(assign, instance, value).Compile();
        }
    }
}
