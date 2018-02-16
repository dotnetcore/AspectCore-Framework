using System;
using AspectCore.Configuration;

namespace AspectCore.Extensions.AspNetCore
{
    public static class ConfigurationExtensions
    {
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
