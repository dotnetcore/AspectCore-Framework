using System;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 拦截上下文工厂
    /// </summary>
    [NonAspect]
    public sealed class AspectContextFactory : IAspectContextFactory
    {
        private static readonly object[] emptyParameters = new object[0];
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 拦截上下文工厂
        /// </summary>
        /// <param name="serviceProvider">IServiceProvider</param>
        public AspectContextFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// 创建一个拦截上下文
        /// </summary>
        /// <param name="activatorContext">用于切面调用过程内部传递切面的上下文信息</param>
        /// <returns>拦截上下文</returns>
        public AspectContext CreateContext(AspectActivatorContext activatorContext)
        {
            return new RuntimeAspectContext(_serviceProvider,
                activatorContext.ServiceMethod,
                activatorContext.TargetMethod,
                activatorContext.ProxyMethod,
                activatorContext.TargetInstance,
                activatorContext.ProxyInstance,
                activatorContext.Parameters ?? emptyParameters);
        }

        /// <summary>
        /// 释放拦截上下文对象资源
        /// </summary>
        /// <param name="aspectContext">拦截上下文</param>
        public void ReleaseContext(AspectContext aspectContext)
        {
            (aspectContext as IDisposable)?.Dispose();
        }
    }
}