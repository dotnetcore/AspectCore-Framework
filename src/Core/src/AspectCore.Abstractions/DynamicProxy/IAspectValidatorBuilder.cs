namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public interface IAspectValidatorBuilder
    {
        IAspectValidator Build();
    }
}
