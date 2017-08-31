namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IResolverScopeFactory
    {
        IResolverScope CreateScope();
    }
}