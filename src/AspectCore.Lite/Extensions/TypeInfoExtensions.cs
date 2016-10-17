using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspectCore.Lite.Extensions
{
    public static class TypeInfoExtensions
    {

        public static bool CanProxy(this TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }

            if (typeInfo.IsValueType)
            {
                return false;
            }

            IPointcut pointcut = PointcutUtilities.GetPointcut(typeInfo);
            return typeInfo.DeclaredMethods.Any(method => pointcut.IsMatch(method));
        }

        //public static MethodInfo GetRequiredMethod(this TypeInfo typeInfo, string name, object[] parameters)
        //{
        //    int bestLength = -1;
        //    var bestMatcher = default(MethodMatcher);

        //    var matchers = typeInfo.DeclaredMethods.Where(m => m.Name == name && m.GetParameters().Length == parameters.Length).Select(m => new MethodMatcher(m)).ToArray();

        //    if (matchers.Length == 1)
        //    {
        //        return (MethodInfo)matchers[0].AsMethodBase();
        //    }

        //    foreach (var matcher in matchers)
        //    {
        //        var length = matcher.Match(parameters);
        //        if (length == -1)
        //        {
        //            continue;
        //        }
        //        if (bestLength < length)
        //        {
        //            bestLength = length;
        //            bestMatcher = matcher;
        //        }
        //    }

        //    if (bestMatcher == null)
        //    {
        //        var message = $"A suitable method for type '{typeInfo.AsType()}' could not be located. Ensure the type is concrete and services are registered for all parameters of a public method.";
        //        throw new InvalidOperationException(message);
        //    }

        //    return (MethodInfo)bestMatcher.AsMethodBase();
        //}
    }
}
