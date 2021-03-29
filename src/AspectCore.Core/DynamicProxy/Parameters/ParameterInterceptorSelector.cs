using System;
using System.Linq;
using System.Reflection;
using AspectCore.Extensions.Reflection;
using AspectCore.DependencyInjection;
using AspectCore.Utils;

namespace AspectCore.DynamicProxy.Parameters
{
    /// <summary>
    /// 参数拦截器的查询器
    /// </summary>
    public sealed class ParameterInterceptorSelector : IParameterInterceptorSelector
    {
        private readonly IPropertyInjectorFactory _propertyInjectorFactory;
        private readonly IAspectCaching _aspectCaching;

        public ParameterInterceptorSelector(IPropertyInjectorFactory propertyInjectorFactory, IAspectCachingProvider aspectCachingProvider)
        {
            if (aspectCachingProvider == null)
            {
                throw new ArgumentNullException(nameof(aspectCachingProvider));
            }
            _propertyInjectorFactory = propertyInjectorFactory ?? throw new ArgumentNullException(nameof(propertyInjectorFactory));
            _aspectCaching = aspectCachingProvider.GetAspectCaching(nameof(ParameterInterceptorSelector));
        }

        /// <summary>
        /// 查询参数parameter关联的参数拦截器
        /// </summary>
        /// <param name="parameter">参数</param>
        /// <returns>关联的参数拦截器数组</returns>
        public IParameterInterceptor[] Select(ParameterInfo parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            return (IParameterInterceptor[])_aspectCaching.GetOrAdd(parameter, info =>
             {
                 //注：OfType根据指定类型筛选 IEnumerable 的元素。
                 var interceptors = ((ParameterInfo)info).GetReflector().GetCustomAttributes().OfType<IParameterInterceptor>().ToArray();
                 for (var i = 0; i < interceptors.Length; i++)
                 {
                     var interceptor = interceptors[i];
                     if (PropertyInjectionUtils.Required(interceptor))
                     {
                         //为拦截器注入需要的属性值
                         _propertyInjectorFactory.Create(interceptor.GetType()).Invoke(interceptor);
                     }
                 }
                 return interceptors;
             });
        }
    }
}