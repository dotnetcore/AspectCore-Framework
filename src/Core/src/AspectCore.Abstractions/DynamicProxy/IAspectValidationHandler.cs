using System.Reflection;

namespace AspectCore.DynamicProxy
{ 
    public interface IAspectValidationHandler
    {
        int Order { get; }

        bool Invoke(MethodInfo method, AspectValidationDelegate next);
    }
}
