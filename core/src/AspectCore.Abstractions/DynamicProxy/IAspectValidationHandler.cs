using System.Reflection;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public interface IAspectValidationHandler
    {
        int Order { get; }

        bool Invoke(AspectValidationContext context, AspectValidationDelegate next);
    }
}
