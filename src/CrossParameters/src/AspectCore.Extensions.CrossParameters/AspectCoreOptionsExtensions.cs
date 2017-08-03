using System;
using System.Reflection;
using AspectCore.Extensions.Configuration;
using AspectCore.Extensions.Configuration.InterceptorFactories;
using AspectCore.Extensions.CrossParameters.Interceptors;

namespace AspectCore.Extensions.CrossParameters
{
    public static class AspectCoreOptionsExtensions
    {
        public static AspectCoreOptions AddParameterIntercept(this AspectCoreOptions aspectCoreOptions, params Predicate<MethodInfo>[] predicates)
        {
            if (aspectCoreOptions == null)
            {
                throw new ArgumentNullException(nameof(aspectCoreOptions));
            }
            aspectCoreOptions.InterceptorFactories.Add(new TypeInterceptorFactory(typeof(ParameterInterceptAttribute), predicates));
            return aspectCoreOptions;
        }

        public static AspectCoreOptions AddMethodInject(this AspectCoreOptions aspectCoreOptions, params Predicate<MethodInfo>[] predicates)
        {
            if (aspectCoreOptions == null)
            {
                throw new ArgumentNullException(nameof(aspectCoreOptions));
            }
            aspectCoreOptions.InterceptorFactories.Add(new TypeInterceptorFactory(typeof(MethodInjectAttribute), predicates));
            return aspectCoreOptions;
        }
    }
}
