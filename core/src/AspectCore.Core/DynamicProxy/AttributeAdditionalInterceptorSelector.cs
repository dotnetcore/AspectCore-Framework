using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

            foreach (var attribute in implementationMethod.DeclaringType.GetTypeInfo().GetReflector().GetCustomAttributes())
            {
                if (attribute is IInterceptor interceptor)
                    yield return interceptor;
            }

            foreach (var attribute in implementationMethod.GetReflector().GetCustomAttributes())
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
                foreach (var attribute in baseMethod.DeclaringType.GetTypeInfo().GetReflector().GetCustomAttributes())
                {
                    if (attribute is IInterceptor interceptor && interceptor.Inherited)
                        interceptors.Add(interceptor);
                }

                foreach (var attribute in baseMethod.GetReflector().GetCustomAttributes())
                {
                    if (attribute is IInterceptor interceptor && interceptor.Inherited)
                        interceptors.Add(interceptor);
                }

                interceptors.AddRange(SelectFromBase(baseMethod).Where(x => x.Inherited));
            }

            return interceptors;
        }
    }
}