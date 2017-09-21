using System;
using AspectCore.DynamicProxy;
using AspectCore.Injector;
using Castle.MicroKernel;
using Castle.Windsor;

namespace AspectCore.Extensions.Windsor
{
    [NonAspect]
    public sealed class WindsorServiceResolver : IServiceResolver
    {
        private readonly IKernel _kernel;

        public WindsorServiceResolver(IKernel kernel)
        {
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        }

        public WindsorServiceResolver(IWindsorContainer windsorContainer)
            :this(windsorContainer.Kernel)
        {
        }

        public void Dispose()
        {
            var d = _kernel as IDisposable;
            d?.Dispose();
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