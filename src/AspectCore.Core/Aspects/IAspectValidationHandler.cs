using System.Reflection;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    [NonAspect]
    public interface IAspectValidationHandler
    {
        int Order { get; }

        bool Invoke(MethodInfo method, AspectValidationDelegate next);
    }
}
