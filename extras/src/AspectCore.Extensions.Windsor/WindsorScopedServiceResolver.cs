using System;
using AspectCore.DynamicProxy;
using AspectCore.Injector;
using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle.Scoped;
using Castle.Windsor;

namespace AspectCore.Extensions.Windsor
{
    [NonAspect]
    internal sealed class WindsorScopedServiceResolver : IServiceResolver
    {
        private readonly IKernelInternal _kernel;
        private readonly ILifetimeScope _scope;

        public WindsorScopedServiceResolver(IWindsorContainer windsorContainer, ILifetimeScope scope)
        {
            _kernel = windsorContainer?.Kernel as IKernelInternal;
            if (_kernel == null)
                throw new ArgumentException(string.Format("The kernel must implement {0}", typeof(IKernelInternal)));
            _scope = scope;
        }

        public void Dispose()
        {
            _scope.Dispose();
        }

        public object GetService(Type serviceType)
        {
            return Resolve(serviceType);
        }

        public object Resolve(Type serviceType)
        {
            if (_kernel.LoadHandlerByType(null, serviceType, null) != null)
                return _kernel.Resolve(serviceType);
            return null;
        }
    }
}