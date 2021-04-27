using System;

namespace AspectCore.DependencyInjection
{
    public static class ServiceContainerBuildExtensions
    {
        /// <summary>
        /// 构建服务解析器
        /// </summary>
        /// <param name="serviceContext">服务上下文</param>
        /// <returns>服务解析器</returns>
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