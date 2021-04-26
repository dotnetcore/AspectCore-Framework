using System;
using AspectCore.DynamicProxy;
using AspectCore.DependencyInjection;

namespace AspectCore.Extensions.AspectScope
{
    public static class ServiceContainerExtensions
    {
        /// <summary>
        /// 添加拦截器作用域服务到容器中
        /// </summary>
        /// <param name="services">服务上下文</param>
        /// <returns>服务上下文</returns>
        public static IServiceContext AddAspectScope(this IServiceContext services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            services.AddType<IAspectScheduler, ScopeAspectScheduler>(Lifetime.Scoped);
            services.AddType<IAspectContextFactory, ScopeAspectContextFactory>(Lifetime.Scoped);
            services.AddType<IAspectBuilderFactory, ScopeAspectBuilderFactory>(Lifetime.Scoped);
            return services;
        }
    }
}