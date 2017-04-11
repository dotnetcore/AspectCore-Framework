namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IAspectConfigureFactory<TContainer>
    {
        IAspectConfigure CreateConfigure(TContainer container);
    }
}
