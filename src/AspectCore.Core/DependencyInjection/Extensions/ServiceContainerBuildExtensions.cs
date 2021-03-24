using System;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// IServiceContext扩展
    /// </summary>
    public static class ServiceContainerBuildExtensions
    {
        /// <summary>
        /// 构建服务解析器(IServiceResolver)
        /// </summary>
        /// <param name="serviceContext">IServiceContext</param>
        /// <returns>服务解析器(IServiceResolver)</returns>
        public static IServiceResolver Build(this IServiceContext serviceContext)
        {
            if (serviceContext == null)
            {
                throw new ArgumentNullException(nameof(serviceContext));
            }
            return new ServiceResolver(serviceContext);
        }
    }
}