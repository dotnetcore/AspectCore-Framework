using AspectCore.Abstractions;

namespace AspectCore.Extensions.ScopedContext
{
    [NonAspect]
    public interface IScopedInterceptor : IInterceptor
    {
        ScopedOptions ScopedOption { get; set; }
    }
}