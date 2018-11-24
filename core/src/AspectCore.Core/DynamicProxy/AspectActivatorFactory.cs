using System;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public sealed class AspectActivatorFactory : IAspectActivatorFactory
    {
        private readonly IAspectContextFactory _aspectContextFactory;
        private readonly IAspectBuilderFactory _aspectBuilderFactory;
        private readonly IAspectExceptionWrapper _aspectExceptionWrapper;

        public AspectActivatorFactory(IAspectContextFactory aspectContextFactory, IAspectBuilderFactory aspectBuilderFactory, IAspectExceptionWrapper aspectExceptionWrapper)
        {
            _aspectContextFactory = aspectContextFactory ?? throw new ArgumentNullException(nameof(aspectContextFactory));
            _aspectBuilderFactory = aspectBuilderFactory ?? throw new ArgumentNullException(nameof(aspectBuilderFactory));
            _aspectExceptionWrapper = aspectExceptionWrapper ?? throw new ArgumentNullException(nameof(aspectExceptionWrapper));
        }

        public IAspectActivator Create()
        {
            return new AspectActivator(_aspectContextFactory, _aspectBuilderFactory, _aspectExceptionWrapper);
        }
    }
}