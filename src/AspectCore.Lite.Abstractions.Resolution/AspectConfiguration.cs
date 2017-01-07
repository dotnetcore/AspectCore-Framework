using AspectCore.Lite.Abstractions.Resolution.Common;

namespace AspectCore.Lite.Abstractions.Resolution
{
    public sealed class AspectConfiguration : IAspectConfiguration
    {
        private readonly IConfigurationOption<IInterceptor> useOption;
        private readonly IConfigurationOption<bool> ignoreOption;

        public AspectConfiguration()
        {
            useOption = new ConfigurationOption<IInterceptor>();

            ignoreOption = new ConfigurationOption<bool>()
                .IgnoreAspNetCore()
                .IgnoreEntityFramework()
                .IgnoreOwin()
                .IgnorePageGenerator()
                .IgnoreSystem()
                .IgnoreObjectVMethod();
        }

        public IConfigurationOption<TOption> GetConfigurationOption<TOption>()
        {
            if (typeof(TOption) == typeof(bool))
            {
                return ignoreOption as IConfigurationOption<TOption>;
            }
            if (typeof(TOption) == typeof(IInterceptor))
            {
                return useOption as IConfigurationOption<TOption>;
            }
            return null;
        }
    }
}
