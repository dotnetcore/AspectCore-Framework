namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IAspectBuilderProvider
    {
        IAspectBuilder GetBuilder(AspectActivatorContext context);
    }
}
