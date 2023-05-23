using AspectCore.Configuration;
using System;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public sealed class AspectActivatorFactory : IAspectActivatorFactory
    {
        private readonly IAspectContextFactory _aspectContextFactory;
        private readonly IAspectBuilderFactory _aspectBuilderFactory;
        private readonly IAspectConfiguration _aspectConfiguration;

        public AspectActivatorFactory(IAspectContextFactory aspectContextFactory, IAspectBuilderFactory aspectBuilderFactory, IAspectConfiguration aspectConfiguration)
        {
            _aspectContextFactory = aspectContextFactory ?? throw new ArgumentNullException(nameof(aspectContextFactory));
            _aspectBuilderFactory = aspectBuilderFactory ?? throw new ArgumentNullException(nameof(aspectBuilderFactory));
            _aspectConfiguration = aspectConfiguration ?? throw new ArgumentNullException(nameof(aspectConfiguration));
        }

        public IAspectActivator Create()
        {
            return new AspectActivator(_aspectContextFactory, _aspectBuilderFactory, _aspectConfiguration);
        }
    }
}