using System;
using System.Reflection;

namespace AspectCore.Lite.Abstractions
{
    public static class AspectConfigurationExtensions
    {
        public static IAspectConfiguration Use(this IAspectConfiguration configuration, Func<MethodInfo, IInterceptor> configure)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }
            var option = configuration.GetConfigurationOption<IInterceptor>();
            option.Add(configure);
            return configuration;
        }

        public static IAspectConfiguration Ignore(this IAspectConfiguration configuration, Func<MethodInfo, bool> configure)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }
            var option = configuration.GetConfigurationOption<bool>();
            option.Add(configure);
            return configuration;
        }
    }
}
