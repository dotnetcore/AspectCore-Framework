namespace AspectCore.Abstractions
{
    public interface IAspectConfigureProvider
    {
        IAspectConfigure AspectConfigure { get; }
    }
}
