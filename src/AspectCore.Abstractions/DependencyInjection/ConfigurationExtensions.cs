using System;
using AspectCore.Configuration;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 配置扩展
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// 用于配置服务上下文中的配置
        /// </summary>
        /// <param name="serviceContext">服务上下文</param>
        /// <param name="configure">通过此委托配置serviceContext中的配置对象</param>
        /// <returns>服务上下文</returns>
        public static IServiceContext Configure(this IServiceContext serviceContext, Action<IAspectConfiguration> configure)
        {
            if (serviceContext == null)
            {
                throw new ArgumentNullException(nameof(serviceContext));
            }
            configure?.Invoke(serviceContext.Configuration);
            return serviceContext;
        }
    }
}