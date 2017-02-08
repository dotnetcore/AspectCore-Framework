using System;

namespace AspectCore.Abstractions.Internal
{
    [NonAspect]
    public interface IPropertyInjector
    {
        void Invoke(IServiceProvider serviceProvider, IInterceptor interceptor);
    }
}
