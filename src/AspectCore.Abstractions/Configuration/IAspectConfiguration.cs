namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IAspectConfiguration
    {
        IConfigurationOption<TOption> GetConfigurationOption<TOption>();
    }
}
