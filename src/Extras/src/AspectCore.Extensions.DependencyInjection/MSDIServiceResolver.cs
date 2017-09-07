using System;
using AspectCore.Injector;

namespace AspectCore.Extensions.DependencyInjection
{
    internal class MSDIServiceResolver : IServiceResolver
    {
        private readonly IServiceProvider _serviceProvider;
        public MSDIServiceResolver(IServiceProvider serviceProvider)
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
    }
}