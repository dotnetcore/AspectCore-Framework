using System.Reflection;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IAspectValidationHandler
    {
        int Order { get; }

        bool Invoke(MethodInfo method, AspectValidationDelegate next);
    }
}
