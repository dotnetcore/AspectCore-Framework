using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Configuration;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 配置拦截器查询器
    /// </summary>
    [NonAspect]
    public sealed class ConfigureInterceptorSelector : IInterceptorSelector
    {
        private readonly IAspectConfiguration _aspectConfiguration;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 构造配置拦截器查询器
        /// </summary>
        /// <param name="aspectConfiguration">拦截配置</param>
        /// <param name="serviceProvider">IServiceProvider</param>
        public ConfigureInterceptorSelector(IAspectConfiguration aspectConfiguration, IServiceProvider serviceProvider)
        {
            _aspectConfiguration = aspectConfiguration ?? throw new ArgumentNullException(nameof(aspectConfiguration));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// 查询拦截配置中此方法关联的拦截器
        /// </summary>
        /// <param name="method">待查询的方法</param>
        /// <returns>拦截器集合</returns>
        public IEnumerable<IInterceptor> Select(MethodInfo method)
        {
            //todo fix nonaspect
            foreach (var interceptorFactory in _aspectConfiguration.Interceptors)
            {
                if (interceptorFactory.Predicates.Length != 0)
                {
                    if (interceptorFactory.CanCreated(method))
                        yield return interceptorFactory.CreateInstance(_serviceProvider);
                }
                else
                {
                    if (!_aspectConfiguration.NonAspectPredicates.Any(x => x(method)))
                        yield return interceptorFactory.CreateInstance(_serviceProvider);
                }
            }
        }
    }
}