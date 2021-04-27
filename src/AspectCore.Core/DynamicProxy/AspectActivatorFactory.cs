using System;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public sealed class AspectActivatorFactory : IAspectActivatorFactory
    {
        private readonly IAspectContextFactory _aspectContextFactory;
        private readonly IAspectBuilderFactory _aspectBuilderFactory;
        private readonly IAspectExceptionWrapper _aspectExceptionWrapper;

        /// <summary>
        /// 创建AspectActivator对象的工厂
        /// </summary>
        /// <param name="aspectContextFactory">拦截上下文工厂</param>
        /// <param name="aspectBuilderFactory">拦截管道构建者工厂</param>
        /// <param name="aspectExceptionWrapper">拦截异常包装器</param>
        public AspectActivatorFactory(IAspectContextFactory aspectContextFactory, IAspectBuilderFactory aspectBuilderFactory, IAspectExceptionWrapper aspectExceptionWrapper)
        {
            _aspectContextFactory = aspectContextFactory ?? throw new ArgumentNullException(nameof(aspectContextFactory));
            _aspectBuilderFactory = aspectBuilderFactory ?? throw new ArgumentNullException(nameof(aspectBuilderFactory));
            _aspectExceptionWrapper = aspectExceptionWrapper ?? throw new ArgumentNullException(nameof(aspectExceptionWrapper));
        }

        /// <summary>
        /// 创建AspectActivator对象
        /// </summary>
        /// <returns>执行拦截管道的对象</returns>
        public IAspectActivator Create()
        {
            return new AspectActivator(_aspectContextFactory, _aspectBuilderFactory, _aspectExceptionWrapper);
        }
    }
}