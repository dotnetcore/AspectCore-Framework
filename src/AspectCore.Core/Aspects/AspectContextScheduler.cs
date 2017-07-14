using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    public sealed class AspectContextScheduler : IAspectContextScheduler
    {
        private readonly ConcurrentDictionary<ScopedAspectContext, object> _scopedContexts = new ConcurrentDictionary<ScopedAspectContext, object>();
        private readonly IInterceptorProvider _interceptorProvider;
        private int _idx = 0;

        public AspectContextScheduler(IInterceptorProvider interceptorProvider)
        {
            _interceptorProvider = interceptorProvider ?? throw new ArgumentNullException(nameof(interceptorProvider));
        }

        public AspectContext[] GetCurrentContexts()
        {
            return _scopedContexts.Keys.Where(x => x.RtContext != null).ToArray();
        }

        public bool TryEnter(ScopedAspectContext context)
        {
            context.Id = Interlocked.Increment(ref _idx);
            return _scopedContexts.TryAdd(context, null);
        }

        public bool TryInclude(ScopedAspectContext context, IInterceptor interceptor)
        {
            if (interceptor == null || context == null)
            {
                return false;
            }
            if (!_scopedContexts.ContainsKey(context))
            {
                return false;
            }
            if (interceptor.ScopedOption == ScopedOptions.None)
            {
                return true;
            }
            var currentContexts = GetCurrentScopedContexts(context).ToArray();
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

            IEnumerable<ScopedAspectContext> GetCurrentScopedContexts(ScopedAspectContext ctx)
            {
                foreach (var current in GetCurrentContexts().OfType<ScopedAspectContext>().OrderBy(x => x.Id))
                {
                    if (current == ctx)
                        break;
                    yield return current;
                }
            }

            bool TryIncludeImpl(ScopedAspectContext ctx)
            {
                return _interceptorProvider.
                    GetInterceptors(ctx.Target.ServiceMethod).
                    Where(x => x.GetType() == interceptor.GetType()).
                    Any(x => TryInclude(ctx, interceptor));
            }
        }

        public void Release(ScopedAspectContext context)
        {
            if (_scopedContexts.TryRemove(context, out object value))
            {
                Interlocked.Decrement(ref _idx);
            }
        }
    }
}