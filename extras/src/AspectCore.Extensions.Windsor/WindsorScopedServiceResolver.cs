using System;
using AspectCore.DynamicProxy;
using AspectCore.Injector;
using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle.Scoped;

namespace AspectCore.Extensions.Windsor
{
    [NonAspect]
    internal sealed class WindsorScopedServiceResolver : IServiceResolver
    {
        private readonly IKernel _kernel;
        private readonly ILifetimeScope _scope;

        public WindsorScopedServiceResolver(IKernel kernel, ILifetimeScope scope)
        {
            _kernel = kernel;
            _scope = scope;
        }

        public void Dispose()
        {
            _scope.Dispose();
        }

        public object GetService(Type serviceType)
        {
            return _kernel.Resolve(serviceType);
        }

        public object Resolve(Type serviceType)
        {
            return _kernel.Resolve(serviceType);
        }
    }
}