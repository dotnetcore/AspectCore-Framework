using System;
using AspectCore.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection
{
    internal class MsdiServiceResolver : IServiceResolver
    {
        private readonly IServiceProvider _serviceProvider;
        public MsdiServiceResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Dispose()
        {
            var d = _serviceProvider as IDisposable;
            d?.Dispose();
        }

        public object GetService(Type serviceType)
        {
            return _serviceProvider.GetService(serviceType);
        }

        public object Resolve(Type serviceType)
        {
            return _serviceProvider.GetService(serviceType);
        }

#if NET8_0_OR_GREATER
        public object GetKeyedService(Type serviceType, object serviceKey)
        {
            throw new NotImplementedException();
        }

        public object GetRequiredKeyedService(Type serviceType, object serviceKey)
        {
            throw new NotImplementedException();
        }
#endif
    }
}
