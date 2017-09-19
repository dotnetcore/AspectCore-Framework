using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AspectCore.DynamicProxy
{
    public static class AspectContextExtensions
    {
        private static readonly ConcurrentDictionary<MethodInfo, bool> isAsyncCache = new ConcurrentDictionary<MethodInfo, bool>();

        public static bool IsAsync(this AspectContext aspectContext)
        {
            if (aspectContext == null)
            {
                throw new ArgumentNullException(nameof(aspectContext));
            }
            var isAsyncFromMetaData = isAsyncCache.GetOrAdd(aspectContext.ServiceMethod, IsAsyncFromMetaData);
            if (isAsyncFromMetaData)
            {
                return true;
            }
            if (aspectContext.ReturnValue != null)
            {
                return IsAsyncType(aspectContext.ReturnValue.GetType().GetTypeInfo());
            }
            return false;
        }

        private static bool IsAsyncFromMetaData(MethodInfo method)
        {
            if (IsAsyncType(method.ReturnType.GetTypeInfo()))
            {
                return true;
            }
            if (method.IsDefined(typeof(AsyncAspectAttribute), true))
            {
                if (method.ReturnType == typeof(object))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsAsyncType(TypeInfo typeInfo)
        {
            if (typeInfo.AsType() == typeof(Task)
                || typeof(Task).GetTypeInfo().IsAssignableFrom(typeInfo)
                || typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(ValueTask<>))
            {
                return true;
            }
            return false;
        }

        public static object UnwrapAsyncReturnValue(this AspectContext aspectContext)
        {
            if (aspectContext == null)
            {
                throw new ArgumentNullException(nameof(aspectContext));
            }
            if (!aspectContext.IsAsync())
            {
                throw new AspectInvocationException(aspectContext, new InvalidOperationException("This operation only supports asynchronous method."));
            }
            var returnValue = aspectContext.ReturnValue;
            if (returnValue == null)
            {
                return null;
            }
            var returnTypeInfo = returnValue.GetType().GetTypeInfo();
            //todo
            return null;
        }
    }
}