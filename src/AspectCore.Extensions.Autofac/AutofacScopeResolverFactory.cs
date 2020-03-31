using AspectCore.DynamicProxy;
using AspectCore.DependencyInjection;
using Autofac;

namespace AspectCore.Extensions.Autofac
{
    [NonAspect]
    internal class AutofacScopeResolverFactory : IScopeResolverFactory
    {
        private readonly ILifetimeScope _lifetimeScope;

        public AutofacScopeResolverFactory(ILifetimeScope lifetimeScope)
        {
            this._lifetimeScope = lifetimeScope;
        }

        public IServiceResolver CreateScope()
        {
            return new AutofacServiceResolver(_lifetimeScope.BeginLifetimeScope());
        }
    }
}