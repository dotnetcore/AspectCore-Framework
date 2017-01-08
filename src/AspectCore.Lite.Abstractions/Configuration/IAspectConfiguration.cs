namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface IAspectConfiguration
    {
        IConfigurationOption<TOption> GetConfigurationOption<TOption>();
    }
}
