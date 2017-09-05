namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public interface IAspectBuilderFactory
    {
        IAspectBuilder Create(AspectContext context);
    }
}
