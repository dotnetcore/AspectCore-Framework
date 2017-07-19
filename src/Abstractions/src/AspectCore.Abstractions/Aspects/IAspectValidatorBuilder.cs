namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IAspectValidatorBuilder
    {
        IAspectValidator Build();
    }
}
