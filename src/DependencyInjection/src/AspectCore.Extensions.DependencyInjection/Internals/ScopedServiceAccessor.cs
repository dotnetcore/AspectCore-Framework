using System;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection
{
    internal sealed class ScopedServiceAccessor<T> : IScopedServiceAccessor<T>
    {
        private readonly IServiceScopeAccessor _scopedServiceProviderAccessor;
        private readonly IServiceProvider _currentServiceProvider;

        private IServiceProvider CurrentScopedServiceProvider
        {
            get
            {
                return _scopedServiceProviderAccessor.CurrentServiceScope?.ServiceProvider ?? _currentServiceProvider;
            }
        }

        public T Value => CurrentScopedServiceProvider.GetService<T>();

        public T RequiredValue => CurrentScopedServiceProvider.GetRequiredService<T>();

        public ScopedServiceAccessor(IServiceScopeAccessor scopedServiceProviderAccessor, IServiceProvider serviceProvider)
        {
            _scopedServiceProviderAccessor = scopedServiceProviderAccessor ?? throw new ArgumentNullException(nameof(scopedServiceProviderAccessor));
            _currentServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }
    }
}
