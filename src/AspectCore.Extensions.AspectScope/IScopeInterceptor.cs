using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.AspectScope
{
    [NonAspect]
    public interface IScopeInterceptor : IInterceptor
    {
        Scope Scope { get; set; }
    }
}