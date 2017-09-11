using System.Collections.Concurrent;
using System.Reflection;

namespace AspectCore.DynamicProxy
{
    public sealed class CacheAspectValidationHandler : IAspectValidationHandler
    {
        private readonly ConcurrentDictionary<MethodInfo, bool> detectorCache = new ConcurrentDictionary<MethodInfo, bool>();

        public int Order { get; } = -101;

        public bool Invoke(MethodInfo method, AspectValidationDelegate next)
        {
            return detectorCache.GetOrAdd(method, m => next(m));
        }
    }
}