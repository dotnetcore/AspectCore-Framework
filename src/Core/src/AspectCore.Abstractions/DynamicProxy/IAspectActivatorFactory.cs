namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public interface IAspectActivatorFactory
    {
        IAspectActivator Create();
    }
}