using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.AspectScope
{
    [NonAspect]
    public sealed class ScopeAspectScheduler : IAspectScheduler
    {
        private readonly ConcurrentDictionary<MethodInfo, AspectEntry> _entries = new ConcurrentDictionary<MethodInfo, AspectEntry>();
        private readonly IInterceptorCollector _interceptorCollector;
        private int _idx = 0;

        public ScopeAspectScheduler(IInterceptorCollector interceptorCollector)
        {
            _interceptorCollector = interceptorCollector ?? throw new ArgumentNullException(nameof(interceptorCollector));
        }

        public AspectContext[] GetCurrentContexts()
        {
            var invokes = SchedulerHelpers.GetInvokeMethods();
            if (invokes.Length == 0)
            {
                return Array.Empty<AspectContext>();
            }
            var entry = GetAspectEntry(invokes.Last());
            return entry.Contexts.ToArray();
        }

        public bool TryEnter(AspectContext context)
        {
            var invokes = SchedulerHelpers.GetInvokeMethods();
            if (invokes.Length == 0)
            {
                return false;
            }
            var entry = GetAspectEntry(invokes.Last());
            entry.Contexts.Add(context);
            return true;
        }

        public bool TryRelate(AspectContext context, IInterceptor interceptor)
        {
            if (interceptor == null || context == null)
            {
                return false;
            }
            if (!(interceptor is IScopeInterceptor scopedInterceptor))
            {
                return true;
            }
            if (scopedInterceptor.Scope == Scope.None)
            {
                return true;
            }
            var currentContexts = GetCurrentContextsInternal(context).ToArray();
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

            IEnumerable<AspectContext> GetCurrentContextsInternal(AspectContext ctx)
            {
                foreach (var current in GetCurrentContexts())
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
            var invokes = SchedulerHelpers.GetInvokeMethods();
            if (invokes.Length == 0)
            {
                return;
            }
            var entry = GetAspectEntry(invokes.Last());
            entry.Contexts.Remove(context);
        }

        private AspectEntry GetAspectEntry(MethodInfo aspectInvoke)
        {
            return _entries.GetOrAdd(aspectInvoke, invoke => new AspectEntry(invoke));
        }

        private class AspectEntry
        {
            private readonly MethodInfo _aspectInvoke;
            private readonly List<AspectContext> _contexts;

            public List<AspectContext> Contexts
            {
                get
                {
                    return _contexts;
                }
            }

            public AspectEntry(MethodInfo aspectInvoke)
            {
                _aspectInvoke = aspectInvoke;
                _contexts = new List<AspectContext>();
            }
        }
    }
}