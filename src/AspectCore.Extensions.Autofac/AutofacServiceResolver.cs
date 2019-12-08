using System;
using AspectCore.DynamicProxy;
using AspectCore.DependencyInjection;
using Autofac;

namespace AspectCore.Extensions.Autofac
{
    [NonAspect]
    internal class AutofacServiceResolver : IServiceResolver
    {
        private readonly IComponentContext _componentContext;

        public AutofacServiceResolver(IComponentContext componentContext)
        {
            _componentContext = componentContext;
        }

        public void Dispose()
        {
            var d = _componentContext as IDisposable;
            d?.Dispose();
        }

        public object GetService(Type serviceType)
        {
            return _componentContext.ResolveOptional(serviceType);
        }

        public object Resolve(Type serviceType)
        {
            return _componentContext.ResolveOptional(serviceType);
        }
    }
}