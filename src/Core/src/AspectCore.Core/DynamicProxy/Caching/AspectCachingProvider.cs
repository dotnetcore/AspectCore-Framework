using System;
using System.Collections.Concurrent;
using AspectCore.Configuration;

namespace AspectCore.DynamicProxy
{
    public sealed class AspectCachingProvider : IAspectCachingProvider
    {
        private static readonly ConcurrentDictionary<IAspectConfiguration, ConcurrentDictionary<string, IAspectCaching>> globalCachings = new ConcurrentDictionary<IAspectConfiguration, ConcurrentDictionary<string, IAspectCaching>>();
        private readonly ConcurrentDictionary<string, IAspectCaching> _cachings;
        private readonly IAspectConfiguration _aspectConfiguration;

        public AspectCachingProvider(IAspectConfiguration aspectConfiguration)
        {
            _aspectConfiguration = aspectConfiguration ?? throw new ArgumentNullException(nameof(aspectConfiguration));
            _cachings = globalCachings.GetOrAdd(_aspectConfiguration, key => new ConcurrentDictionary<string, IAspectCaching>());
        }

        public void Dispose()
        {
            globalCachings.TryRemove(_aspectConfiguration, out _);
            foreach(var caching in _cachings)
            {
                caching.Value.Dispose();
            }
            _cachings.Clear();
        }

        public IAspectCaching GetAspectCaching(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            return _cachings.GetOrAdd(name, key => new AspectCaching(key));
        }
    }
}