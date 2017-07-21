using AspectCore.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AspectCore.Extensions.DependencyInjection.Internals
{
    [Dynamically]
    public sealed class AspectCoreServiceScopeFactory : IServiceScopeFactory
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IServiceScopeAccessor _scopedServiceProviderAccessor;

        public IServiceScope CreateScope()
        {
            var scopedserviceProvider = _serviceScopeFactory.CreateScope().ServiceProvider;
            var aspectCoreServiceProvider = new AspectCoreServiceProvider(scopedserviceProvider);
            return new AspectCoreServiceScope(aspectCoreServiceProvider, _scopedServiceProviderAccessor);
        }

        public AspectCoreServiceScopeFactory(IRealServiceProvider serviceProvider, IServiceScopeAccessor scopedServiceProviderAccessor)
        {
            _serviceScopeFactory = serviceProvider?.GetService<IServiceScopeFactory>() ?? throw new ArgumentNullException("serviceScopeFactory");
            _scopedServiceProviderAccessor = scopedServiceProviderAccessor ?? throw new ArgumentNullException(nameof(scopedServiceProviderAccessor));
        }
    }
}