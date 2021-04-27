using System;
using AspectCore.Configuration;
using AspectCore.Extensions.DataValidation;
using AspectCore.DependencyInjection;

namespace AspectCore.Extensions.DataAnnotations
{
    public static class ServiceContainerExtensions
    {
        /// <summary>
        /// 向容器添加数据校验相关的服务
        /// </summary>
        /// <param name="services">aspectcore服务上下文</param>
        /// <param name="predicates">拦截条件</param>
        /// <returns>aspectcore服务上下文</returns>
        public static IServiceContext AddDataAnnotations(this IServiceContext services, params AspectPredicate[] predicates)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            services.AddType<IDataValidator, AnnotationDataValidator>();
            services.AddType<IPropertyValidator, AnnotationPropertyValidator>();
            services.AddType<IDataStateFactory, DataStateFactory>();
            services.Configuration.Interceptors.AddTyped<DataValidationInterceptorAttribute>(predicates);
            return services;
        }
    }
}