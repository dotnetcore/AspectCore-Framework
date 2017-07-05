using System;
using System.Linq;
using System.Collections.Concurrent;
using AspectCore.Abstractions;
using System.Threading;

namespace AspectCore.Core
{
    [NonAspect]
    public class AspectScopeManager : IDisposable
    {
        private readonly ConcurrentDictionary<AspectContext, AspectScope> _scopes;
        private readonly IInterceptorProvider _interceptorProvider;
        private int _count = 0;

        public AspectScopeManager(IInterceptorProvider interceptorProvider)
        {
            if (interceptorProvider == null)
            {
                throw new ArgumentNullException(nameof(interceptorProvider));
            }
            _interceptorProvider = interceptorProvider;
            _scopes = new ConcurrentDictionary<AspectContext, AspectScope>();
        }

        internal void AddScope(AspectContext context)
        {
            var scope = new AspectScope(context);
            scope.Level = Interlocked.Increment(ref _count);
            _scopes.TryAdd(context, scope);
        }

        internal void Remove(AspectContext context)
        {
            AspectScope scope;
            if (_scopes.TryRemove(context, out scope))
            {
                scope.Local.Value = null;
                scope.Local = null;
            }
        }

        internal bool TryGetScope(AspectContext context, out AspectScope scope)
        {
            return _scopes.TryGetValue(context, out scope);
        }

        internal AspectScope[] GetScopes()
        {
            return _scopes.Values.ToArray();
        }

        internal bool CanExecute(AspectContext context, IInterceptor interceptor)
        {
            if (interceptor.Execution == ExecutionMode.PerExecuted)
            {
                return true;
            }
            if (_scopes.Count <= 1)
            {
                return true;
            }
            AspectScope scope;
            if (!TryGetScope(context, out scope))
            {
                return true;
            }
            if (scope.Level == 0)
            {
                return true;
            }
            if (interceptor.Execution == ExecutionMode.PerNested)
            {
                var scopes = GetScopes().Where(x => x.ScopeId != scope.ScopeId).OrderByDescending(x => x.Level).ToArray();
                for (var i = 0; i < scopes.Length; i++)
                {
                    
                }
            }
        }

        public void Dispose()
        {
            var keys = _scopes.Keys.ToArray();
            if (keys.Length == 0) return;
            foreach (var key in keys)
            {
                AspectScope scope;
                if (_scopes.TryRemove(key, out scope))
                {
                    scope.Local.Value = null;
                    scope.Local = null;
                }
            }
        }
    }
}
