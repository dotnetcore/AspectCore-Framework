namespace AspectCore.Abstractions
{
    [NonAspect]
    public sealed class AspectCoreOptions
    {
        public InterceptorFactoryCollection InterceptorFactories { get; } = new InterceptorFactoryCollection();

        public NonAspectOptionCollection NonAspectOptions { get; } = new NonAspectOptionCollection();
    }
}