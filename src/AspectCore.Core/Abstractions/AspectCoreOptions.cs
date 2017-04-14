namespace AspectCore.Abstractions
{
    public sealed class AspectCoreOptions
    {
        public InterceptorFactoryCollection InterceptorFactories { get; }

        public NonAspectOptionCollection NonAspectOptions { get; }
    }
}