namespace AspectCore.Extensions.Reflection
{
    public interface ICustomAttributeReflectorProvider
    {
        CustomAttributeReflector[] CustomAttributeReflectors { get; }
    }
}