using System;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection.Internals
{
    internal sealed class TransientServiceAccessor<T> : ITransientServiceAccessor<T>
    {
        private readonly IServiceProvider _serviceProvider;

        public T Value => _serviceProvider.GetService<T>();

        public T RequiredValue => _serviceProvider.GetRequiredService<T>();

        public TransientServiceAccessor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }
    }
}
