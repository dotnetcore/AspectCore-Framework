using System.Collections.Generic;
using System.Reflection;
using AspectCore.Extensions.Reflection;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 特性拦截器查询器
    /// </summary>
    [NonAspect]
    public sealed class AttributeInterceptorSelector : IInterceptorSelector
    {
        /// <summary>
        /// 通过方法查询所关联的特性拦截器,不追踪继承树
        /// </summary>
        /// <param name="method">待查询的方法</param>
        /// <returns>特性拦截器集合</returns>
        public IEnumerable<IInterceptor> Select(MethodInfo method)
        {
            //查询声明方法的类所标注的拦截器特性
            foreach (var attribute in method.DeclaringType.GetTypeInfo().GetReflector().GetCustomAttributes())
            {
                if (attribute is IInterceptor interceptor)
                    yield return interceptor;
            }

            //查询方法上标注的拦截器特性
            foreach (var attribute in method.GetReflector().GetCustomAttributes())
            {
                if (attribute is IInterceptor interceptor)
                    yield return interceptor;
            }
        }
    }
}