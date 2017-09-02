using System.Reflection;

namespace AspectCore.DynamicProxy
{
    public interface IAspectValidator
    {
        bool Validate(MethodInfo method);
    }
}