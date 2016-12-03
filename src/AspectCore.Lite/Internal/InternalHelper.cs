using AspectCore.Lite.Abstractions;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AspectCore.Lite.Internal
{
    internal static class InternalHelper
    {
        internal static Action<IInterceptor, object> SetPropertyVaule(PropertyInfo propertyInfo)
        {
            var instanceParameter = Expression.Parameter(typeof(IInterceptor));
            var valueParameter = Expression.Parameter(typeof(object));
            var caseInstance = Expression.Convert(instanceParameter, propertyInfo.DeclaringType);
            var castValue = Expression.Convert(valueParameter, propertyInfo.PropertyType);
            var assign = Expression.Call(instanceParameter, propertyInfo.SetMethod, castValue);
            return Expression.Lambda<Action<IInterceptor, object>>(assign, instanceParameter, valueParameter).Compile();
        }
    }
}
