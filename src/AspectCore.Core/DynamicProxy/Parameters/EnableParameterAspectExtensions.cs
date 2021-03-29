using System;
using AspectCore.Configuration;

namespace AspectCore.DynamicProxy.Parameters
{
    public static class EnableParameterAspectExtensions
    {
        /// <summary>
        /// 启用参数拦截
        /// </summary>
        /// <param name="configuration">拦截配置</param>
        /// <param name="predicates">拦截条件</param>
        /// <returns>拦截配置</returns>
        public static IAspectConfiguration EnableParameterAspect(this IAspectConfiguration configuration, params AspectPredicate[] predicates)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            configuration.Interceptors.AddTyped<EnableParameterAspectInterceptor>(predicates);
            return configuration;
        }
    }
}