using System;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    [NonAspect]
    public interface IPropertyInjector
    {
        void Invoke(IServiceProvider serviceProvider, IInterceptor interceptor);
    }
}
