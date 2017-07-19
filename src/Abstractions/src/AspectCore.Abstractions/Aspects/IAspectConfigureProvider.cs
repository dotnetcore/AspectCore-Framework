namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IAspectConfigureProvider
    {
        IAspectConfigure AspectConfigure { get; }
    }
}
