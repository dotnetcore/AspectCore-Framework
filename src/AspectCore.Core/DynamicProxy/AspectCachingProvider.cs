using System;
using System.Collections.Concurrent;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public sealed class AspectCachingProvider : IAspectCachingProvider
    {
        private readonly ConcurrentDictionary<string, IAspectCaching> _cachings;

        public AspectCachingProvider()
        {
            _cachings = new ConcurrentDictionary<string, IAspectCaching>();
        }

        public void Dispose()
        {
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