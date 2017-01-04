using AspectCore.Lite.Abstractions.Resolution.Common;
using System;
using System.Reflection;

namespace AspectCore.Lite.Abstractions.Resolution
{
    public sealed class AspectConfiguration : IAspectConfiguration
    {
        private readonly IConfigurationOption<IInterceptor> interceptorOption = new ConfigurationOption<IInterceptor>();
        private readonly IConfigurationOption<bool> ignoreOption = new ConfigurationOption<bool>();

        public AspectConfiguration()
        {
            this.IgnoreAspNetCore()
                .IgnoreOwin()
                .IgnorePageGenerator()
                .IgnoreSystem()
                .IgnoreObjectVMethod();
        }

        public void Add(Func<MethodInfo, IInterceptor> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }
            interceptorOption.Add(configure);
        }

        public void Ignore(Func<MethodInfo, bool> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }
            ignoreOption.Add(configure);
        }

        public IConfigurationOption<TOption> GetConfiguration<TOption>()
        {
            if (typeof(TOption) == typeof(bool))
            {
                return ignoreOption as IConfigurationOption<TOption>;
            }
            if (typeof(TOption) == typeof(IInterceptor))
            {
                return interceptorOption as IConfigurationOption<TOption>;
            }
            return null;
        }
    }
}
