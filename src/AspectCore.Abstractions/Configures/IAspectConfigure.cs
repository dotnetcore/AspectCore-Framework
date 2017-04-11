namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IAspectConfigure
    {
        IAspectConfigureOption<TOption> GetConfigureOption<TOption>();
    }
}
