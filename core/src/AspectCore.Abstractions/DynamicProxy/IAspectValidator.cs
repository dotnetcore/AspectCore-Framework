using System.Reflection;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public interface IAspectValidator
    {
        bool Validate(MethodInfo method, bool isStrictValidation);
    }
}