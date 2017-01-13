using AspectCore.Lite.Abstractions.Resolution.Extensions;
using System;
using System.Collections.Concurrent;

namespace AspectCore.Lite.Abstractions.Resolution
{
    public sealed class AspectConfiguration : IAspectConfiguration
    {
        private readonly ConcurrentDictionary<Type, object> optionCache;

        public AspectConfiguration()
        {
            optionCache = new ConcurrentDictionary<Type, object>();

            var ignoreOption = GetConfigurationOption<bool>();

            ignoreOption.IgnoreAspNetCore()
                        .IgnoreEntityFramework()
                        .IgnoreOwin()
                        .IgnorePageGenerator()
                        .IgnoreSystem()
                        .IgnoreObjectVMethod();
        }

        public IConfigurationOption<TOption> GetConfigurationOption<TOption>()
        {
            return (ConfigurationOption<TOption>)optionCache.GetOrAdd(typeof(TOption), key => new ConfigurationOption<TOption>());
        }
    }
}
