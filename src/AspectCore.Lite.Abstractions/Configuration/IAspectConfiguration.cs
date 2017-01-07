using AspectCore.Lite.Abstractions.Attributes;

namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface IAspectConfiguration
    {
        IConfigurationOption<TOption> GetConfigurationOption<TOption>();
    }
}
