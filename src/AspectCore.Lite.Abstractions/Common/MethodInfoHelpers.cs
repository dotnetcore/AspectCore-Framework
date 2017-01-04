using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AspectCore.Lite.Abstractions.Common
{
    public static class MethodInfoHelpers
    {
        public static MethodInfo GetMethod<T>(Expression<T> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }
            var methodCallExpression = expression.Body as MethodCallExpression;
            if (methodCallExpression == null)
            {
                throw new InvalidCastException("Cannot be converted to MethodCallExpression");
            }
            return methodCallExpression.Method;
        }

        public static MethodInfo GetMethod<T>(string name)
        {
            return typeof(T).GetTypeInfo().GetMethod(name);
        }
    }
}
