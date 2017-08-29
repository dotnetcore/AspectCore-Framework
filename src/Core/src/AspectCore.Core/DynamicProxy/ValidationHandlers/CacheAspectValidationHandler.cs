using System.Collections.Concurrent;
using System.Reflection;
using AspectCore.Abstractions;

namespace AspectCore.Core.DynamicProxy
{
    [NonAspect]
    public sealed class CacheAspectValidationHandler : IAspectValidationHandler
    {
        private static readonly ConcurrentDictionary<MethodInfo, bool> detectorCache = new ConcurrentDictionary<MethodInfo, bool>();

        public int Order { get; } = -101;

        public bool Invoke(MethodInfo method, AspectValidationDelegate next)
        {
            return detectorCache.GetOrAdd(method, m => next(m));
        }
    }
}