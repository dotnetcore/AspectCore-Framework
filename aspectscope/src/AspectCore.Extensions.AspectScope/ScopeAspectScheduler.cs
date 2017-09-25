using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.AspectScope
{
    [NonAspect]
    public sealed class ScopeAspectScheduler : IAspectScheduler
    {
        private readonly ConcurrentDictionary<ScopeAspectContext, object> _scopedContexts = new ConcurrentDictionary<ScopeAspectContext, object>();
        private readonly IInterceptorCollector _interceptorCollector;
        private int _idx = 0;

        public ScopeAspectScheduler(IInterceptorCollector interceptorCollector)
        {
            _interceptorCollector = interceptorCollector ?? throw new ArgumentNullException(nameof(interceptorCollector));
        }

        public AspectContext[] GetCurrentContexts()
        {
            return _scopedContexts.Keys.Where(x => x.RuntimeContext != null).OrderBy(x => x.Id).ToArray();
        }

        public bool TryEnter(AspectContext context)
        {
            if (context is ScopeAspectContext scopedContext)
            {
                scopedContext.Id = Interlocked.Increment(ref _idx);
                return _scopedContexts.TryAdd(scopedContext, null);
            }
            return false;
        }

        public bool TryRelate(AspectContext context, IInterceptor interceptor)
        {
            if (interceptor == null || context == null)
            {
                return false;
            }
            if (!(context is ScopeAspectContext scopedContext))
            {
                return false;
            }
            if (!(interceptor is IScopeInterceptor scopedInterceptor))
            {
                return true;
            }
            if (!_scopedContexts.ContainsKey(scopedContext))
            {
                return false;
            }
            if (scopedInterceptor.Scope == Scope.None)
            {
                return true;
            }
            var currentContexts = GetCurrentScopedContexts(scopedContext).ToArray();
            if (currentContexts.Length == 0)
            {
                return true;
            }
            if (scopedInterceptor.Scope == Scope.Nested)
            {
                var preContext = currentContexts[currentContexts.Length - 1];
                return !TryInlineImpl(preContext);
            }

            foreach (var current in currentContexts)
                if (TryInlineImpl(current))
                    return false;

            return true;

            IEnumerable<AspectContext> GetCurrentScopedContexts(ScopeAspectContext ctx)
            {
                foreach (var current in GetCurrentContexts().OfType<ScopeAspectContext>().OrderBy(x => x.Id))
                {
                    if (current == ctx)
                        break;
                    yield return current;
                }
            }

            bool TryInlineImpl(AspectContext ctx)
            {
                return _interceptorCollector.
                    Collect(ctx.ServiceMethod, ctx.ImplementationMethod).
                    Where(x => x.GetType() == interceptor.GetType()).
                    Any(x => TryRelate(ctx, x));
            }
        }

        public void Release(AspectContext context)
        {
            if (_scopedContexts.TryRemove(context as ScopeAspectContext, out _))
            {
                Interlocked.Decrement(ref _idx);
            }
        }
    }
}