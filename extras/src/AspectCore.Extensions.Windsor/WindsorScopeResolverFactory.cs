using System;
using AspectCore.DynamicProxy;
using AspectCore.Injector;
using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle.Scoped;
using Castle.Windsor;

namespace AspectCore.Extensions.Windsor
{
    [NonAspect]
    internal sealed class WindsorScopeResolverFactory : IScopeResolverFactory
    {
        private readonly IWindsorContainer _windsorContainer;

        public WindsorScopeResolverFactory(IWindsorContainer  windsorContainer)
        {
            _windsorContainer = windsorContainer ?? throw new ArgumentNullException(nameof(windsorContainer));
        }

        public IServiceResolver CreateScope()
        {
            return new WindsorScopedServiceResolver(_windsorContainer, new DefaultLifetimeScope());
        }
    }
}