using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace AspectCore.DynamicProxy
{
    public sealed class CacheAspectValidationHandler : IAspectValidationHandler
    {
        private readonly ConcurrentDictionary<AspectValidationContext, bool> detectorCache = new ConcurrentDictionary<AspectValidationContext, bool>();

        public int Order { get; } = -101;

        public bool Invoke(AspectValidationContext context, AspectValidationDelegate next)
        {
            return detectorCache.GetOrAdd(context, tuple => next(context));
        }
    }
}