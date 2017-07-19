using System.Reflection;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IAspectValidator
    {
        bool Validate(MethodInfo method);
    }
}
