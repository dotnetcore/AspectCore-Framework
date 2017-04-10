using System.Reflection;

namespace AspectCore.Abstractions.Internal
{
    public sealed class AspectValidator : IAspectValidator
    {
        private readonly AspectValidationDelegate _aspectValidationDelegate;

        public AspectValidator(AspectValidationDelegate aspectValidationDelegate)
        {
            _aspectValidationDelegate = aspectValidationDelegate;
        }

        public bool Validate(MethodInfo method)
        {
            if (method == null)
            {
                return false;
            }

            return _aspectValidationDelegate(method);
        }
    }
}