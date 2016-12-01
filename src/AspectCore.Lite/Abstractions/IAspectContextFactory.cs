namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface IAspectContextFactory
    {
        IAspectContext Create();
    }
}
