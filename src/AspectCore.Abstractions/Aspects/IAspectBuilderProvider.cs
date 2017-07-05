namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IAspectBuilderProvider
    {
        IAspectBuilder GetBuilder(AspectContext context);
    }
}
