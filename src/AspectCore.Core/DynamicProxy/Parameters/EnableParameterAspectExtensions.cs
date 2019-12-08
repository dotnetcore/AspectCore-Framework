using System;
using AspectCore.Configuration;

namespace AspectCore.DynamicProxy.Parameters
{
    public static class EnableParameterAspectExtensions
    {
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