namespace AspectCore.Extensions.Reflection
{
    public interface IParameterReflectorProvider
    {
        ParameterReflector[] ParameterReflectors { get; }
    }
}