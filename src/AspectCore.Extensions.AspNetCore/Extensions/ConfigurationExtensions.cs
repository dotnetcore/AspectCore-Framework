using System;
using AspectCore.Configuration;

namespace AspectCore.Extensions.AspNetCore
{
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// 添加方法执行日志拦截器到配置中
        /// </summary>
        /// <param name="configuration">拦截配置</param>
        /// <param name="predicates">条件</param>
        /// <returns>拦截配置</returns>
        public static IAspectConfiguration AddMethodExecuteLogging(this IAspectConfiguration configuration, params AspectPredicate[] predicates)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            configuration.Interceptors.AddTyped<MethodExecuteLoggerInterceptor>(predicates);
            return configuration;
        }
    }
}
