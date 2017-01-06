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

        //public void Use(Func<MethodInfo, IInterceptor> configure)
        //{
        //    if (configure == null)
        //    {
        //        throw new ArgumentNullException(nameof(configure));
        //    }
        //    interceptorOption.Add(configure);
        //}

        //public void Ignore(Func<MethodInfo, bool> configure)
        //{
        //    if (configure == null)
        //    {
        //        throw new ArgumentNullException(nameof(configure));
        //    }
        //    ignoreOption.Add(configure);
        //}

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
