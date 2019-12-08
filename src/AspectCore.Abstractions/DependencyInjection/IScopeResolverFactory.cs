using AspectCore.DynamicProxy;

namespace AspectCore.DependencyInjection
{
    [NonAspect]
    public interface IScopeResolverFactory
    {
        IServiceResolver CreateScope();
    }
}