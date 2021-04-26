using System;
using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.AspectScope
{
    /// <summary>
    /// 此工厂创建具有作用域范围的拦截上下文
    /// </summary>
    [NonAspect]
    public sealed class ScopeAspectContextFactory : IAspectContextFactory
    {
        private readonly IAspectScheduler _aspectScheduler;
        private readonly AspectContextFactory _aspectContextFactory;

        /// <summary>
        /// 此工厂创建具有作用域范围的拦截上下文
        /// </summary>
        /// <param name="serviceProvider">IServiceProvider</param>
        /// <param name="aspectContextScheduler">拦截上下文调度器</param>
        public ScopeAspectContextFactory(IServiceProvider serviceProvider, IAspectScheduler aspectContextScheduler)
        {
            _aspectScheduler = aspectContextScheduler ?? throw new ArgumentNullException(nameof(aspectContextScheduler));
            _aspectContextFactory = new AspectContextFactory(serviceProvider);
        }

        /// <summary>
        /// 创建具有作用域的拦截上下文
        /// </summary>
        /// <param name="activatorContext">用于切面调用过程内部传递切面上下文信息</param>
        /// <returns>拦截上下文</returns>
        public AspectContext CreateContext(AspectActivatorContext activatorContext)
        {
            var aspectContext = _aspectContextFactory.CreateContext(activatorContext);
            if (!_aspectScheduler.TryEnter(aspectContext))
            {
                throw new InvalidOperationException("Error occurred in the schedule AspectContext.");
            }
            return aspectContext;
        }

        /// <summary>
        /// 释放上下文
        /// </summary>
        /// <param name="aspectContext">拦截上下文</param>
        public void ReleaseContext(AspectContext aspectContext)
        {
            _aspectContextFactory.ReleaseContext(aspectContext);
            _aspectScheduler.Release(aspectContext);
        }
    }
}