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
        private readonly ConcurrentDictionary<AspectContext, int> _entries = new ConcurrentDictionary<AspectContext, int>();
        private readonly IInterceptorCollector _interceptorCollector;
        private int _version;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interceptorCollector"></param>
        public ScopeAspectScheduler(IInterceptorCollector interceptorCollector)
        {
            _interceptorCollector = interceptorCollector ?? throw new ArgumentNullException(nameof(interceptorCollector));
        }

        /// <summary>
        /// 获取当前拦截上下文
        /// </summary>
        /// <returns>拦截上下文数组</returns>
        public AspectContext[] GetCurrentContexts()
        {
            return _entries.OrderBy(x => x.Value).Select(x => x.Key).ToArray();
        }

        /// <summary>
        /// 为拦截上下文增加唯一版本号
        /// </summary>
        /// <param name="context">拦截上下文</param>
        /// <returns>是否成功</returns>
        public bool TryEnter(AspectContext context)
        {
            return _entries.TryAdd(context, Interlocked.Increment(ref _version));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="interceptor"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void Release(AspectContext context)
        {
            if(_entries.TryRemove(context, out _))
            {
                Interlocked.Decrement(ref _version);
            }
        }   
    }
}