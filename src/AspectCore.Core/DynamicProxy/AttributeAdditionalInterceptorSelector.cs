using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Extensions.Reflection;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 用于查询继承树上相关的拦截器特性
    /// </summary>
    [NonAspect]
    public sealed class AttributeAdditionalInterceptorSelector : IAdditionalInterceptorSelector
    {
        /// <summary>
        /// 查询继承树上相关的拦截器特性
        /// </summary>
        /// <param name="serviceMethod">暴露的服务方法</param>
        /// <param name="implementationMethod">目标方法</param>
        /// <returns>特性拦截器集合</returns>
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

        /// <summary>
        /// 查询基类型和基类型中对应方法(即：implementationMethod实现的基类型的方法)上标注的拦截器特性
        /// </summary>
        /// <param name="implementationMethod">实现方法</param>
        /// <returns>特性拦截器集合</returns>
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