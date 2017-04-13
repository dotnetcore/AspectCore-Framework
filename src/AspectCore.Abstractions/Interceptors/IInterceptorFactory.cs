using System;
using System.Reflection;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IInterceptorFactory
    {
        Predicate<MethodInfo> Predicate { get; }

        IInterceptor CreateInstance(IServiceProvider serviceProvider);
    }
}
