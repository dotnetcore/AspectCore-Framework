namespace AspectCore.Injector
{
    public interface IScopeResolverFactory
    {
        IServiceResolver CreateScope();
    }
}