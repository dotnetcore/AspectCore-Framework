using AspectCore.DynamicProxy;

namespace AspectCore.Injector
{
    [NonAspect]
    public interface IScopeResolverFactory
    {
        IServiceResolver CreateScope();
    }
}