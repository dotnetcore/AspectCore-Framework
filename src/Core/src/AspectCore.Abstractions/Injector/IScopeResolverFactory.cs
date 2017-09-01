namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IScopeResolverFactory
    {
        IServiceResolver CreateScope();
    }
}