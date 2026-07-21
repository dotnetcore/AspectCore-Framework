using System;
using System.Collections.Concurrent;
#if NET8_0_OR_GREATER
using System.Collections.Frozen;
using System.Threading;
#endif
using System.Reflection;

namespace AspectCore.DynamicProxy
{
    public sealed class CacheAspectValidationHandler : IAspectValidationHandler
    {
        private readonly ConcurrentDictionary<AspectValidationContext, bool> detectorCache = new ConcurrentDictionary<AspectValidationContext, bool>();

#if NET8_0_OR_GREATER
        /// <summary>
        /// After a warmup period, the cache is frozen into a <see cref="FrozenDictionary{TKey, TValue}"/>
        /// for faster lookups. The ConcurrentDictionary remains as the fallback for cache misses.
        /// </summary>
        private volatile FrozenDictionary<AspectValidationContext, bool> _frozen;
        private int _callCount;
        private const int FreezeThreshold = 100;
#endif

        public int Order { get; } = -101;

        public bool Invoke(AspectValidationContext context, AspectValidationDelegate next)
        {
#if NET8_0_OR_GREATER
            // Fast path: use the frozen dictionary for lookups after warmup stabilization.
            var frozen = _frozen;
            if (frozen != null && frozen.TryGetValue(context, out var frozenResult))
            {
                return frozenResult;
            }
#endif

            if (detectorCache.TryGetValue(context, out var cached))
            {
#if NET8_0_OR_GREATER
                // After threshold calls, snapshot the concurrent dictionary into a frozen one.
                if (_frozen == null && Interlocked.Increment(ref _callCount) == FreezeThreshold)
                {
                    _frozen = detectorCache.ToFrozenDictionary();
                }
#endif
                return cached;
            }

            var result = detectorCache.GetOrAdd(context, _ => next(context));

#if NET8_0_OR_GREATER
            if (_frozen == null && Interlocked.Increment(ref _callCount) == FreezeThreshold)
            {
                _frozen = detectorCache.ToFrozenDictionary();
            }
#endif

            return result;
        }
    }
}
