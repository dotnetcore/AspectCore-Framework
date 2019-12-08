using System;

namespace AspectCore.DependencyInjection
{
    public sealed class TransientServiceAccessor<T> : ITransientServiceAccessor<T> where T : class
    {
        private readonly IServiceProvider _serviceProvider;
        public T Value => (T)_serviceProvider.GetService(typeof(T));

        public TransientServiceAccessor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }
    }
}