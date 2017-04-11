using System.Reflection;

namespace AspectCore.Abstractions
{
    public interface IAspectValidationHandler
    {
        int Order { get; }

        bool Invoke(MethodInfo method, AspectValidationDelegate next);
    }
}
