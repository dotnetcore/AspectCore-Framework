using System;
using AspectCore.DynamicProxy;
using AspectCore.DependencyInjection;
using Autofac;
using Autofac.Core;
using Autofac.Core.Lifetime;

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

#if NET8_0_OR_GREATER
        public object GetKeyedService(Type serviceType, object serviceKey)
        {
            if (serviceKey is null)
            {
                return _componentContext.ResolveOptional(serviceType);
            }
            return _componentContext.ResolveKeyed(serviceKey, serviceType);
        }

        public object GetRequiredKeyedService(Type serviceType, object serviceKey)
        {
            if (serviceKey is null)
            {
                return _componentContext.Resolve(serviceType);
            }
            return _componentContext.ResolveKeyed(serviceKey, serviceType);
        }
#endif
    }
}