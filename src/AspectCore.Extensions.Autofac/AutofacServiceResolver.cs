using System;
using AspectCore.DynamicProxy;
using AspectCore.DependencyInjection;
using Autofac;
#if NET8_0_OR_GREATER
using Autofac.Core;
using Autofac.Core.Registration;
#endif

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
            return _componentContext.ResolveOptionalService(new KeyedService(serviceKey, serviceType));
        }

        public object GetRequiredKeyedService(Type serviceType, object serviceKey)
        {
            try
            {
                return _componentContext.ResolveService(new KeyedService(serviceKey, serviceType));
            }
            catch (ComponentNotRegisteredException ex)
            {
                throw new InvalidOperationException($"No service for type '{serviceType}' and key '{serviceKey}' has been registered.", ex);
            }
        }
#endif
    }
}