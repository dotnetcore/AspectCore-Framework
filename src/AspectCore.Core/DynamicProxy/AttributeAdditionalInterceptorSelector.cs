using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using AspectCore.Extensions.Reflection;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public sealed class AttributeAdditionalInterceptorSelector : IAdditionalInterceptorSelector
    {
        public IEnumerable<IInterceptor> Select(MethodInfo serviceMethod, MethodInfo implementationMethod)
        {
            if (serviceMethod == implementationMethod)
            {
                yield break;
            }

            foreach (var attribute in GetCustomAttributesSafe(implementationMethod.DeclaringType.GetTypeInfo()))
            {
                if (attribute is IInterceptor interceptor)
                    yield return interceptor;
            }

            foreach (var attribute in GetCustomAttributesSafe(implementationMethod))
            {
                if (attribute is IInterceptor interceptor)
                    yield return interceptor;
            }

            if (!serviceMethod.DeclaringType.GetTypeInfo().IsClass)
            {
                foreach (var interceptor in SelectFromBase(implementationMethod))
                    yield return interceptor;
            }
        }

        private IEnumerable<IInterceptor> SelectFromBase(MethodInfo implementationMethod)
        {
            var interceptors = new List<IInterceptor>();
            var typeInfo = implementationMethod.DeclaringType.GetTypeInfo();
            var baseType = typeInfo.BaseType;
            if (baseType == typeof(object) || baseType == null)
            {
                return interceptors;
            }

            var baseMethod = baseType.GetTypeInfo().GetMethodBySignature(new MethodSignature(implementationMethod));
            if (baseMethod != null)
            {
                foreach (var attribute in GetCustomAttributesSafe(baseMethod.DeclaringType.GetTypeInfo()))
                {
                    if (attribute is IInterceptor interceptor && interceptor.Inherited)
                        interceptors.Add(interceptor);
                }

                foreach (var attribute in GetCustomAttributesSafe(baseMethod))
                {
                    if (attribute is IInterceptor interceptor && interceptor.Inherited)
                        interceptors.Add(interceptor);
                }

                interceptors.AddRange(SelectFromBase(baseMethod).Where(x => x.Inherited));
            }

            return interceptors;
        }

        private static IEnumerable<Attribute> GetCustomAttributesSafe(MemberInfo member)
        {
            if (!RuntimeFeature.IsDynamicCodeSupported)
            {
                return member.GetCustomAttributes(true).OfType<Attribute>();
            }
            if (member is TypeInfo typeInfo)
            {
                return typeInfo.GetReflector().GetCustomAttributes();
            }
            if (member is MethodInfo method)
            {
                return method.GetReflector().GetCustomAttributes();
            }
            return member.GetCustomAttributes(true).OfType<Attribute>();
        }
    }
}
