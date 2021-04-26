using System;
using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.AspectScope
{
    /// <summary>
    /// 构建具作用域的拦截管道的生成器
    /// </summary>
    [NonAspect]
    public sealed class ScopeAspectBuilderFactory : IAspectBuilderFactory
    {
        private readonly IInterceptorCollector _interceptorCollector;
        private readonly IAspectScheduler _aspectContextScheduler;

        /// <summary>
        /// 构建有作用域的拦截管道
        /// </summary>
        /// <param name="interceptorProvider">拦截器收集器</param>
        /// <param name="aspectContextScheduler">拦截上下文调度器</param>
        public ScopeAspectBuilderFactory(IInterceptorCollector interceptorProvider, IAspectScheduler aspectContextScheduler)
        {
            _interceptorCollector = interceptorProvider ?? throw new ArgumentNullException(nameof(interceptorProvider));
            _aspectContextScheduler = aspectContextScheduler ?? throw new ArgumentNullException(nameof(aspectContextScheduler));
        }

        /// <summary>
        /// 构建有作用域的拦截管道
        /// </summary>
        /// <param name="context">拦截上下文</param>
        /// <returns>构建拦截管道</returns>
        public IAspectBuilder Create(AspectContext context)
        {
            var aspectBuilder = new AspectBuilder(ctx => ctx.Complete(), null);

            foreach (var interceptor in _interceptorCollector.Collect(context.ServiceMethod, context.ImplementationMethod))
            {
                if (interceptor is IScopeInterceptor scopedInterceptor)
                {
                    if (!_aspectContextScheduler.TryRelate(context, scopedInterceptor))
                        continue;
                }
                aspectBuilder.AddAspectDelegate(interceptor.Invoke);
            }

            return aspectBuilder;
        }
    }
}