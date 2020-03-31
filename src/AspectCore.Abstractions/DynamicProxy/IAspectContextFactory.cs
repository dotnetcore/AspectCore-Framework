namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public interface IAspectContextFactory
    {
        AspectContext CreateContext(AspectActivatorContext activatorContext);

        void ReleaseContext(AspectContext aspectContext);
    }
}
