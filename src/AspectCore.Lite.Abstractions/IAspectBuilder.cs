namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface IAspectBuilder
    {
        void AddAspectDelegate(AspectDelegate aspectDelegate);

        AspectDelegate Build(AspectDelegate targetInvokeDelegate);
    }
}
