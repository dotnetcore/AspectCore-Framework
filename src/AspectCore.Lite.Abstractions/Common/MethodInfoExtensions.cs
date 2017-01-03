using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions.Common
{
    public static class MethodInfoExtensions
    {
        public static object DynamicInvoke(this MethodInfo method, object instance, params object[] parameters)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            return new MethodAccessor(method).CreateMethodInvoker()(instance, parameters);
        }

        public static TResult DynamicInvoke<TResult>(this MethodInfo method, object instance, params object[] parameters)
        {
            return (TResult)DynamicInvoke(method, instance, parameters);
        }

        public static Type[] GetParameterTypes(this MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            return method.GetParameters().Select(parame => parame.ParameterType).ToArray();
        }
    }
}
