using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Abstractions.Extensions
{
    internal static class TypeInfoExtensions
    {
        internal static MethodInfo GetMethodBySign(this TypeInfo typeInfo, MethodInfo method)
        {
            if (method.IsGenericMethod)
            {
                foreach (var genericMethod in typeInfo.DeclaredMethods.Where(m => m.IsGenericMethod))
                {
                    if (method.ToString() == genericMethod.ToString())
                    {
                        return genericMethod;
                    }
                }
            }

            return typeInfo.GetMethod(method.Name, method.GetParameterTypes());
        }
    }
}
