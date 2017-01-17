namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IAspectConfigurationFactory<TContainer>
    {
        IAspectConfiguration CreateConfiguration(TContainer container);
    }
}
