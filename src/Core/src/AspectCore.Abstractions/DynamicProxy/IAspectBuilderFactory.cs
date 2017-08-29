namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IAspectBuilderFactory
    {
        IAspectBuilder Create(AspectContext context);
    }
}
