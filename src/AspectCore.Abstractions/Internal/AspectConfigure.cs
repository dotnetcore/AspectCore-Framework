using System;
using System.Collections.Concurrent;
using AspectCore.Abstractions.Extensions;

namespace AspectCore.Abstractions.Internal
{
    public sealed class AspectConfigure : IAspectConfigure
    {
        private readonly ConcurrentDictionary<Type, object> _optionCache;

        public AspectConfigure()
        {
            _optionCache = new ConcurrentDictionary<Type, object>();

            var ignoreOption = GetConfigureOption<bool>();

            ignoreOption.IgnoreAspNetCore()
                        .IgnoreEntityFramework()
                        .IgnoreOwin()
                        .IgnorePageGenerator()
                        .IgnoreSystem()
                        .IgnoreObjectVMethod();
        }

        public IAspectConfigureOption<TOption> GetConfigureOption<TOption>()
        {
            return (AspectConfigureOption<TOption>)_optionCache.GetOrAdd(typeof(TOption), key => new AspectConfigureOption<TOption>());
        }
    }
}
