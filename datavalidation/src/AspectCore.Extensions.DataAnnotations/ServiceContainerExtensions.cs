using System;
using AspectCore.Configuration;
using AspectCore.Extensions.DataValidation;
using AspectCore.Injector;

namespace AspectCore.Extensions.DataAnnotations
{
    public static class ServiceContainerExtensions
    {
        public static IServiceContainer AddDataAnnotations(this IServiceContainer services, params AspectPredicate[] predicates)
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