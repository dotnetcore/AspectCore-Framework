using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.ScopedContext
{
    internal sealed class ScopedAspectContextScheduler : IAspectContextScheduler
    {
        private readonly ConcurrentDictionary<ScopedAspectContext, object> _scopedContexts = new ConcurrentDictionary<ScopedAspectContext, object>();
        private readonly IInterceptorCollector _interceptorCollector;
        private int _idx = 0;

        public ScopedAspectContextScheduler(IInterceptorCollector interceptorCollector)
        {
            _interceptorCollector = interceptorCollector ?? throw new ArgumentNullException(nameof(interceptorCollector));
        }

        public AspectContext[] GetCurrentContexts()
        {
            return _scopedContexts.Keys.Where(x => x.RuntimeContext != null).ToArray();
        }

        public bool TryEnter(AspectContext context)
        {
            if (context is ScopedAspectContext scopedContext)
            {
                scopedContext.Id = Interlocked.Increment(ref _idx);
                return _scopedContexts.TryAdd(scopedContext, null);
            }
            return false;
        }

        public bool TryInclude(AspectContext context, IScopedInterceptor interceptor)
        {
            if (interceptor == null || context == null)
            {
                return false;
            }
            if (!(context is ScopedAspectContext scopedContext))
            {
                return false;
            }
            if (!_scopedContexts.ContainsKey(scopedContext))
            {
                return false;
            }
            if (interceptor.ScopedOption == ScopedOptions.None)
            {
                return true;
            }
            var currentContexts = GetCurrentScopedContexts(scopedContext).ToArray();
            if (currentContexts.Length == 0)
            {
                return true;
            }
            if (interceptor.ScopedOption == ScopedOptions.OnlyNested)
            {
                var preContext = currentContexts[currentContexts.Length - 1];
                return !TryIncludeImpl(preContext);
            }

            foreach (var current in currentContexts)
                if (TryIncludeImpl(current))
                    return false;

            return true;

            IEnumerable<AspectContext> GetCurrentScopedContexts(ScopedAspectContext ctx)
            {
                foreach (var current in GetCurrentContexts().OfType<ScopedAspectContext>().OrderBy(x => x.Id))
                {
                    if (current == ctx)
                        break;
                    yield return current;
                }
            }

            bool TryIncludeImpl(AspectContext ctx)
            {
                return _interceptorCollector.
                    Collect(ctx.ServiceMethod).
                    Where(x => x.GetType() == interceptor.GetType()).
                    Any(x => TryInclude(ctx, interceptor));
            }
        }

        public void Release(AspectContext context)
        {
            if (_scopedContexts.TryRemove(context as ScopedAspectContext, out _))
            {
                Interlocked.Decrement(ref _idx);
            }
        }
    }
}