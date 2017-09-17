using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspectCore.Injector;
using Autofac;

namespace AspectCore.Extensions.Autofac
{
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