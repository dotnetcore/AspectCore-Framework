using System;

namespace AspectCore.Abstractions.Resolution.Internal
{
    [NonAspect]
    public interface IPropertyInjector
    {
        void Invoke(IServiceProvider serviceProvider, IInterceptor interceptor);
    }
}
