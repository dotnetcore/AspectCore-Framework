using System;
using System.Reflection;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IInterceptorFactory
    {
        bool CanCreated(MethodInfo method);

        IInterceptor CreateInstance(IServiceProvider serviceProvider);
    }
}
