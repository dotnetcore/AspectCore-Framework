using System;
using System.Linq;
using System.Collections.Concurrent;
using AspectCore.Abstractions;
using System.Threading;
using AbsAspectContext = AspectCore.Abstractions.AspectContext;

namespace AspectCore.Core
{
    [NonAspect]
    public sealed class AspectScopeManager : IDisposable
    {
        private readonly ConcurrentDictionary<AbsAspectContext, AspectScope> _scopes;
      
        private int _count = 0;

        internal int Count => _count;

        public AspectScopeManager(IInterceptorProvider interceptorProvider)
        {
            _interceptorProvider = interceptorProvider ?? throw new ArgumentNullException(nameof(interceptorProvider));
            _scopes = new ConcurrentDictionary<AspectContext, AspectScope>();
        }

        internal void AddScope(AbsAspectContext context)
        {
            var scope = new AspectScope(context);
            scope.Level = Interlocked.Increment(ref _count);
            _scopes.TryAdd(context, scope);
        }

        internal void Remove(AbsAspectContext context)
        {
            AspectScope scope;
            if (_scopes.TryRemove(context, out scope))
            {
                scope.AspectContext = null;
            }
        }

        internal bool TryGetScope(AbsAspectContext context, out AspectScope scope)
        {
            return _scopes.TryGetValue(context, out scope);
        }

        internal AspectScope[] GetScopes()
        {
            return _scopes.Values.ToArray();
        }

        public void Dispose()
        {
            var keys = _scopes.Keys.ToArray();
            if (keys.Length == 0) return;
            foreach (var key in keys)
            {
                Remove(key);
            }
        }
    }
}