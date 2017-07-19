using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.ScopedContext
{
    public sealed class ScopedAspectContextScheduler : IAspectContextScheduler
    {
        private readonly ConcurrentDictionary<ScopedAspectContext, object> _scopedContexts = new ConcurrentDictionary<ScopedAspectContext, object>();
        private readonly IInterceptorProvider _interceptorProvider;
        private int _idx = 0;

        public ScopedAspectContextScheduler(IInterceptorProvider interceptorProvider)
        {
            _interceptorProvider = interceptorProvider ?? throw new ArgumentNullException(nameof(interceptorProvider));
        }

        public AspectContext[] GetCurrentContexts()
        {
            return _scopedContexts.Keys.Where(x => x.RtContext != null).ToArray();
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

        public bool TryInclude(AspectContext context, IInterceptor interceptor)
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
                return _interceptorProvider.
                    GetInterceptors(ctx.Target.ServiceMethod).
                    Where(x => x.GetType() == interceptor.GetType()).
                    Any(x => TryInclude(ctx, interceptor));
            }
        }

        public void Release(AspectContext context)
        {
            if (_scopedContexts.TryRemove(context as ScopedAspectContext, out object value))
            {
                Interlocked.Decrement(ref _idx);
            }
        }
    }
}