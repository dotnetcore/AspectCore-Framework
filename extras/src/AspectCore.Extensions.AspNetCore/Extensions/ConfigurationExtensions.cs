using System;
using AspectCore.Configuration;

namespace AspectCore.Extensions.AspNetCore
{
    public static class ConfigurationExtensions
    {
        public static IAspectConfiguration AddMethodExecuteLogger(this IAspectConfiguration configuration, params AspectPredicate[] predicates)
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
