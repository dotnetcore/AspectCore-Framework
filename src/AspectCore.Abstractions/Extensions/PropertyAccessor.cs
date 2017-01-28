using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace AspectCore.Abstractions.Extensions
{
    public sealed class PropertyAccessor
    {
        private readonly static ConcurrentDictionary<PropertyInfo, Func<object, object>> getterCache = new ConcurrentDictionary<PropertyInfo, Func<object, object>>();

        private readonly static ConcurrentDictionary<PropertyInfo, Action<object, object>> setterCache = new ConcurrentDictionary<PropertyInfo, Action<object, object>>();

        public PropertyInfo Property { get; }

        public PropertyAccessor(PropertyInfo property)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            this.Property = property;
        }

        public Func<object, object> CreatePropertyGetter()
        {
            return getterCache.GetOrAdd(Property, MakeFastPropertyGetter);
        }

        public Action<object,object> CreatePropertySetter()
        {
            return setterCache.GetOrAdd(Property, MakeFastPropertySetter);
        }

        private Func<object, object> MakeFastPropertyGetter(PropertyInfo propertyInfo)
        {
            var instanceParam = Expression.Parameter(typeof(object), "instance");
            var castInstanceExpression = Expression.Convert(instanceParam, propertyInfo.DeclaringType);
            var getter = propertyInfo.GetGetMethod();
            var val = Expression.Call(castInstanceExpression, getter);
            var lambdaExpression = Expression.Lambda<Func<object, object>>(Expression.Convert(val, typeof(object)), instanceParam);
            return lambdaExpression.Compile();
        }

        private Action<object, object> MakeFastPropertySetter(PropertyInfo propertyInfo)
        {
            var instanceParam = Expression.Parameter(typeof(object), "instance");
            var valueParam = Expression.Parameter(typeof(object), "value");
            var castInstanceExpression = Expression.Convert(instanceParam, propertyInfo.DeclaringType);
            var castValueExpression = Expression.Convert(valueParam, propertyInfo.PropertyType);
            var setter = propertyInfo.SetMethod;
            var assignExpression = Expression.Call(castInstanceExpression, setter, castValueExpression);
            var lambdaExpression = Expression.Lambda<Action<object, object>>(assignExpression, instanceParam, valueParam);
            return lambdaExpression.Compile();
        }
    }
}
