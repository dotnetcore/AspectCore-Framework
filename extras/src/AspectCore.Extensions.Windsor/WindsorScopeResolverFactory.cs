using System;
using AspectCore.DynamicProxy;
using AspectCore.Injector;
using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle.Scoped;

namespace AspectCore.Extensions.Windsor
{
    [NonAspect]
    internal sealed class WindsorScopeResolverFactory : IScopeResolverFactory
    {
        private readonly IKernel _kernel;

        public WindsorScopeResolverFactory(IKernel kernel)
        {
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        }

        public IServiceResolver CreateScope()
        {
            return new WindsorScopedServiceResolver(_kernel, new DefaultLifetimeScope());
        }
    }
}