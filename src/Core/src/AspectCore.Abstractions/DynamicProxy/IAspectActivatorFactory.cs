namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IAspectActivatorFactory
    {
        IAspectActivator Create();
    }
}