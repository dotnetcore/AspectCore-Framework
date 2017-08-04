namespace AspectCore.Abstractions.IoC
{
    public interface IScopedResolverFactory
    {
        IServiceResolver CreateScope();
    }
}
