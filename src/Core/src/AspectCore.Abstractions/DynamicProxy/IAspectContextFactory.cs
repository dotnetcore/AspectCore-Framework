namespace AspectCore.DynamicProxy
{
    public interface IAspectContextFactory
    {
        AspectContext CreateContext(AspectActivatorContext activatorContext);
    }
}
