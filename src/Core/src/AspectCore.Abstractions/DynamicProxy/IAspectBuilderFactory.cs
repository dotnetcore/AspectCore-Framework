namespace AspectCore.DynamicProxy
{
    public interface IAspectBuilderFactory
    {
        IAspectBuilder Create(AspectContext context);
    }
}
